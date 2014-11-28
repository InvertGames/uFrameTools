using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Common;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity
{
    public class UnityPlatform :IPlatformOperations
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
        public void DrawPolyLine(Vector3[] lines)
        {
            Handles.DrawPolyLine(lines);
        }

        public void DrawBezier(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent,
            Color color, float width)
        {
            Handles.DrawBezier(startPosition,endPosition,startTangent,endTangent,color,null,width);
        }

        public void DrawConnector(float scale, ConnectorViewModel viewModel)
        {
            
        }
    }

}
