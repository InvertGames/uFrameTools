using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = System.Object;

namespace Invert.Core.GraphDesigner
{
    public interface ISubscribable<T>
    {
        List<T> Listeners { get; set; }

        Action Subscribe(T handler);
        void Unsubscribe(T handler);
    }

    public static class SubscribableExtensions
    {
        public static void Signal<T>(this ISubscribable<T> t, Action<T> action)
        {
            foreach (var listener in t.Listeners)
            {
                action(listener);
            }
        }
    }
    public interface IProjectRepository : INodeRepository, ISubscribable<IGraphEvents>
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
        private List<IGraphEvents> _listeners;

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
        /// <summary>
        /// Creates a new diagram based on the type passed in and adds it to this project.
        /// 
        /// <example>
        /// CreateNewDiagram(typeof(MyDiagramData), new MyDiagramNode());
        /// </example>
        /// </summary>
        /// <param name="diagramType">The type of diagram to create.</param>
        /// <param name="defaultFilter">The root node or the root filter to use.</param>
        /// <returns>The created graph.</returns>
        public virtual IGraphData CreateNewDiagram(Type diagramType, IDiagramFilter defaultFilter = null)
        {
            var graph = Activator.CreateInstance(diagramType) as InvertGraph;
            // Create a unique name
            graph.Name = string.Format("{0}{1}", diagramType.Name, Graphs.Count());
            // Set up the root node if specified
            if (defaultFilter != null)
            {
                graph.RootFilter = defaultFilter;
                defaultFilter.Name = graph.Name;
            }
            else
            {
                graph.RootFilter = graph.CreateDefaultFilter();
            }
            // Send the signal to any listeners that a new a graph has been created
            this.Signal(p => p.GraphCreated(this, graph));
            // Add this graph to the project and do any loading necessary
            AddGraph(graph);
            // Go ahead and save this project
            Save();
            // Return the newly created graph
            return graph;
        }

        public Vector2 GetItemLocation(IDiagramNode node)
        {
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

        public List<IGraphEvents> Listeners
        {
            get { return _listeners ?? (_listeners = new List<IGraphEvents>()); }
            set { _listeners = value; }
        }

        public Action Subscribe(IGraphEvents handler)
        {
            Listeners.Add(handler);
            return () => { Unsubscribe(handler); };
        }

        public void Unsubscribe(IGraphEvents handler)
        {
            Listeners.Remove(handler);
        }

        protected virtual void AddGraph(IGraphData graphData)
        {
          
            if (graphData.CodePathStrategy == null)
            {
                graphData.CodePathStrategy = new DefaultCodePathStrategy()
                {
                    Data = graphData,

                };
            }
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

    public class TemporaryProjectRepository : DefaultProjectRepository
    {
        public TemporaryProjectRepository(IEnumerable<IGraphData> graphs)
        {
            Graphs = graphs;
            
        }

        public TemporaryProjectRepository(IGraphData currentGraph, IEnumerable<IGraphData> graphs)
        {
            CurrentGraph = currentGraph;
            Graphs = graphs;
        }

        public TemporaryProjectRepository(IGraphData currentGraph)
        {
            CurrentGraph = currentGraph;
            Graphs = new[] {currentGraph};
        }

        public override IGraphData CurrentGraph { get; set; }

        public override IEnumerable<IGraphData> Graphs { get; set; }

        public override string LastLoadedDiagram { get; set; }

        public override void RecordUndo(INodeRepository data, string title)
        {
            
        }

        public override void Refresh()
        {
           
        }

        public override void SaveDiagram(INodeRepository data)
        {
          
        }
    }
}