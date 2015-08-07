using Invert.Core;
using UnityEditor;

public class uFrameInspectorWindow : EditorWindow {
    [MenuItem("uFrame/Inspector #&i")]
    internal static void ShowWindow()
    {
        var window = GetWindow<uFrameInspectorWindow>();
        window.title = "uFrame Inspector";
        Instance = window;
        // window.minSize = new Vector2(400, 500);

        window.Show();
    }

    public static uFrameInspectorWindow Instance { get; set; }

    public void OnGUI()
    {
        Instance = this;
        InvertApplication.SignalEvent<IDrawInspector>(_=>_.DrawInspector());
    }
}