using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Data;
using Invert.IOC;
using Mono.CSharp;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity
{
    public class GraphConfiguration : ScriptableObject, IGraphConfiguration
    {
        [SerializeField]
        private bool _isCurrent;

        [SerializeField]
        private string _databaseName = "uFrameDB";

        [SerializeField]
        private string _codePath = "Code";
        [SerializeField]
        private string _ns;

        public string DatabaseName
        {
            get { return _databaseName; }
            set { _databaseName = value; EditorUtility.SetDirty(this); }
        }

        public string CodePath
        {
            get { return _codePath; }
            set { _codePath = value; EditorUtility.SetDirty(this); }
        }

        public bool IsCurrent
        {
            get { return _isCurrent; }
            set
            {
                _isCurrent = value;
                EditorUtility.SetDirty(this);
            }
        }

        public string FullPath
        {
            get { return Path.Combine(Application.dataPath.Replace("Assets/",string.Empty).Replace("Assets\\",string.Empty).Replace("Assets", string.Empty), DatabaseName); }
        }

        public string CodeOutputSystemPath
        {
            get { return Path.Combine(Application.dataPath, CodePath); }
        }

        public string Namespace
        {
            get { return _ns; }
            set { _ns = value; }
        }

        public string Title
        {
            get { return this.name; }
        }

        public string Group
        {
            get { return "Local"; }
        }

        public string SearchTag { get; set; }
    }

    public class Save : ElementsDiagramToolbarCommand
    {
        public override void Perform(DiagramViewModel node)
        {
            var db = InvertGraphEditor.Container.Resolve<IRepository>();
            db.Commit();

        }
    }
    public class UnityDatabases : DiagramPlugin
    {
        private static GraphConfiguration _currentDatabase;
        public static GraphConfiguration[] Databases { get; set; }
        [MenuItem("[u]Frame/PrintDB", false, 2)]
        public static void PrintDB()
        {
            var db = InvertGraphEditor.Container.Resolve<IRepository>() as TypeDatabase;
            var sb = new StringBuilder();
            foreach (var item in db.Repositories)
            {
                sb.AppendFormat("{0}:{1}", item.Key, item.Value.GetAll().Count()).AppendLine();
                foreach (var record in item.Value.GetAll())
                {
                    sb.AppendFormat("----- {0}:{1}", record.Identifier, record.GetType().Name).AppendLine();
                }
            }
            Debug.Log(sb.ToString());
        }

        [MenuItem("Assets/[u]Frame/New Database", false, 2)]
        public static void NewUFrameProject()
        {
            var database = CreateAsset<GraphConfiguration>();
            
            foreach (var item in Databases)
            {
                item.IsCurrent = false;
            }
            database.IsCurrent = true;
            AssetDatabase.SaveAssets();
            InvertApplication.Container = null;
        }

        /// <summary>
        //	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static T CreateAsset<T>(string assetPath = null, string assetName = null) where T : ScriptableObject
        {
            return CreateAsset(typeof(T), assetPath, assetName) as T;


        }

        public override void Initialize(UFrameContainer container)
        {
            base.Initialize(container);
            container.RegisterToolbarCommand<Save>();
            Databases = GetAssetsOfType<GraphConfiguration>(".asset");

            foreach (var database in Databases)
            {
                container.RegisterInstance<IGraphConfiguration>(database,database.DatabaseName);
            }
        
        }

        public static ScriptableObject CreateAsset(Type type, string assetPath = null, string assetName = null)
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

            string assetPathAndName = assetName == null ? AssetDatabase.GenerateUniqueAssetPath(path + "/" + type.ToString() + ".asset") : AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetName + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            return asset;
        }

        /// <summary>
        /// Used to get assets of a certain type and file extension from entire project
        /// </summary>
        /// <param name="fileExtension">The file extention the type uses eg ".prefab".</param>
        /// <returns>An Object array of assets.</returns>
        public static T[] GetAssetsOfType<T>(string fileExtension) where T : UnityEngine.Object
        {
            List<T> tempObjects = new List<T>();
            DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
            FileInfo[] goFileInfo = directory.GetFiles("*" + fileExtension, SearchOption.AllDirectories);

            int i = 0; int goFileInfoLength = goFileInfo.Length;
            FileInfo tempGoFileInfo; string tempFilePath;
            T tempGO;
            for (; i < goFileInfoLength; i++)
            {
                tempGoFileInfo = goFileInfo[i];
                if (tempGoFileInfo == null)
                    continue;

                tempFilePath = tempGoFileInfo.FullName;
                tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");
                tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(T)) as T;
                if (tempGO == null)
                {
                    continue;
                }
                else if (!(tempGO is T))
                {
                    continue;
                }

                tempObjects.Add(tempGO);
            }

            return tempObjects.ToArray();
        }

        
    
    }
}
