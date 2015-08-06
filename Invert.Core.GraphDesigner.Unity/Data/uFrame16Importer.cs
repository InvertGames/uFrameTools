using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Invert.IOC;
using Invert.Json;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Invert.Data;

namespace Invert.Core.GraphDesigner.Unity
{
    public class uFrame16Importer : DiagramPlugin
    {

        public override void Initialize(UFrameContainer container)
        {
            base.Initialize(container);
            GraphDatas = GetAssetsOfType<UnityGraphData>(".asset");
            container.RegisterToolbarCommand<Import16Databases>();


        }

        public static UnityGraphData[] GraphDatas { get; set; }

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

    public class Import16Databases : ToolbarCommand<DesignerWindow>
    {
        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.BottomLeft; }
        }

        public override string Name
        {
            get { return "Import 1.6 Graphs"; }
        }

        public override void Perform(DesignerWindow node)
        {
            Repository = InvertGraphEditor.Container.Resolve<IRepository>();
            InvertGraphEditor.WindowManager.InitItemWindow(uFrame16Importer.GraphDatas, ImportGraph);
        }

        public override string CanPerform(DesignerWindow node)
        {
            return null;
        }

        public IRepository Repository { get; set; }

        private void ImportGraph(UnityGraphData unityGraphData)
        {
            Debug.Log(string.Format("Importing {0}", unityGraphData.name));
            Debug.Log(unityGraphData._jsonData);
            var json = JSON.Parse(unityGraphData._jsonData);
            ImportData(json as JSONClass);
        }

        public void ImportData(JSONClass node)
        {
            var typeName = string.Empty;
            if (node["_CLRType"] != null)
            {
                typeName = node["_CLRType"].Value;
            }
            else if (node["Type"] != null)
            {
                typeName = node["Type"].Value;
            }
            var type = InvertApplication.FindType(typeName) ?? Type.GetType(typeName);
            if (type == null && typeName.StartsWith("ConnectionData"))
            {
                type = typeof(ConnectionData);
            }

            if (type != null)
            {
                var result = ImportType(type, node);

                if (result is IGraphData)
                {
                    var item = InvertApplication.Container.Resolve<WorkspaceService>();
                    if (item.CurrentWorkspace != null)
                        item.CurrentWorkspace.AddGraph(result as IGraphData);
                    CurrentGraph = result as InvertGraph;
                    CurrentGraph.RootFilterId = node["RootNode"]["Identifier"].Value;
                    Debug.Log("Set Root filter id to " + CurrentGraph.RootFilterId);

                }
                if (result is DiagramNode)
                {
                    CurrentNode = result as DiagramNode;
                    CurrentNode.GraphId = CurrentGraph.Identifier;

                }
                if (result is DiagramNodeItem)
                {
                    ((IDiagramNodeItem)result).NodeId = CurrentNode.Identifier;
                }
                if (result is ITypedItem)
                {
                    ((ITypedItem)result).RelatedType = node["ItemType"].Value;
                }

                foreach (KeyValuePair<string, JSONNode> child in node)
                {
                    var array = child.Value as JSONArray;
                    if (array != null)
                    {

                        foreach (var item in array.Childs.OfType<JSONClass>())
                        {
                            ImportData(item);
                        }


                    }
                    var cls = child.Value as JSONClass;
                    if (cls != null)
                    {
                        if (child.Key == "FilterState") continue;
                        if (child.Key == "Settings") continue;
                        if (child.Key == "Changes") continue;
                        if (child.Key == "PositionData")
                        {
                            ImportPositionData(cls);
                        }
                        else
                        {
                            if (child.Key == "RootNode")
                            {
                                InvertApplication.Log("Importing ROOT NODE");
                            }
                            ImportData(cls);    
                        }
                        
                    }
                }
            }


        }

        private void ImportPositionData(JSONClass positionData)
        {
            foreach (KeyValuePair<string, JSONNode> item in positionData)
            {
                if (item.Key == "_CLRType") continue;
                var filterId = item.Key;
                foreach (KeyValuePair<string, JSONNode> positionItem in item.Value.AsObject)
                {
                    var filterItem = Repository.Create<FilterItem>();
                    filterItem.FilterId = filterId;
                    filterItem.NodeId = positionItem.Key;

                    var x = positionItem.Value["x"].AsInt;
                    var y = positionItem.Value["y"].AsInt;
                    InvertApplication.Log("Importing position ");
                    filterItem.Position = new Vector2(x, y);
                    filterItem.Collapsed = true;
                }


            }
        }

        public DiagramNode CurrentNode { get; set; }

        public InvertGraph CurrentGraph { get; set; }

        public IDataRecord ImportType(Type type, JSONClass cls)
        {
            var node = InvertJsonExtensions.DeserializeObject(type, cls) as IDataRecord;
            if (node != null)
                Repository.Add(node);
            return node;
        }

    }
}