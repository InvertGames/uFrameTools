using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core.GraphDesigner;
using Invert.uFrame;
using Invert.uFrame.Editor;
using UnityEditor;
using UnityEngine;
using Object = System.Object;


[Serializable]
public class ProjectRepository : ScriptableObject, IProjectRepository, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private string[] _settingsKeys;
    [SerializeField, HideInInspector]
    private bool[] _settingsValues;

    private Dictionary<string, bool> _settingsBag;
    public Dictionary<string, bool> SettingsBag
    {
        get { return _settingsBag ?? (_settingsBag = new Dictionary<string, bool>()); }
        set { _settingsBag = value; }
    }
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
    public void OnBeforeSerialize()
    {
        if (_settingsBag == null) return;
        _settingsKeys = _settingsBag.Keys.ToArray();
        _settingsValues = _settingsBag.Values.ToArray();
    }

    public void OnAfterDeserialize()
    {
        if (_settingsKeys == null) return;
        SettingsBag.Clear();
        for (var i = 0; i < _settingsKeys.Length; i++)
        {
            if (i >= _settingsValues.Length)
            {
                break;
            }
            var key = _settingsKeys[i];
            var value = _settingsValues[i];
            SettingsBag.Add(key, value);

        }

        OpenTabs.RemoveAll(p => string.IsNullOrEmpty(p.GraphIdentifier));
    }

    public bool GetSetting(string key, bool def = true)
    {
        return this[key, def];
    }

    public bool SetSetting(string key, bool value)
    {
        return this[key] = value;
    }

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

    public void SetItemLocation(IDiagramNode node, Vector2 position)
    {
        CurrentGraph.PositionData[CurrentFilter, node] = position;
    }

    private Dictionary<string, string> _derivedTypes;

    public void MarkDirty(INodeRepository data)
    {
        if (data != null)
            EditorUtility.SetDirty(data as UnityEngine.Object);
    }

    protected string[] _diagramNames;

    [SerializeField, HideInInspector]
    protected List<ScriptableObject> _diagrams;


    private IGraphData _currentGraph;

    public void RecacheAssets()
    {

    }

    //public Dictionary<string, string> GetProjectDiagrams()
    //{
    //    var items = new Dictionary<string, string>();
    //    foreach (var elementDesignerData in Diagrams.Where(p => p.GetType() == RepositoryFor))
    //    {
    //        var asset = AssetDatabase.GetAssetPath(elementDesignerData as UnityEngine.Object);

    //        items.Add(elementDesignerData.Name, asset);
    //    }
    //    return items;
    //}

    public IEnumerable<IGraphData> Graphs
    {
        get { return Diagrams.Cast<IGraphData>(); }
        set { Diagrams = value.Cast<ScriptableObject>().ToList(); }
    }

    public IGraphData CreateNewDiagram(Type diagramType, IDiagramFilter defaultFilter = null)
    {
        Selection.activeObject = this;
        var t = diagramType;
        var diagram = InvertGraphEditor.AssetManager.CreateAsset(t) as IGraphData;
        if (defaultFilter != null && diagram != null)
        {
            diagram.RootFilter = defaultFilter;
        }
        diagram.Version = InvertGraphEditor.CURRENT_VERSION_NUMBER.ToString();
        Diagrams.Add(diagram as ScriptableObject);
        EditorUtility.SetDirty(diagram as ScriptableObject);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Refresh();
        return diagram;
    }
    public virtual Type RepositoryFor
    {
        get { return typeof(IGraphData); }
    }

    public string Name
    {
        get { return name; }
    }


    private IDiagramNode[] _nodeItems;

    [SerializeField]
    private string _projectNamespace;

    [SerializeField]
    private List<OpenGraph> _openTabs;

    public IEnumerable<IDiagramNode> NodeItems
    {
        get
        {
            return _nodeItems ?? (_nodeItems = AllNodeItems.ToArray());
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
    public ElementDiagramSettings Settings
    {
        get
        {
            return CurrentGraph.Settings;
        }
    }

    public void AddNode(IDiagramNode data)
    {
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

    public void RemoveNode(IDiagramNode enumData)
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

    public void RemoveItem(IDiagramNodeItem nodeItem)
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

    public void AddItem(IDiagramNodeItem item)
    {
        //item.Node = node;
        var node = item.Node;
        node.PersistedItems = node.PersistedItems.Concat(new[] { item });

        foreach (var nodeItem in NodeItems)
        {
            nodeItem.NodeItemAdded(item);
        }
    }

    public IDiagramFilter CurrentFilter
    {
        get
        {
            return CurrentGraph.CurrentFilter;
        }
    }

    public FilterPositionData PositionData
    {
        get { return CurrentGraph.PositionData; }
    }

    public string LastLoadedDiagram
    {
        get { return EditorPrefs.GetString("UF_LastLoadedDiagram" + this.name, string.Empty); }
        set { EditorPrefs.SetString("UF_LastLoadedDiagram" + this.name, value); }
    }
    public List<ScriptableObject> Diagrams
    {
        get
        {
            if (_diagrams == null)
            {
                _diagrams = new List<ScriptableObject>();
            }
            return _diagrams;
        }
        set { _diagrams = value; }
    }

    public IGraphData CurrentGraph
    {
        get
        {
            if (Diagrams == null) return null;
            if (_currentGraph == null)
            {
                if (!String.IsNullOrEmpty(LastLoadedDiagram))
                {
                    CurrentGraph = Diagrams.FirstOrDefault(p => p != null && p.name == LastLoadedDiagram) as IGraphData;
                }
                if (_currentGraph == null)
                {
                    CurrentGraph = Diagrams.FirstOrDefault() as IGraphData;
                }
            }
            return _currentGraph;
        }
        set
        {
            _currentGraph = value;
            if (value == null) return;
            var openGraph = OpenGraphs.FirstOrDefault(p => p.GraphName == value.Name);
            if (openGraph == null)
            {
                OpenTabs.Add(new OpenGraph()
                {
                    GraphName = value.Name,
                    GraphIdentifier = value.Identifier
                });
                Debug.Log("Opened Tab");
            }

            LastLoadedDiagram = value.Name;
        }
    }

    public string ProjectNamespace
    {
        get { return _projectNamespace; }
        set { _projectNamespace = value; }
    }


    public IGraphData LoadDiagram(string path)
    {
        var data = InvertGraphEditor.AssetManager.LoadAssetAtPath(path, RepositoryFor) as IGraphData;
        if (data == null)
        {
            return null;
        }
        CurrentGraph = data;
        return data;
    }

    public void SaveDiagram(INodeRepository data)
    {
        if (data != null)
        {
            EditorUtility.SetDirty(data as UnityEngine.Object);
        }
        AssetDatabase.SaveAssets();
    }

    public void RecordUndo(INodeRepository data, string title)
    {
        if (data != null)
            Undo.RecordObject(data as UnityEngine.Object, title);
    }


    public virtual void Refresh()
    {
        //var assets = uFrameEditor.GetAssets(typeof(GraphData));
        //Diagrams = assets.OfType<GraphData>().ToList();

        CurrentGraph = null;
        _nodeItems = null;

        foreach (var diagram in Diagrams)
        {
            (diagram as IGraphData).Prepare();
        }

    }

    public void HideNode(string identifier)
    {
        CurrentGraph.PositionData.Remove(CurrentGraph.CurrentFilter, identifier);
    }

    public List<OpenGraph> OpenTabs
    {
        get { return _openTabs ?? (_openTabs = new List<OpenGraph>()); }
        set { _openTabs = value; }
    }

    public IEnumerable<OpenGraph> OpenGraphs
    {
        get { return OpenTabs; }
    }

    public void CloseGraph(OpenGraph tab)
    {
        OpenTabs.Remove(tab);
    }
}