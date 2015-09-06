using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Invert.Common;
using Mono.CSharp;
using UnityEditor;
using UnityEngine;
using Enum = System.Enum;
using Event = UnityEngine.Event;

namespace Invert.Core.GraphDesigner.Unity
{
    public class UnityPlatform : IPlatformOperations, IDebugLogger
    {
        public void OpenScriptFile(string filePath)
        {
            var scriptAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset));
            AssetDatabase.OpenAsset(scriptAsset);
        }

        public void OpenLink(string link)
        {
            Application.OpenURL(link);
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
            //AssetDatabase.Refresh();
        }

        public void Progress(float progress, string message)
        {
            try
            {
                InvertApplication.SignalEvent<ITaskProgressHandler>(_=>_.Progress(progress, message));
                //if (progress > 100f)
                //{
                //    EditorUtility.ClearProgressBar();
                //    return;
                //}
                //EditorUtility.DisplayProgressBar("Generating", message, progress/1f);
            }
            catch (Exception ex)
            {
                
            }
        }

        public void Log(string message)
        {
            Debug.Log(message); 
        }

        public void LogException(Exception ex)
        {
            Debug.LogException(ex);
            if (ex.InnerException != null)
            {
                Debug.LogException(ex.InnerException);
            }
        }
    }

    public interface ITaskProgressHandler
    {
        void Progress(float progress, string message);
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
            EditorPrefs.SetString(name, value);
        }

        public void SetFloat(string name, float value)
        {
            EditorPrefs.SetFloat(name, value);
        }

        public void SetInt(string name, int value)
        {

            EditorPrefs.SetInt(name, value);
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
        public override void AddSeparator()
        {
            base.AddSeparator();
            Commands.Add(new ContextMenuItem() {Title = string.Empty});
        }

        public void CreateMenuItems(GenericMenu genericMenu)
        {
            var groups = Commands.GroupBy(p => p==null? "" : p.Group).OrderBy(p => p.Key == "Default").ToArray();

            foreach (var group in groups)
            {

                //genericMenu.AddDisabledItem(new GUIContent(group.Key));
                var groupCount = 0;
                foreach (var editorCommand in group.OrderBy(p => p.Order))
                {
                    ICommand command = editorCommand.Command;
                    genericMenu.AddItem(new GUIContent(editorCommand.Title),editorCommand.Checked, () =>
                    {
                        InvertApplication.Execute(command);
                    } );
                  
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
               
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            foreach (var editorCommand in LeftCommands.OrderBy(p => p.Order))
            {
                DoCommand(editorCommand);
            }
            GUILayout.FlexibleSpace();


            foreach (var editorCommand in RightCommands.OrderBy(p => p.Order))
            {
         
                DoCommand(editorCommand);
            }
            GUILayout.EndHorizontal();
        }

        public override void GoBottom()
        {
            base.GoBottom();
                 GUILayout.BeginHorizontal(EditorStyles.toolbar);
            //var scale = GUILayout.HorizontalSlider(ElementDesignerStyles.Scale, 0.8f, 1f, GUILayout.Width(200f));
            //if (scale != ElementDesignerStyles.Scale)
            //{
            //    ElementDesignerStyles.Scale = scale;
            //    InvertGraphEditor.ExecuteCommand(new ScaleCommand() { Scale = scale });

            //}
            foreach (var editorCommand in BottomLeftCommands.OrderBy(p => p.Order))
            {
                
                DoCommand(editorCommand);
            }
            GUILayout.FlexibleSpace();
            foreach (var editorCommand in BottomRightCommands.OrderBy(p => p.Order))
            {
                DoCommand(editorCommand);
            }
              GUILayout.EndHorizontal();
        }
        public void DoCommand(ToolbarItem command)
        {

            var style = EditorStyles.toolbarButton;
            if (command.IsDropdown)
            {
                style = EditorStyles.toolbarDropDown;
            }
            if (command.Checked)
            {
                style = new GUIStyle(EditorStyles.toolbarButton);
                style.normal.background = style.active.background;
            }
            
            var guiContent = new GUIContent(command.Title);
            if (GUILayout.Button(guiContent, style))
            {
                InvertApplication.Execute(command.Command);
            }
            InvertGraphEditor.PlatformDrawer.SetTooltipForRect(GUILayoutUtility.GetLastRect(),command.Description);
            
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

        // TODO DRAWER Eliminate Vector3 convertion
        public void DrawPolyLine(Vector2[] lines, Color color)
        {
            Handles.color = color;
            Handles.DrawPolyLine(lines.Select(x => new Vector3(x.x, x.y, 0)).ToArray()); 
        }

        public void DrawLine(Vector3[] lines, Color color)
        {
            Handles.color = color;
            Handles.DrawPolyLine(lines); 
        }

        private string currentTooltip;
        
        public void SetTooltipForRect(Rect rect, string tooltip)
        {
            bool isMouseOver = rect.Contains(Event.current.mousePosition);
            if (isMouseOver) currentTooltip = tooltip;
        }

        public string GetTooltip()
        {
            return currentTooltip;
        }

        public void ClearTooltip()
        {
            currentTooltip = null;
        }


        // TODO DRAWER Beziers with texture
        public void DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent,
            Color color, float width)
        {
            Handles.color = color;
            Handles.DrawBezier(startPosition, endPosition, startTangent, endTangent, color, null, width);
        }

        // TODO DRAWER Add Scale parameter
        public Vector2 CalculateTextSize(string text, object styleObject)
        {
            var style = ((GUIStyle) styleObject);
            return style.CalcSize(new GUIContent(text));
        }

        // TODO DRAWER Add Scale parameter
        public float CalculateTextHeight(string text, object styleObject, float width)
        {
            var style = (GUIStyle) styleObject;
            return style.CalcHeight(new GUIContent(text), width);
        }

        public Vector2 CalculateImageSize(string imageName)
        {
            var image = ElementDesignerStyles.GetSkinTexture(imageName);
            if (image != null)
            {
                return new Vector2(image.width, image.height);
            }
            return Vector2.zero;
        }

        // TODO DRAWER Add tooltip parameter
        public void DrawLabel(Rect rect, string label, object style, DrawingAlignment alignment = DrawingAlignment.MiddleLeft)
        {
            var guiStyle = (GUIStyle)style;
            var oldAlignment = guiStyle.alignment;
            guiStyle.alignment = ((TextAnchor)(int)alignment);
            GUI.Label(rect, label, guiStyle);
            guiStyle.alignment = oldAlignment;
        }

        // TODO DRAWER Add tooltip parameter | Change the way it is done and separate icon from icon
        public void DrawLabelWithIcon(Rect rect, string label, string iconName, object style,
      DrawingAlignment alignment = DrawingAlignment.MiddleLeft)
        {
            var s = (GUIStyle)style;
            s.alignment = ((TextAnchor)(int)alignment);
            //GUI.Label(rect, label, s);
            GUI.Label(rect, new GUIContent(label,ElementDesignerStyles.GetSkinTexture(iconName)), s);
        }


        public void DrawStretchBox(Rect scale, object nodeBackground, float offset)
        {
            DrawExpandableBox(scale, nodeBackground , string.Empty, offset);
        }

        
        public void DrawStretchBox(Rect scale, object nodeBackground, Rect offset)
        {
            //var rectOffset = new RectOffset(Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y), Mathf.RoundToInt(offset.width), Mathf.RoundToInt(offset.height));
            
            var rectOffset = new RectOffset((int)offset.x, (int)offset.y, (int)offset.width, (int)offset.height);
            
            DrawExpandableBox(scale, (GUIStyle)nodeBackground,
               string.Empty, rectOffset);
        }

        public void DrawExpandableBox(Rect rect, object style, string text, float offset = 12)
        {
            var guiStyle = (GUIStyle)style;
            var oldBorder = guiStyle.border;
            guiStyle.border = new RectOffset(
                (int)(offset),
                (int)(offset),
                (int)(offset),
                (int)(offset));
            GUI.Box(rect, text, guiStyle);
            guiStyle.border = oldBorder;
        }

        public void DrawExpandableBox(Rect rect, object style, string text, RectOffset offset)
        {
            var guiStyle = (GUIStyle)style;
            var oldBorder = guiStyle.border;
            GUI.Box(rect, text, guiStyle);
            guiStyle.border = oldBorder;
        }

        //TODO DRAWER introduce Tooptip parameter
        public void DoButton(Rect scale, string label, object style, Action action, Action rightClick = null)
        {
            var s = style == null ? ElementDesignerStyles.EventSmallButtonStyle : (GUIStyle)style;
       
            if (GUI.Button(scale, label, s))
            {
                if (Event.current.button == 0) action();
                else
                {
                    if (rightClick != null)
                    rightClick();
                }
            }
        }

        //TODO DRAWER introduce tooltip param
        public void DoButton(Rect scale, string label, object style, Action<Vector2> action, Action<Vector2> rightClick = null)
        {
            var s = style == null ? ElementDesignerStyles.EventSmallButtonStyle : (GUIStyle)style;

            if (GUI.Button(scale, label, s))
            {
                var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - new Vector2(0, 22);
                if (Event.current.button == 0) action(mousePos); 
                else
                {
                    if (rightClick != null)
                        rightClick(mousePos);
                }
            }
        }

        public void DrawWarning(Rect rect, string key)
        {
            EditorGUI.HelpBox(rect, key, MessageType.Warning);
        }

        public void DrawError(Rect rect, string key)
        {
            EditorGUI.HelpBox(rect, key, MessageType.Error);
        }

        public void DrawInfo(Rect rect, string key)
        {
            EditorGUI.HelpBox(rect, key, MessageType.Info);
        }

        public void DrawImage(Rect bounds, string texture, bool b)
        {
            DrawImage(bounds, Styles.Image(texture), b);
        }

        public void DrawImage(Rect bounds, object texture, bool b)
        {
            GUI.DrawTexture(bounds, texture as Texture2D, ScaleMode.ScaleToFit, true);
        }

        //TODO DRAWER Introduce tooltip parameter
        public void DrawPropertyField(PropertyFieldViewModel fieldViewModel, float scale)
        {
            //base.Draw(scale);
            GUILayout.BeginArea(fieldViewModel.Bounds.Scale(scale), ElementDesignerStyles.SelectedItemStyle);
            EditorGUIUtility.labelWidth = fieldViewModel.Bounds.width * 0.55f;
            DrawInspector(fieldViewModel);
            GUILayout.EndArea();
        }

 

        public void EndRender()
        {
            EditorGUI.FocusTextInControl("EditingField");            
        }

        public void DrawRect(Rect boundsRect, Color color)
        {
            EditorGUI.DrawRect(boundsRect,color);
        }

        public void DrawNodeHeader(Rect boxRect, object backgroundStyle, bool isCollapsed, float scale, object image)
        {
            if(image != null) (backgroundStyle as GUIStyle).ForAllStates(image as Texture2D);

            Rect adjustedBounds;
            if (isCollapsed)
            {
                //adjustedBounds = new Rect(boxRect.x - 9, boxRect.y + 1, boxRect.width + 19, boxRect.height + 9);
                adjustedBounds = new Rect(boxRect.x, boxRect.y, boxRect.width, (boxRect.height)*scale);
                DrawStretchBox(adjustedBounds, backgroundStyle, 20 * scale);
            }
            else
            {
                //adjustedBounds = new Rect(boxRect.x - 9, boxRect.y + 1, boxRect.width + 19, boxRect.height-6 * scale);
                adjustedBounds = new Rect(boxRect.x, boxRect.y, boxRect.width, (boxRect.height)*scale);
                DrawStretchBox(adjustedBounds,
                     backgroundStyle,
                     new Rect(20 * scale, 20 * scale, 35 * scale, 22 * scale)
                );
            }
        }

        public void DoToolbar(Rect toolbarTopRect, DesignerWindow designerWindow, ToolbarPosition position)
        {
            if (designerWindow == null) throw new ArgumentNullException("designerWindow");
        
            if (designerWindow.Toolbar == null) throw new ArgumentNullException("designerWindow.Toolbar");
            GUILayout.BeginArea(toolbarTopRect);
            if (toolbarTopRect.y > 20)
            {
                designerWindow.Toolbar.GoBottom();
            }
            else
            {
                designerWindow.Toolbar.Go();
            }
            GUILayout.EndArea();
        }

        public void DoTabs(Rect tabsRect, DesignerWindow designerWindow)
        {
            EditorGUI.DrawRect(tabsRect, InvertGraphEditor.Settings.BackgroundColor);
            var color = new Color(InvertGraphEditor.Settings.BackgroundColor.r*0.8f,
                InvertGraphEditor.Settings.BackgroundColor.g*0.8f, InvertGraphEditor.Settings.BackgroundColor.b*0.8f);
            EditorGUI.DrawRect(tabsRect, color);
            if (designerWindow != null && designerWindow.Designer != null)
            {
                GUILayout.BeginArea(tabsRect);
                GUILayout.BeginHorizontal();
                
                foreach (var tab in designerWindow.Designer.Tabs.ToArray())
                {
                    if (tab == null) continue;
                    if (tab.Name == null)
                        continue;
                    var isCurrent = designerWindow.Workspace != null && designerWindow.Workspace.CurrentGraph != null && tab.Identifier == designerWindow.Workspace.CurrentGraph.Identifier;
                    if (GUILayout.Button(tab.Name,
                        isCurrent
                            ? ElementDesignerStyles.TabStyle
                            : ElementDesignerStyles.TabInActiveStyle,GUILayout.MinWidth(150)))
                    {
                        var projectService = InvertGraphEditor.Container.Resolve<WorkspaceService>();
                   
                        if (Event.current.button == 1)
                        {
                         
                           var isLastGraph = projectService.CurrentWorkspace.Graphs.Count() <= 1;

                           if (!isLastGraph)
                            {
                                var tab1 = tab;
                                projectService.Repository.RemoveAll<WorkspaceGraph>(p=>p.WorkspaceId == projectService.CurrentWorkspace.Identifier && p.GraphId == tab1.Identifier);
                                var lastGraph = projectService.CurrentWorkspace.Graphs.LastOrDefault();
                                if (isCurrent && lastGraph != null)
                                {
                                    designerWindow.SwitchDiagram(lastGraph);
                                }
                            
                            }
                        }
                        else
                        {
                            designerWindow.SwitchDiagram(projectService.CurrentWorkspace.Graphs.FirstOrDefault(p => p.Identifier == tab.Identifier));    
                        }
                        
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        public void DisableInput()
        {
            if (!GUI.enabled) return;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, GUI.color.a*2);
            GUI.enabled = false;
        }

        public void EnableInput()
        {
            if (GUI.enabled) return;
            GUI.enabled = true;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, GUI.color.a/2);
        }

        public void BeginRender(object sender, MouseEvent mouseEvent)
        {
            DiagramDrawer.IsEditingField = false;
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
        public void DrawColumns(Rect rect, float[] columnWidths, params Action<Rect>[] columns)
        {
            var x = 0f;
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
            //if (DiagramDrawer.IsEditingField)
            //{
                //GUI.FocusControl("EditingField");
            //}
            //else
            //{
            //}
        }

        public void DrawTextbox(string id, Rect rect, string value, object itemTextEditingStyle, Action<string, bool> valueChangedAction)
        {
            //EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("EditingField");
            DiagramDrawer.IsEditingField = true;
            var newName = EditorGUI.TextField(rect, value, (GUIStyle)itemTextEditingStyle);

            //if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
            //{
                valueChangedAction(newName, false);
            //}
            if (Event.current.keyCode == KeyCode.Return)
            {
                valueChangedAction(value, true);
            }
            EditorGUI.FocusTextInControl("EditingField");
        }

        public void DrawingStarted()
        {

        }

        public virtual void DrawInspector(PropertyFieldViewModel d)
        {
            var colorCache = GUI.color;
            GUI.color = Color.white;
            if (d.InspectorType == InspectorType.GraphItems)
            {
                var item = d.CachedValue as IGraphItem;
                var text = "--Select--";
                if (item != null)
                {
                    text = item.Label;
                }
                //GUILayout.BeginHorizontal();
               
                if (GUILayout.Button(d.Label + ": " + text,ElementDesignerStyles.ButtonStyle))
                {
                    var type = d.Type;

                    var items = InvertGraphEditor.CurrentDiagramViewModel.CurrentRepository.AllOf<IGraphItem>().Where(p => type.IsAssignableFrom(p.GetType()));

                    var menu = new SelectionMenu();

                    foreach (var graphItem in items)
                    {
                        menu.AddItem(new SelectionMenuItem(graphItem, () =>
                        {
                            InvertApplication.Execute(() =>
                            {
                                d.Setter(graphItem);
                            });
                        }));
                    }

                    InvertApplication.SignalEvent<IShowSelectionMenu>(_=>_.ShowSelectionMenu(menu));



//
//                    InvertGraphEditor.WindowManager.InitItemWindow(items, 
//                        
//                    },true);

                }
                SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);

                GUI.color = colorCache;
                //GUILayout.EndHorizontal();
                return;
            }
       

            if (d.Type == typeof(string))
            {
                if (d.InspectorType == InspectorType.TextArea)
                {
                    EditorGUILayout.LabelField(d.Name);
                    SetTooltipForRect(GUILayoutUtility.GetLastRect(),d.InspectorTip);
                    

                    EditorGUI.BeginChangeCheck();
                    d.CachedValue = EditorGUILayout.TextArea((string)d.CachedValue, GUILayout.Height(50));
                    if (EditorGUI.EndChangeCheck())
                    {
                        d.Setter(d.CachedValue);
                        
                    }
                    if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                    {
                        InvertApplication.Execute(() =>
                        {
                            
                        });
                    }
                }
                else if (d.InspectorType == InspectorType.TypeSelection)
                {
                    GUILayout.BeginHorizontal();
                    //GUILayout.Label(d.ViewModel.Name);

                    if (GUILayout.Button((string)d.CachedValue))
                    {
                        d.NodeViewModel.Select();
                        // TODO 2.0 Open Selection?
                    }
                    SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);
                    

                    GUILayout.EndHorizontal();
                }

                else
                {
                    EditorGUI.BeginChangeCheck();
                    d.CachedValue = EditorGUILayout.TextField(d.Name, (string)d.CachedValue);
                    SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        d.Setter(d.CachedValue);
                    }
                }

            }
            else if (d.Type == typeof(int))
            {
                EditorGUI.BeginChangeCheck();
                d.CachedValue = EditorGUILayout.IntField(d.Name, (int)d.CachedValue);
                SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);

                if (EditorGUI.EndChangeCheck())
                {
                    d.Setter(d.CachedValue);
                }
            }
            else if (d.Type == typeof(float))
            {
                EditorGUI.BeginChangeCheck();
                d.CachedValue = EditorGUILayout.FloatField(d.Name, (float)d.CachedValue);
                SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);

                if (EditorGUI.EndChangeCheck())
                {
                    d.Setter(d.CachedValue);
                }
            }
            else if (d.Type == typeof(Vector2))
            {
                EditorGUI.BeginChangeCheck();
                d.CachedValue = EditorGUILayout.Vector2Field(d.Name, (Vector3)d.CachedValue);
                SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);

                if (EditorGUI.EndChangeCheck())
                {
                    d.Setter(d.CachedValue);
                }
            }

            else if (d.Type == typeof(Vector3))
            {
                EditorGUI.BeginChangeCheck();
                d.CachedValue = EditorGUILayout.Vector3Field(d.Name, (Vector3)d.CachedValue);
                SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);

                if (EditorGUI.EndChangeCheck())
                {
                    d.Setter(d.CachedValue);
                }

            }
            else if (d.Type == typeof(Color))
            {
                EditorGUI.BeginChangeCheck();
                d.CachedValue = EditorGUILayout.ColorField(d.Name, (Color)d.CachedValue);
                SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);

                if (EditorGUI.EndChangeCheck())
                {
                    d.Setter(d.CachedValue);
                }

            }
            else if (d.Type == typeof(Vector4))
            {
                EditorGUI.BeginChangeCheck();
                d.CachedValue = EditorGUILayout.Vector4Field(d.Name, (Vector4)d.CachedValue);
                SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);

                if (EditorGUI.EndChangeCheck())
                {
                    d.Setter(d.CachedValue);
                }
            }
            else if (d.Type == typeof(bool))
            {
                EditorGUI.BeginChangeCheck();
                d.CachedValue = EditorGUILayout.Toggle(d.Name, (bool)d.CachedValue);
                SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);

                if (EditorGUI.EndChangeCheck())
                {
                    d.Setter(d.CachedValue);
                }
            }
            else if (typeof(Enum).IsAssignableFrom(d.Type))
            {
                EditorGUI.BeginChangeCheck();
                d.CachedValue = EditorGUILayout.EnumPopup(d.Name, (Enum)d.CachedValue);
                SetTooltipForRect(GUILayoutUtility.GetLastRect(), d.InspectorTip);

                if (EditorGUI.EndChangeCheck())
                {
                    InvertApplication.Execute(() =>
                    {
                        d.Setter(d.CachedValue);
                    });
                    
                }
            }
            else if (d.Type == typeof(Type))
            {
                //InvertGraphEditor.WindowManager.InitTypeListWindow();
            }

            GUI.color = colorCache;

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
            


        }

        public Texture2D Image(string name)
        {
            return (Texture2D)GetImage(name);
        }
        public object GetImage(string name)
        {
            if (Textures.ContainsKey(name))
                return Textures[name];
            return ElementDesignerStyles.GetSkinTexture(name);
        }

        public object GetStyle(InvertStyles name)
        {
            if (Textures.Count < 1)
            {
                Textures.Add("DiagramArrowRight", ElementDesignerStyles.ArrowRightTexture);
                Textures.Add("DiagramArrowLeft", ElementDesignerStyles.ArrowLeftTexture);
                Textures.Add("DiagramArrowUp", ElementDesignerStyles.ArrowUpTexture);
                Textures.Add("DiagramArrowDown", ElementDesignerStyles.ArrowDownTexture);
                Textures.Add("DiagramArrowRightEmpty", ElementDesignerStyles.ArrowRightEmptyTexture);
                Textures.Add("DiagramArrowLeftEmpty", ElementDesignerStyles.ArrowLeftEmptyTexture);
                Textures.Add("DiagramCircleConnector", ElementDesignerStyles.DiagramCircleConnector);
            }
            switch (name)
            {
                case InvertStyles.DefaultLabel:
                    return EditorStyles.label;
                case InvertStyles.DefaultLabelLarge:
                    return EditorStyles.largeLabel;
                case InvertStyles.Tag1:
                    return ElementDesignerStyles.Tag1;         
                case InvertStyles.ListItemTitleStyle:
                    return ElementDesignerStyles.ListItemTitleStyle;
                case InvertStyles.BreadcrumbBoxStyle:
                    return ElementDesignerStyles.BreadcrumbBoxStyle;            
                case InvertStyles.BreadcrumbBoxActiveStyle:
                    return ElementDesignerStyles.BreadcrumbBoxActiveStyle;
                case InvertStyles.BreadcrumbTitleStyle:
                    return ElementDesignerStyles.BreadcrumbTitleStyle;                
                case InvertStyles.WizardBox:
                    return ElementDesignerStyles.WizardBoxStyle;      
                case InvertStyles.WizardSubBox:
                    return ElementDesignerStyles.WizardSubBoxStyle;          
                case InvertStyles.WizardActionButton:
                    return ElementDesignerStyles.WizardActionButtonStyle;
                case InvertStyles.WizardActionTitle:
                    return ElementDesignerStyles.WizardActionTitleStyle;     
                case InvertStyles.WizardSubBoxTitle:
                    return ElementDesignerStyles.WizardSubBoxTitleStyle;
                case InvertStyles.TooltipBox:
                    return ElementDesignerStyles.TooltipBoxStyle;
                case InvertStyles.WizardListItemBox:
                    return ElementDesignerStyles.WizardListItemBoxStyle;
                case InvertStyles.TabBox:
                    return ElementDesignerStyles.TabBoxStyle;       
                case InvertStyles.SearchBarText:
                    return ElementDesignerStyles.SearchBarTextStyle;
                case InvertStyles.TabCloseButton:
                    return ElementDesignerStyles.TabCloseButtonStyle;
                case InvertStyles.TabBoxActive:
                    return ElementDesignerStyles.TabBoxActiveStyle;
                case InvertStyles.TabTitle:
                    return ElementDesignerStyles.TabTitleStyle;
                case InvertStyles.NodeBackground:
                    return ElementDesignerStyles.NodeBackground;           
                case InvertStyles.NodeBackgroundBorderless:
                    return ElementDesignerStyles.NodeBackgroundBorderless;
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
                    return ElementDesignerStyles.HeaderStyle;
                case InvertStyles.ViewModelHeaderStyle:
                    return ElementDesignerStyles.ViewModelHeaderStyle;
                case InvertStyles.AddButtonStyle:
                    return ElementDesignerStyles.AddButtonStyle;
                case InvertStyles.ItemTextEditingStyle:
                    return ElementDesignerStyles.ItemTextEditingStyle;
                case InvertStyles.GraphTitleLabel:
                    return ElementDesignerStyles.GraphTitleLabel;
            }


            return ElementDesignerStyles.ClearItemStyle;
        }


        private static Dictionary<string, Font> Fonts = new Dictionary<string, Font>();

        public object GetFont(string fontName)
        {
            if (string.IsNullOrEmpty(fontName)) return null;
            if (!Fonts.ContainsKey(fontName))
            {
                Fonts.Add(fontName, Resources.Load<Font>("fonts/" + fontName));
            }
            return Fonts[fontName];
        }

        internal struct IconTintItem
        {
            public string Name { get; set; }
            public Color Tint { get; set; }
        }

        private Dictionary<IconTintItem, object> IconCache = new Dictionary<IconTintItem, object>();

        public object GetIcon(string name, Color tint)
        {
            return null;
        }

        public INodeStyleSchema GetNodeStyleSchema(NodeStyle name)
        {
            switch (name)
            {
                case NodeStyle.Minimalistic:
                    return ElementDesignerStyles.NodeStyleSchemaMinimalistic;
                    break;
                case NodeStyle.Bold:
                    return ElementDesignerStyles.NodeStyleSchemaBold;
                    break;
                case NodeStyle.Normal:
                    return ElementDesignerStyles.NodeStyleSchemaNormal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("name", name, null);
            }
        }

        public IConnectorStyleSchema GetConnectorStyleSchema(ConnectorStyle name)
        {
            switch (name)
            {
                case ConnectorStyle.Triangle:
                    return ElementDesignerStyles.ConnectorStyleSchemaTriangle;
                    break;
                case ConnectorStyle.Circle:
                    return ElementDesignerStyles.ConnectorStyleSchemaCircle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("name", name, null);
            }
        }

        public IBreadcrumbsStyleSchema GetBreadcrumbStyleSchema(BreadcrumbsStyle name)
        {
            switch (name)
            {
                case BreadcrumbsStyle.Default:
                    return ElementDesignerStyles.DefaultBreadcrumbsStyleSchema;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("name", name, null);
            }
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
