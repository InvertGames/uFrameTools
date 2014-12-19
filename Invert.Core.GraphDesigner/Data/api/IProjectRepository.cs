using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = System.Object;

namespace Invert.Core.GraphDesigner
{
    public interface IProjectRepository : INodeRepository
    {
        IGraphData CurrentGraph { get; set; }

        IEnumerable<IGraphData> Graphs { get; set; }

        IEnumerable<OpenGraph> OpenGraphs { get; }

        Type RepositoryFor { get; }

        void CloseGraph(OpenGraph tab);

        //Dictionary<string, string> GetProjectDiagrams();
        IGraphData CreateNewDiagram(Type diagramType, IDiagramFilter defaultFilter = null);

        bool GetSetting(string key, bool def = true);

        IGraphData LoadDiagram(string path);

        void Refresh();

        bool SetSetting(string key, bool value);
    }

    public abstract class DefaultProjectRepository
#if UNITY_DLL
 : ScriptableObject, IProjectRepository
#else
        : IProjectRepository
#endif

    {
        protected IGraphData _currentGraph;
        protected IDiagramNode[] _nodeItems;

        private List<OpenGraph> _openTabs = new List<OpenGraph>();
        private Dictionary<string, bool> _settingsBag;
        private List<IGraphData> _includedGraphs;

        public bool this[string settingsKey, bool def = true]
        {
            get
            {
                if (SettingsBag.ContainsKey(settingsKey))
                {
                    return SettingsBag[settingsKey];
                }
                return def;
            }
            set
            {
                if (SettingsBag.ContainsKey(settingsKey))
                {
                    SettingsBag[settingsKey] = value;
                }
                else
                {
                    SettingsBag.Add(settingsKey, value);
                }
            }
        }

        public IEnumerable<IGraphItem> AllGraphItems
        {
            get
            {
                foreach (var diagram in Graphs)
                {
                    if (diagram == null || Object.Equals(diagram, null)) continue;
                    foreach (var item in diagram.AllGraphItems)
                    {
                        yield return item;
                    }
                }
            }
        }

        public IEnumerable<IDiagramNode> AllNodeItems
        {
            get
            {
                foreach (var item in Graphs)
                {
                    if (item == null || Object.Equals(item, null)) continue;
                    foreach (var node in item.NodeItems)
                    {
                        yield return node;
                    }
                }
            }
        }

        public IEnumerable<ConnectionData> Connections
        {
            get
            {
                foreach (var diagram in Graphs)
                {
                    if (diagram == null || Object.Equals(diagram, null)) continue;
                    foreach (var item in diagram.Connections)
                    {
                        yield return item;
                    }
                }
            }
        }

        public IDiagramFilter CurrentFilter
        {
            get
            {
                return CurrentGraph.CurrentFilter;
            }
        }

        public abstract IGraphData CurrentGraph { get; set; }

        public abstract IEnumerable<IGraphData> Graphs { get; set; }

        public abstract string LastLoadedDiagram { get; set; }

        public virtual string Name { get; set; }

        public virtual IEnumerable<IDiagramNode> NodeItems
        {
            get
            {
                return _nodeItems ?? (_nodeItems = Enumerable.ToArray<IDiagramNode>(AllNodeItems));
            }
        }

        public IEnumerable<OpenGraph> OpenGraphs
        {
            get { return OpenTabs; }
        }

        public virtual List<OpenGraph> OpenTabs
        {
            get { return _openTabs ?? (_openTabs = new List<OpenGraph>()); }
            set { _openTabs = value; }
        }

        public FilterPositionData PositionData
        {
            get { return CurrentGraph.PositionData; }
        }

        public virtual string Namespace { get; set; }

        public virtual Type RepositoryFor
        {
            get { return typeof(IGraphData); }
        }

        public ElementDiagramSettings Settings
        {
            get
            {
                return CurrentGraph.Settings;
            }
        }

        public Dictionary<string, bool> SettingsBag
        {
            get { return _settingsBag ?? (_settingsBag = new Dictionary<string, bool>()); }
            set { _settingsBag = value; }
        }

        public virtual void AddItem(IDiagramNodeItem item)
        {
            //item.Node = node;
            var node = item.Node;
            node.PersistedItems = node.PersistedItems.Concat(new[] { item });

            foreach (var nodeItem in NodeItems)
            {
                nodeItem.NodeItemAdded(item);
            }
        }

        public virtual void AddNode(IDiagramNode data)
        {
            data.Graph = CurrentGraph;
            foreach (var item in NodeItems)
            {
                data.NodeAdded(data);
                foreach (var containedItem in item.PersistedItems)
                {
                    containedItem.NodeAdded(data);
                }
            }

            _nodeItems = null;
            CurrentGraph.AddNode(data);
        }

        public void CloseGraph(OpenGraph tab)
        {
            OpenTabs.Remove(tab);
        }

        public abstract IGraphData CreateNewDiagram(Type diagramType, IDiagramFilter defaultFilter = null);

        public Vector2 GetItemLocation(IDiagramNode node)
        {
            //if (!CurrentDiagram.PositionData.HasPosition(CurrentFilter, node))
            //{
            //    foreach (var diagram in Diagrams)
            //    {
            //        if (diagram.PositionData.HasPosition(CurrentFilter, node))
            //        {
            //            return diagram.PositionData[CurrentFilter,node];
            //        }
            //    }
            //}
            return CurrentGraph.PositionData[CurrentFilter, node];
        }

        public bool GetSetting(string key, bool def = true)
        {
            return this[key, def];
        }

        public void HideNode(string identifier)
        {
            CurrentGraph.PositionData.Remove(CurrentGraph.CurrentFilter, identifier);
        }

        public virtual IGraphData LoadDiagram(string path)
        {
            //foreach (var asset in Graphs.OfType<InvertGraph>())
            //{
            //    if (asset.Path == path)
            //    {
            //        CurrentGraph = asset;
            //        return asset;
            //    }
            //}
            var data = InvertGraphEditor.AssetManager.LoadAssetAtPath(path, RepositoryFor) as IGraphData;
            if (data == null)
            {
                return null;
            }
            CurrentGraph = data;
            return data;
        }

        public virtual void MarkDirty(INodeRepository data)
        {
        }

        public abstract void RecordUndo(INodeRepository data, string title);

        public abstract void Refresh();

        public virtual void RemoveItem(IDiagramNodeItem nodeItem)
        {
            nodeItem.Node.RemoveItem(nodeItem);

            foreach (var node in NodeItems.ToArray())
            {
                node.NodeItemRemoved(nodeItem);
                foreach (var item in node.PersistedItems.ToArray())
                {
                    item.NodeItemRemoved(nodeItem);
                }
            }
        }

        public virtual void RemoveNode(IDiagramNode enumData)
        {
            _nodeItems = null;
            foreach (var item in enumData.PersistedItems.ToArray())
            {
                RemoveItem(item);
            }
            CurrentGraph.RemoveNode(enumData);
            foreach (var item in NodeItems.ToArray())
            {
                item.NodeRemoved(enumData);
            }
        }

        public virtual void Save()
        {
            foreach (var graph in Graphs)
            {
                graph.Save();
            }
        }

        public abstract void SaveDiagram(INodeRepository data);

        public void SetItemLocation(IDiagramNode node, Vector2 position)
        {
            CurrentGraph.PositionData[CurrentFilter, node] = position;
        }

        public bool SetSetting(string key, bool value)
        {
            return this[key] = value;
        }
    }

    [Serializable]
    public class OpenGraph
    {
        [SerializeField]
        private string _graphIdentifier;

        [SerializeField]
        private string _graphName;

        [SerializeField]
        private string[] _path;

        public string GraphIdentifier
        {
            get { return _graphIdentifier; }
            set { _graphIdentifier = value; }
        }

        public string GraphName
        {
            get { return _graphName; }
            set { _graphName = value; }
        }

        public string[] Path
        {
            get { return _path; }
            set { _path = value; }
        }
    }
}