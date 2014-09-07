using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.uFrame.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public class UFrameAssetManager : AssetPostprocessor
{

    [MenuItem("Assets/[u]Frame/New Element Diagram", false, 40)]
    public static void NewJsonViewModelDiagram()
    {
        uFrameEditor.CurrentProject.CreateNewDiagram();
    }

    [MenuItem("Assets/[u]Frame/New uFrame Project", false, 40)]
    public static void NewUframeProject()
    {
        var project = CreateAsset<ProjectRepository>();
        project.OutputDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(project));
        //project.n = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(project));
    }
    public static void NewUframeProject(string name)
    {
        //var project = CreateAsset<ProjectRepository>("Assets/" + path);
        AssetDatabase.CreateFolder("Assets", name);
        var project = ScriptableObject.CreateInstance<ProjectRepository>();
        AssetDatabase.CreateAsset(project,"Assets/" + name + "/" + name + ".asset");
        project.OutputDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(project));
        uFrameEditor.CurrentProject = project;
        Selection.activeObject = project;
        uFrameEditor.CurrentProject.CreateNewDiagram();

        //project.n = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(project));
    }
    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    public static T CreateAsset<T>(string assetPath = null, string assetName = null) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = assetPath ?? AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = assetName == null ? AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset") : path + "/" + assetName + ".asset";

        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        return asset;
    }


    public static void OnPostprocessAllAssets(
        String[] importedAssets,
        String[] deletedAssets,
        String[] movedAssets,
        String[] movedFromAssetPaths)
    {

    }



    private static void Refresh()
    {

    }
}