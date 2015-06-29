using System.IO;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using UnityEditor;
using UnityEngine;

public class UFramePluginProcessor : AssetPostprocessor
{
    
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        //if (importedAssets.Length > 0)
        //{
        
            var dir = new DirectoryInfo(Application.dataPath);
            var packages = dir.GetFiles("UF-*.unitypackage",SearchOption.AllDirectories);
        
            foreach (var fileInfo in packages)
            {
                var typeNameCheck=fileInfo.Name.Replace("UF-", "").Replace(".unitypackage", "");
                if (InvertApplication.FindType(typeNameCheck) != null)
                {
                    var isInstalled = InvertApplication.Plugins.FirstOrDefault(p => p.PackageName.ToUpper() == typeNameCheck.ToUpper()) != null;
                    var askedYet = EditorPrefs.GetBool("UF_" + typeNameCheck + "_ASKED", false);
                    if (!isInstalled && !askedYet)
                    {
                        if (EditorUtility.DisplayDialog("uFrame",
                            typeNameCheck + " can now be installed.  Would you like to do this now?", "Install",
                            "Don't Install"))
                        {
                            AssetDatabase.ImportPackage(fileInfo.FullName,true);
                            var relativePath = fileInfo.FullName.Replace(dir.FullName, "");
                            Debug.Log(relativePath);
                        }
                        else
                        {
                            EditorPrefs.SetBool("UF_" + typeNameCheck + "_ASKED", true);
#if DEBUG
                           
                            Debug.Log(typeNameCheck + "install will not ask.");
#endif
                        }
                        
                    }
                }
                
            }

        //}
    }
}
public class UFrameAssetProcessor : AssetPostprocessor
{

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var str in importedAssets)
        {
            var str1 = str;
            InvertApplication.SignalEvent<IAssetImported>(_=>_.AssetImported(str1));
        }
        foreach (var str in deletedAssets)
        {
            var str1 = str;
            InvertApplication.SignalEvent<IAssetDeleted>(_ => _.AssetDeleted(str1));
        }

        for (var i = 0; i < movedAssets.Length; i++)
        {
            var i1 = i;
            InvertApplication.SignalEvent<IAssetMoved>(_ => _.AssetMoved(movedAssets[i1], movedFromAssetPaths[i1]));
        }
    }
}