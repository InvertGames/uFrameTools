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
    public const string VM_ASSEMBLY_NAME = "ViewModel, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
#if DEBUG
    [MenuItem("Assets/[u]Frame/New Element Diagram (Legacy Asset)", false, 41)]
    public static void NewViewModelDiagram()
    {
        uFrameEditor.Container.Resolve<IElementsDataRepository>(".asset").CreateNewDiagram();
    }
#endif

    [MenuItem("Assets/[u]Frame/New Element Diagram", false, 40)]
    public static void NewJsonViewModelDiagram()
    {
        uFrameEditor.Container.Resolve<IElementsDataRepository>(".json").CreateNewDiagram();
    }




    static UFrameAssetManager()
    {
        Refresh();
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