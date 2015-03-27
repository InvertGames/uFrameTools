using System.Linq;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.Core.GraphDesigner;
using UnityEditor;
using UnityEngine;

public class uFrameSettingsWindow : EditorWindow
{

    [MenuItem("uFrame/Settings")]
    internal static void ShowWindow()
    {
        var window = GetWindow<uFrameSettingsWindow>();
        window.title = "uFrame Settings";
        window.minSize = new Vector2(400, 500);

        window.ShowUtility();
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
        var s = InvertGraphEditor.Settings;
        DrawTitleBar("uFrame Settings");
        if (s != null)
        {
            if (GUIHelpers.DoToolbarEx("Color Settings"))
            {
                s.BackgroundColor = EditorGUILayout.ColorField("Background Color", s.BackgroundColor);
                s.UseGrid = EditorGUILayout.BeginToggleGroup("Grid", s.UseGrid);

                s.GridLinesColor = EditorGUILayout.ColorField("Grid Lines Color", s.GridLinesColor);
                s.GridLinesColorSecondary = EditorGUILayout.ColorField("Grid Lines Secondary Color",
                    s.GridLinesColorSecondary);
                EditorGUILayout.EndToggleGroup();


                //s.AssociationLinkColor = EditorGUILayout.ColorField("Association Link Color", s.AssociationLinkColor);
                //s.DefinitionLinkColor = EditorGUILayout.ColorField("Definition Link Color", s.DefinitionLinkColor);
                //s.InheritanceLinkColor = EditorGUILayout.ColorField("Inheritance Link Color", s.InheritanceLinkColor);
                //s.SubSystemLinkColor = EditorGUILayout.ColorField("SubSystem Link Color", s.SubSystemLinkColor);
                //s.TransitionLinkColor = EditorGUILayout.ColorField("Transition Link Color", s.TransitionLinkColor);
                //s.ViewLinkColor = EditorGUILayout.ColorField("View Link Color", s.ViewLinkColor);
            }

        }
        else
        {
            EditorGUILayout.HelpBox("Settings not available.",MessageType.Info);
        }
       
        if (GUIHelpers.DoToolbarEx("Plugins - Enabled"))
        {
            foreach (var plugin in InvertApplication.Plugins.Where(p=>!p.Required && p.Enabled).OrderBy(p=>p.LoadPriority))
            {
                DoPlugin(plugin);
            }
          
        } if (GUIHelpers.DoToolbarEx("Plugins - Disabled"))
        {
            foreach (var plugin in InvertApplication.Plugins.Where(p=>!p.Required && !p.Enabled).OrderBy(p=>p.LoadPriority))
            {
                DoPlugin(plugin);
            }
          
        }
        if (GUIHelpers.DoToolbarEx("Registered Nodes"))
        {
            foreach (var plugin in InvertGraphEditor.Container.ResolveAll<NodeConfigBase>())
            {
                GUIHelpers.DoTriggerButton(new UFStyle(plugin.Name + " : " + plugin.NodeType.Name, ElementDesignerStyles.EventButtonStyleSmall)
                {
                    IsWindow = true,
                    FullWidth = true
                });
            }

        }
        if (GUIHelpers.DoToolbarEx("Loaded Assemblies"))
        {
            foreach (var plugin in InvertApplication.CachedAssemblies)
            {
                GUIHelpers.DoTriggerButton(new UFStyle(plugin.FullName, ElementDesignerStyles.EventButtonStyleSmall)
                {
                    IsWindow = true,
                    FullWidth = true
                });
            }

        }
       
    }

    private static void DoPlugin(ICorePlugin plugin)
    {
        if (
            GUIHelpers.DoTriggerButton(new UFStyle("     " + plugin.Title, ElementDesignerStyles.EventButtonStyleSmall,
                null,
                plugin.Enabled ? ElementDesignerStyles.TriggerActiveButtonStyle : ElementDesignerStyles.TriggerInActiveButtonStyle, () => { }, false,
                TextAnchor.MiddleCenter)
            {
                IsWindow = true,
                FullWidth = true
            }))
        {
            plugin.Enabled = !plugin.Enabled;
            InvertApplication.Container = null;
        }
    }

  
}