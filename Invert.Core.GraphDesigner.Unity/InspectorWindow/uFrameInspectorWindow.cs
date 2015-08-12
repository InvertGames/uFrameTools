using Invert.Core;
using Invert.Core.GraphDesigner;
using UnityEditor;

public class uFrameInspectorWindow : EditorWindow {
    [MenuItem("uFrame/Inspector #&i")]
    internal static void ShowWindow()
    {
        var window = GetWindow<uFrameInspectorWindow>();
        window.title = "uFrame Inspector";
        Instance = window;
        window.Show();
    }

    public static uFrameInspectorWindow Instance { get; set; }

    public void OnGUI()
    {
        Instance = this;
        InvertApplication.SignalEvent<IDrawExplorer>(_=>_.DrawExplorer());
        InvertApplication.SignalEvent<IDrawInspector>(_=>_.DrawInspector());
    }
}