using System.Linq;
using System.Reflection;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;
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
        DrawTitleBar("uFrame Settings");
        uFrameSettings();
    }

    [PreferenceItem("uFrame")]
    public static void uFrameSettings()
    {
        ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
        var s = InvertGraphEditor.Settings;
       
        if (s != null)
        {
            if (GUIHelpers.DoToolbarEx("Color Settings"))
            {
                s.BackgroundColor = EditorGUILayout.ColorField("Background Color", s.BackgroundColor);
                EditorGUI.BeginChangeCheck();
                s.TabTextColor = EditorGUILayout.ColorField("Tab Text Color", s.TabTextColor);
                if (EditorGUI.EndChangeCheck())
                {
                    ElementDesignerStyles.TabStyle = null;
                }
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
            EditorGUILayout.HelpBox("Settings not available.", MessageType.Info);
        }

        //if (GUIHelpers.DoToolbarEx("Plugins - Enabled"))
        //{
            foreach (
                var plugin in InvertApplication.Plugins.Where(p => !p.Required && !p.Ignore).OrderBy(p=>p.LoadPriority))
            {
                DoPlugin(plugin);
            }
        //}
        //if (GUIHelpers.DoToolbarEx("Plugins - Disabled"))
        //{
        //    foreach (
        //        var plugin in InvertApplication.Plugins.Where(p => !p.Required && !p.Enabled).OrderBy(p => p.LoadPriority))
        //    {
        //        DoPlugin(plugin);
        //    }
        //}
        //if (GUIHelpers.DoToolbarEx("Registered Nodes"))
        //{
        //    foreach (var plugin in InvertGraphEditor.Container.ResolveAll<NodeConfigBase>())
        //    {
        //        GUIHelpers.DoTriggerButton(new UFStyle(plugin.Name + " : " + plugin.NodeType.Name,
        //            ElementDesignerStyles.EventButtonStyleSmall)
        //        {
        //            IsWindow = true,
        //            FullWidth = true
        //        });
        //    }
        //}
        //if (GUIHelpers.DoToolbarEx("Loaded Assemblies"))
        //{
        //    foreach (var plugin in InvertApplication.CachedAssemblies)
        //    {
        //        GUIHelpers.DoTriggerButton(new UFStyle(plugin.FullName, ElementDesignerStyles.EventButtonStyleSmall)
        //        {
        //            IsWindow = true,
        //            FullWidth = true
        //        });
        //    }
        //}
        EditorGUILayout.EndScrollView();
    }

    public static Vector2 ScrollPosition { get; set; }


    private static void DoPlugin(ICorePlugin plugin)
    {

        if (GUIHelpers.DoToolbarEx(plugin.Title))
        {
            EditorGUI.BeginChangeCheck();

            plugin.Enabled = GUILayout.Toggle(plugin.Enabled, "Enabled");
            if (EditorGUI.EndChangeCheck())
            {
                InvertApplication.Container = null;
            }
            if (plugin.Enabled)
            {
                var properties = plugin.GetType().GetPropertiesWithAttribute<InspectorProperty>(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                var platform = InvertGraphEditor.PlatformDrawer as UnityDrawer;
                foreach (var property in properties)
                {
               
                    var property1 = property;
                    platform.DrawInspector(new PropertyFieldViewModel()
                    {
                        CachedValue = property.Key.GetValue(null, null),
                        Getter = () => property1.Key.GetValue(null, null),
                        Setter = _ =>
                        {
                            property1.Key.SetValue(null, _, null);
                            InvertApplication.Container = null;
                        },
                        Name = property.Key.Name,
                        Type = property.Key.PropertyType
                    });
               
                }
            }

        }
       
        //if (
        //    GUIHelpers.DoTriggerButton(new UFStyle("     " + plugin.Title, ElementDesignerStyles.EventButtonStyleSmall,
        //        null,
        //        plugin.Enabled ? ElementDesignerStyles.TriggerActiveButtonStyle : ElementDesignerStyles.TriggerInActiveButtonStyle, () => { }, false,
        //        TextAnchor.MiddleCenter)
        //    {
        //        IsWindow = true,
        //        FullWidth = true
        //    }))
        //{
        //    plugin.Enabled = !plugin.Enabled;
        //    InvertApplication.Container = null;
        //}
      
    }

  
}