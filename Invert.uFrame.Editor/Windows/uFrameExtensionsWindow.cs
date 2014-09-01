using Invert.Common;
using Invert.Common.UI;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEditor;
using UnityEngine;

public class uFramePluginsWindow : EditorWindow
{

    [MenuItem("Tools/[u]Frame/Plugins")]
    internal static void ShowWindow()
    {
        var window = GetWindow<uFramePluginsWindow>();
        window.title = "uFrame Plugins";
        window.minSize = new Vector2(240, 300);

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
        
        DrawTitleBar("uFrame Plugins");
        foreach (var plugin in uFrameEditor.Container.ResolveAll<IDiagramPlugin>())
        {
            if (
                GUIHelpers.DoTriggerButton(new UFStyle("     " + plugin.Title, UBStyles.EventButtonStyle,
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