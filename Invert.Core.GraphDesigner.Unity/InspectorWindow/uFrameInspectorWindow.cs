using Invert.Core;
using Invert.Core.GraphDesigner;
using UnityEditor;
using UnityEngine;

public class uFrameInspectorWindow : EditorWindow {
    private Vector2 _scrollPosition;

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
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        InvertApplication.SignalEvent<IDrawInspector>(_ => _.DrawInspector());
        var x = 0;
        InvertApplication.SignalEvent<IDrawExplorer>(_ =>
        {
            _.DrawExplorer();
        });

        GUILayout.EndScrollView();
    }
}