﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Common;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity
{
    public class UnityPlatform :IPlatformOperations, IDebugLogger
    {
        public void OpenScriptFile(string filePath)
        {
            var scriptAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset));
            AssetDatabase.OpenAsset(scriptAsset);
        }

        public string GetAssetPath(object graphData)
        {
            return AssetDatabase.GetAssetPath(graphData as UnityEngine.Object);
        }
        public bool MessageBox(string title, string message, string ok)
        {
            return EditorUtility.DisplayDialog(title, message, ok);
        }
        public bool MessageBox(string title, string message, string ok, string cancel)
        {
            return EditorUtility.DisplayDialog(title, message, ok, cancel);
        }

        public void SaveAssets()
        {
            AssetDatabase.SaveAssets();
        }

        public void RefreshAssets()
        {
            AssetDatabase.Refresh();
        }

        public void Progress(float progress, string message)
        {
            if (progress > 100f)
            {
               EditorUtility.ClearProgressBar();
                return;
            }
            EditorUtility.DisplayProgressBar("Generating",message,progress / 1f);
        }

        public void Log(string message)
        {
            Debug.Log(message);
        }

        public void LogException(Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public class UnityPlatformPreferences : IPlatformPreferences
    {
        public bool GetBool(string name, bool def)
        {
            return EditorPrefs.GetBool(name, def);
        }

        public string GetString(string name, string def)
        {
            return EditorPrefs.GetString(name, def);
        }

        public float GetFloat(string name, float def)
        {
            return EditorPrefs.GetFloat(name, def);
        }

        public float GetInt(string name, int def)
        {
            
            return EditorPrefs.GetInt(name, def);
        }
        public void SetBool(string name, bool value)
        {
            EditorPrefs.SetBool(name, value);
        }

        public void SetString(string name, string value)
        {
            EditorPrefs.GetString(name, value);
        }

        public void SetFloat(string name, float value)
        {
            EditorPrefs.GetFloat(name, value);
        }

        public void SetInt(string name, int value)
        {

            EditorPrefs.GetInt(name, value);
        }

        public bool HasKey(string name)
        {
            return EditorPrefs.HasKey(name);
        }
        public void DeleteKey(string name)
        {
            EditorPrefs.DeleteKey(name);
        }
        public void DeleteAll()
        {
            EditorPrefs.DeleteAll();
        }
    
    }

    public class UnityContextMenu : ContextMenuUI
    {
        public void CreateMenuItems(GenericMenu genericMenu)
        {
            var groups = Commands.GroupBy(p => p.Group).OrderBy(p => p.Key == "Default").ToArray();

            foreach (var group in groups)
            {

                //genericMenu.AddDisabledItem(new GUIContent(group.Key));
                var groupCount = 0;
                foreach (var editorCommand in group.OrderBy(p => p.Order))
                {


                    IEditorCommand command = editorCommand;
                    var argument = Handler.ContextObjects.FirstOrDefault(p => p != null && command.For.IsAssignableFrom(p.GetType()));

                    var dynamicCommand = command as IDynamicOptionsCommand;
                    if (dynamicCommand != null)
                    {
                        var canPerform = command.CanPerform(argument);
                        if (canPerform == null)
                        {
                            foreach (var option in dynamicCommand.GetOptions(argument).OrderBy(p => p.Name))
                            {
                                groupCount++;
                                UFContextMenuItem option1 = option;
                                genericMenu.AddItem(new GUIContent(Flatten ? editorCommand.Title : option.Name),
                                    option.Checked, () =>
                                    {
                                        dynamicCommand.SelectedOption = option1;
                                        Handler.ExecuteCommand(command);
                                    });
                            }
                        }
                        else
                        {
                            if (command.ShowAsDiabled)
                                genericMenu.AddDisabledItem(new GUIContent((Flatten ? editorCommand.Title : editorCommand.Path) + " : " + canPerform));
                        }

                    }
                    else
                    {
                        var canPerform = command.CanPerform(argument);
                        if (canPerform != null)
                        {
                            if (command.ShowAsDiabled)
                                genericMenu.AddDisabledItem(new GUIContent((Flatten ? editorCommand.Title : editorCommand.Path) + " : " + canPerform));
                        }
                        else
                        {
                            groupCount++;
                            genericMenu.AddItem(new GUIContent(Flatten ? editorCommand.Title : editorCommand.Path), editorCommand.IsChecked(argument), () =>
                            {
                                Handler.ExecuteCommand(command);
                            });
                        }
                    }


                }
                if (group != groups.Last() && groupCount > 0)
                    genericMenu.AddSeparator("");
            }
        }

        public override void Go()
        {
            base.Go();
            var genericMenu = new GenericMenu();
            CreateMenuItems(genericMenu);
            genericMenu.ShowAsContext();
        }
    }

    public class UnityToolbar : ToolbarUI
    {
        public override void Go()
        {
            base.Go();
            foreach (var editorCommand in LeftCommands.OrderBy(p => p.Order))
            {
                DoCommand(editorCommand);
            }
            GUILayout.FlexibleSpace();


            foreach (var editorCommand in RightCommands.OrderBy(p => p.Order))
            {
                DoCommand(editorCommand);
            }
        }

        public override void GoBottom()
        {
            base.GoBottom();
            var scale = GUILayout.HorizontalSlider(ElementDesignerStyles.Scale, 0.8f, 1f, GUILayout.Width(200f));
            if (scale != ElementDesignerStyles.Scale)
            {
                ElementDesignerStyles.Scale = scale;
                Handler.ExecuteCommand(new ScaleCommand() { Scale = scale });

            }
            foreach (var editorCommand in BottomLeftCommands.OrderBy(p => p.Order))
            {
                DoCommand(editorCommand);
            }
            GUILayout.FlexibleSpace();
            foreach (var editorCommand in BottomRightCommands.OrderBy(p => p.Order))
            {
                DoCommand(editorCommand);
            }
        }
        public void DoCommand(IEditorCommand command)
        {

            var obj = Handler.ContextObjects.FirstOrDefault(p => command.For.IsAssignableFrom(p.GetType()));
            GUI.enabled = command.CanPerform(obj) == null;
            if (command is IDynamicOptionsCommand)
            {
                var cmd = command as IDynamicOptionsCommand;
                foreach (var ufContextMenuItem in cmd.GetOptions(obj))
                {
                    if (GUILayout.Button(new GUIContent(ufContextMenuItem.Name), EditorStyles.toolbarButton))
                    {
                        cmd.SelectedOption = ufContextMenuItem;
                        Handler.ExecuteCommand(command);
                    }
                }
            }
            else if (GUILayout.Button(new GUIContent(command.Title), EditorStyles.toolbarButton))
            {

                if (command is IParentCommand)
                {
                    var contextUI = InvertGraphEditor.CreateCommandUI<ContextMenuUI>(Handler, command.GetType());
                    contextUI.Flatten = true;
                    contextUI.Go();
                }
                else
                {
                    Handler.ExecuteCommand(command);
                }
            }
            GUI.enabled = true;
        }
    }


    public class UnityDrawer : IPlatformDrawer
    {
        private UnityStyleProvider _styles;

        public UnityStyleProvider Styles
        {
            get { return _styles ?? (_styles = new UnityStyleProvider()); }
            set { _styles = value; }
        }

        // TODO I HATE the vector3 conversion
        public void DrawPolyLine(Vector2[] lines)
        {
            Handles.DrawPolyLine(lines.Select(x=>new Vector3(x.x,x.y,0f)).ToArray());
        }

        public void DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent,
            Color color, float width)
        {
            Handles.DrawBezier(startPosition,endPosition,startTangent,endTangent,color,null,width);
        }

    
        public Vector2 CalculateSize(string text, object tag1)
        {
            return ((GUIStyle)tag1).CalcSize(new GUIContent(text));
        }

        public void DrawLabel(Rect rect, string label, object style, DrawingAlignment alignment = DrawingAlignment.MiddleLeft)
        {
            var s = (GUIStyle) style;
            s.alignment = ((TextAnchor) Enum.Parse(typeof (TextAnchor), alignment.ToString(), true));
            
           GUI.Label(rect,label,s);
        }

        public void DrawStretchBox(Rect scale, object nodeBackground, float offset)
        {
            ElementDesignerStyles.DrawExpandableBox(scale,(GUIStyle)nodeBackground,
                string.Empty, offset);
        }

        public void DrawStretchBox(Rect scale, object nodeBackground, Rect offset)
        {
            ElementDesignerStyles.DrawExpandableBox(scale, (GUIStyle)nodeBackground,
               string.Empty, new RectOffset(Mathf.RoundToInt(offset.x),Mathf.RoundToInt(offset.y),Mathf.RoundToInt(offset.width),Mathf.RoundToInt(offset.height)));

        }

        public void DoButton(Rect scale, string label, object style, Action action)
        {
            var s =(GUIStyle) style;
            s.alignment = TextAnchor.MiddleCenter;
             if (GUI.Button(scale, label,s))
             {
                 action();
             }
        }

        public void DrawWarning(Rect rect, string key)
        {
            EditorGUI.HelpBox(rect, key, MessageType.Warning);
        }

        public void DrawImage(Rect bounds, string texture, bool b)
        {
            GUI.DrawTexture(bounds, Styles.Image(texture), ScaleMode.StretchToFill, true);
        }


        public void DrawPropertyField(PropertyFieldDrawer d, float scale)
        {
            //base.Draw(scale);
            GUILayout.BeginArea(d.Bounds.Scale(scale), ElementDesignerStyles.SelectedItemStyle);
            EditorGUIUtility.labelWidth = d.Bounds.width * 0.55f;

            DrawInspector(d);
            GUILayout.EndArea();
        }

        public void EndRender()
        {
          
        }

  

        public void BeginRender(object sender, MouseEvent mouseEvent)
        {
         
        }

        public void DrawColumns(Rect rect, params Action<Rect>[] columns)
        {
            var columnsLength = columns.Length;
            var columnFactor = rect.width / columnsLength;

            for (int index = 0; index < columns.Length; index++)
            {
                var item = columns[index];
                var newRect = new Rect(rect.x + (index * columnFactor), rect.y, columnFactor, rect.height);
                item(newRect);
            }
        }
        public void DrawColumns(Rect rect, int[] columnWidths, params Action<Rect>[] columns)
        {
            var x = 0;
            for (int index = 0; index < columns.Length; index++)
            {
                var item = columns[index];
                if (index == columns.Length - 1)
                {
                    // Use the remaining width of this item
                    var width = rect.width - x;
                    var newRect = new Rect(rect.x + x, rect.y, width, rect.height);
                    item(newRect);
                }
                else
                {
                    var newRect = new Rect(rect.x + x, rect.y, columnWidths[index], rect.height);
                    item(newRect);
                    
                }
               
                x += columnWidths[index];
            }
        }

        public void DrawingComplete()
        {
            if (DiagramDrawer.IsEditingField)
            {
                //GUI.FocusControl("EditingField");

                EditorGUI.FocusTextInControl("EditingField");
                EditorGUI.FocusTextInControl("EditingField");
            }
            else
            {

            }
        }

        public void DrawTextbox(string id, Rect rect, string value, object itemTextEditingStyle, Action<string,bool> valueChangedAction)
        {
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("EditingField");
            DiagramDrawer.IsEditingField = true;
            var newName = EditorGUI.TextField(rect, value, (GUIStyle)itemTextEditingStyle);
           
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
            {
                valueChangedAction(newName, false);
            }
            if (Event.current.keyCode == KeyCode.Return)
            {
                valueChangedAction(value, true);
            }
        }

        public void DrawingStarted()
        {
            
        }

        public virtual void DrawInspector(PropertyFieldDrawer d,bool refreshGraph = false, float scale = 1f)
        {
            EditorGUI.BeginChangeCheck();
            if (d.CustomDrawer != null)
            {
                d.CachedValue = d.CustomDrawer.Draw(scale, d.ViewModel);
            }
            else
                if (d.ViewModel.Type == typeof(string))
                {
                    if (d.ViewModel.InspectorType == InspectorType.TextArea)
                    {
                        EditorGUILayout.LabelField(d.ViewModel.Name);
                        d.CachedValue = EditorGUILayout.TextArea((string)d.CachedValue, GUILayout.Height(50));
                    }
                    else if (d.ViewModel.InspectorType == InspectorType.TypeSelection)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(d.ViewModel.Name);

                        if (GUILayout.Button((string)d.CachedValue))
                        {
                            InvertGraphEditor.WindowManager.InitTypeListWindow(InvertApplication.GetDerivedTypes<System.Object>(true, true).Select(p => new GraphTypeInfo()
                            {
                                Name = p.Name,
                                Group = p.Namespace,
                                Label = p.Name,

                            }).ToArray()
                            , (type) =>
                            {
                                InvertGraphEditor.ExecuteCommand(x => d.ViewModel.Setter(type.Name));
                            });
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        d.CachedValue = EditorGUILayout.TextField(d.ViewModel.Name, (string)d.CachedValue);
                    }

                }
                else if (d.ViewModel.Type == typeof(int))
                {
                    d.CachedValue = EditorGUILayout.IntField(d.ViewModel.Name, (int)d.CachedValue);
                }
                else if (d.ViewModel.Type == typeof(float))
                {
                    d.CachedValue = EditorGUILayout.FloatField(d.ViewModel.Name, (float)d.CachedValue);
                }
                else if (d.ViewModel.Type == typeof(Vector2))
                {
                    d.CachedValue = EditorGUILayout.Vector2Field(d.ViewModel.Name, (Vector3)d.CachedValue);
                }

                else if (d.ViewModel.Type == typeof(Vector3))
                {
                    d.CachedValue = EditorGUILayout.Vector3Field(d.ViewModel.Name, (Vector3)d.CachedValue);

                }
                else if (d.ViewModel.Type == typeof(Color))
                {
                    d.CachedValue = EditorGUILayout.ColorField(d.ViewModel.Name, (Color)d.CachedValue);

                }
                else if (d.ViewModel.Type == typeof(Vector4))
                {
                    d.CachedValue = EditorGUILayout.Vector4Field(d.ViewModel.Name, (Vector4)d.CachedValue);
                }
                else if (d.ViewModel.Type == typeof(bool))
                {
                    d.CachedValue = EditorGUILayout.Toggle(d.ViewModel.Name, (bool)d.CachedValue);
                }
                else if (typeof(Enum).IsAssignableFrom(d.ViewModel.Type))
                {
                    d.CachedValue = EditorGUILayout.EnumPopup(d.ViewModel.Name, (Enum)d.CachedValue);
                }
                else if (d.ViewModel.Type == typeof(Type))
                {
                    //InvertGraphEditor.WindowManager.InitTypeListWindow();
                }

            if (EditorGUI.EndChangeCheck())
            {

                d.ViewModel.Setter(d.CachedValue);
                if (refreshGraph)
                {

                    InvertGraphEditor.DesignerWindow.RefreshContent();
                }
            }
        }
        public void DrawError(Rect rect, string key)
        {
            EditorGUI.HelpBox(rect, key, MessageType.Error);
        }
        public void DrawInfo(Rect rect, string key)
        {
            EditorGUI.HelpBox(rect, key, MessageType.Info);
        }

        public void DrawConnector(float scale, ConnectorViewModel viewModel)
        {
            
        }
    }

    public class UnityStyleProvider : IStyleProvider
    {
        private static Dictionary<string, Texture2D> _textures;

        public GUIStyle Style(InvertStyles style)
        {
            return (GUIStyle)GetStyle(style);
        }

        protected static Dictionary<string, Texture2D> Textures
        {
            get { return _textures ?? (_textures = new Dictionary<string, Texture2D>()); }
            set { _textures = value; }
        }

        static UnityStyleProvider()
        {
            Textures.Add("DiagramArrowRight", ElementDesignerStyles.ArrowRightEmptyTexture);
            Textures.Add("DiagramArrowLeft", ElementDesignerStyles.ArrowLeftTexture);
            Textures.Add("DiagramArrowUp", ElementDesignerStyles.ArrowUpTexture);
            Textures.Add("DiagramArrowDown", ElementDesignerStyles.ArrowDownTexture);
            Textures.Add("DiagramArrowRightEmpty", ElementDesignerStyles.ArrowRightEmptyTexture);
            Textures.Add("DiagramArrowLeftEmpty", ElementDesignerStyles.ArrowLeftEmptyTexture);
      
     
        }

        public Texture2D Image(string name)
        {
            return (Texture2D) GetImage(name);
        }
        public object GetImage(string name)
        {
            return Textures[name];
        }

        public object GetStyle(InvertStyles name)
        {
            switch (name)
            {
                case InvertStyles.DefaultLabel:
                    return EditorStyles.label;
                case InvertStyles.DefaultLabelLarge:
                    return EditorStyles.largeLabel;
               case InvertStyles.Tag1:
                    return ElementDesignerStyles.Tag1;
                case InvertStyles.NodeBackground:
                    return ElementDesignerStyles.NodeBackground;
                case InvertStyles.NodeExpand:
                    return ElementDesignerStyles.NodeExpand;
                case InvertStyles.NodeCollapse:
                    return ElementDesignerStyles.NodeCollapse;
                case InvertStyles.BoxHighlighter1:
                    return ElementDesignerStyles.BoxHighlighter1;
                case InvertStyles.BoxHighlighter2:
                    return ElementDesignerStyles.BoxHighlighter2;
                case InvertStyles.BoxHighlighter3:
                    return ElementDesignerStyles.BoxHighlighter3;
                case InvertStyles.BoxHighlighter4:
                    return ElementDesignerStyles.BoxHighlighter4;
                case InvertStyles.BoxHighlighter5:
                    return ElementDesignerStyles.BoxHighlighter5;
                case InvertStyles.BoxHighlighter6:
                    return ElementDesignerStyles.BoxHighlighter6;
                case InvertStyles.NodeHeader1:
                    return ElementDesignerStyles.NodeHeader1;
                case InvertStyles.NodeHeader2:
                    return ElementDesignerStyles.NodeHeader2;
                case InvertStyles.NodeHeader3:
                    return ElementDesignerStyles.NodeHeader3;
                case InvertStyles.NodeHeader4:
                    return ElementDesignerStyles.NodeHeader4;
                case InvertStyles.NodeHeader5:
                    return ElementDesignerStyles.NodeHeader5;
                case InvertStyles.NodeHeader6:
                    return ElementDesignerStyles.NodeHeader6;
                case InvertStyles.NodeHeader7:
                    return ElementDesignerStyles.NodeHeader7;
                case InvertStyles.NodeHeader8:
                    return ElementDesignerStyles.NodeHeader8;
                case InvertStyles.NodeHeader9:
                    return ElementDesignerStyles.NodeHeader9;
                case InvertStyles.NodeHeader10:
                    return ElementDesignerStyles.NodeHeader10;
                case InvertStyles.NodeHeader11:
                    return ElementDesignerStyles.NodeHeader11;
                case InvertStyles.NodeHeader12:
                    return ElementDesignerStyles.NodeHeader12;
                case InvertStyles.NodeHeader13:
                    return ElementDesignerStyles.NodeHeader13;
                case InvertStyles.Item1:
                    return ElementDesignerStyles.Item1;                
                case InvertStyles.Item2:
                    return ElementDesignerStyles.Item2;                
                case InvertStyles.Item3:
                    return ElementDesignerStyles.Item3;                
                case InvertStyles.Item4:
                    return ElementDesignerStyles.Item4;
                case InvertStyles.Item5:
                    return ElementDesignerStyles.Item5;
                case InvertStyles.Item6:
                    return ElementDesignerStyles.Item6;
                case InvertStyles.SelectedItemStyle:
                    return ElementDesignerStyles.SelectedItemStyle;      
                case InvertStyles.HeaderStyle:
                    return ElementDesignerStyles.HeaderStyle;   case InvertStyles.ViewModelHeaderStyle:
                    return ElementDesignerStyles.ViewModelHeaderStyle;
                case InvertStyles.AddButtonStyle:
                    return ElementDesignerStyles.AddButtonStyle;
                case InvertStyles.ItemTextEditingStyle:
                    return ElementDesignerStyles.ItemTextEditingStyle;
            }
            return ElementDesignerStyles.ClearItemStyle;
        }

        public object GetNodeHeaderStyle(NodeColor color)
        {
            switch (color)
            {
                case NodeColor.DarkGray:
                    return ElementDesignerStyles.NodeHeader1;
                case NodeColor.Blue:
                    return ElementDesignerStyles.NodeHeader2;
                case NodeColor.Gray:
                    return ElementDesignerStyles.NodeHeader3;
                case NodeColor.LightGray:
                    return ElementDesignerStyles.NodeHeader4;
                case NodeColor.Black:
                    return ElementDesignerStyles.NodeHeader5;
                case NodeColor.DarkDarkGray:
                    return ElementDesignerStyles.NodeHeader6;
                case NodeColor.Orange:
                    return ElementDesignerStyles.NodeHeader7;
                case NodeColor.Red:
                    return ElementDesignerStyles.NodeHeader8;
                case NodeColor.YellowGreen:
                    return ElementDesignerStyles.NodeHeader9;
                case NodeColor.Green:
                    return ElementDesignerStyles.NodeHeader10;
                case NodeColor.Purple:
                    return ElementDesignerStyles.NodeHeader11;
                case NodeColor.Pink:
                    return ElementDesignerStyles.NodeHeader12;
                case NodeColor.Yellow:
                    return ElementDesignerStyles.NodeHeader13;

            }
            return ElementDesignerStyles.NodeHeader1;
        }
    }

}
