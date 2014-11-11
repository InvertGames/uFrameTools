using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Invert.Core.GraphDesigner.UnitySpecific
{
    public class UnityAssetManager : IAssetManager
    {
        public object CreateAsset(Type type)
        {
            return CreateAssetInternal(type);
        }

        public object LoadAssetAtPath(string path, Type repositoryFor)
        {
            return AssetDatabase.LoadAssetAtPath(path, repositoryFor);
        }

        public IEnumerable<object> GetAssets(Type type)
        {
            return GetUnityAssets(type).Cast<object>();
        }

        public static Object[] GetUnityAssets(Type assetType)
        {
            var tempObjects = new List<Object>();
            var directory = new DirectoryInfo(Application.dataPath);
            FileInfo[] goFileInfo = directory.GetFiles("*" + ".asset", SearchOption.AllDirectories);

            int i = 0; int goFileInfoLength = goFileInfo.Length;
            for (; i < goFileInfoLength; i++)
            {
                FileInfo tempGoFileInfo = goFileInfo[i];
                if (tempGoFileInfo == null)
                    continue;

                string tempFilePath = tempGoFileInfo.FullName;
                tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");
                try
                {

                    var tempGo = AssetDatabase.LoadAssetAtPath(tempFilePath, assetType) as Object;
                    if (tempGo == null)
                    {

                    }
                    else
                    {
                        tempObjects.Add(tempGo);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }

            }

            return tempObjects.ToArray();
        }

        /// <summary>
        //	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static T CreateAssetInternal<T>(string assetPath = null, string assetName = null) where T : ScriptableObject
        {
            return CreateAssetInternal(typeof(T), assetPath, assetName) as T;


        }

        public static ScriptableObject CreateAssetInternal(Type type, string assetPath = null, string assetName = null)
        {
            var asset = ScriptableObject.CreateInstance(type) as ScriptableObject;

            string path = assetPath ?? AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = assetName == null ? AssetDatabase.GenerateUniqueAssetPath(path + "/New" + type.ToString() + ".asset") : AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            return asset;
        }
    }
}
