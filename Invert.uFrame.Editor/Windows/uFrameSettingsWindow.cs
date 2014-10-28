using Invert.Common;
using Invert.Common.UI;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;
using UnityEngine;

public class uFrameSettingsWindow : EditorWindow
{

    [MenuItem("Tools/[u]Frame/Settings")]
    internal static void ShowWindow()
    {
        var window = GetWindow<uFrameSettingsWindow>();
        window.title = "uFrame Settings";
        window.minSize = new Vector2(400, 500);

        window.Show();
    }

    private void OnEnable()
    {
        //minSize = new Vector2(520, 400);
        //maxSize = new Vector2(520, 400);
        position = new Rect(position.x, position.y, 240, 300);
    }

    public static void DrawTitleBar(string subTitle)
    {
        //GUI.Label();
        ElementDesignerStyles.DoTilebar(subTitle);
    }

    public void OnGUI()
    {
        var s = uFrameEditor.Settings;
        DrawTitleBar("uFrame Settings");
        if (GUIHelpers.DoToolbarEx("Color Settings"))
        {
            s.BackgroundColor = EditorGUILayout.ColorField("Background Color", s.BackgroundColor);
            s.UseGrid = EditorGUILayout.BeginToggleGroup("Grid", s.UseGrid);
           
                s.GridLinesColor = EditorGUILayout.ColorField("Grid Lines Color", s.GridLinesColor);
                s.GridLinesColorSecondary = EditorGUILayout.ColorField("Grid Lines Secondary Color", s.GridLinesColorSecondary);    
            EditorGUILayout.EndToggleGroup();
            

            //s.AssociationLinkColor = EditorGUILayout.ColorField("Association Link Color", s.AssociationLinkColor);
            //s.DefinitionLinkColor = EditorGUILayout.ColorField("Definition Link Color", s.DefinitionLinkColor);
            //s.InheritanceLinkColor = EditorGUILayout.ColorField("Inheritance Link Color", s.InheritanceLinkColor);
            //s.SubSystemLinkColor = EditorGUILayout.ColorField("SubSystem Link Color", s.SubSystemLinkColor);
            //s.TransitionLinkColor = EditorGUILayout.ColorField("Transition Link Color", s.TransitionLinkColor);
            //s.ViewLinkColor = EditorGUILayout.ColorField("View Link Color", s.ViewLinkColor);
        }
        if (GUIHelpers.DoToolbarEx("Plugins"))
        {
            foreach (var plugin in uFrameEditor.Container.ResolveAll<IDiagramPlugin>())
            {
                if (
                    GUIHelpers.DoTriggerButton(new UFStyle("     " + plugin.Title, UBStyles.EventButtonStyleSmall,
                        null,
                        plugin.Enabled ? UBStyles.TriggerActiveButtonStyle : UBStyles.TriggerInActiveButtonStyle, () => { }, false, TextAnchor.MiddleCenter)
                    {
                        IsWindow = true,
                        FullWidth = true
                    }))
                {
                    plugin.Enabled = !plugin.Enabled;
                    uFrameEditor.Container = null;
                }
            }
        }
       
    }
}