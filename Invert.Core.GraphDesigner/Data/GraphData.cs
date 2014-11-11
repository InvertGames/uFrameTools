using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using UnityEditor;
using UnityEngine;

public class GraphData : ScriptableObject, IGraphData, ISerializationCallbackReceiver, IItem
{
    [SerializeField,HideInInspector]
    public string _jsonData;

    [NonSerialized]
    private List<IDiagramNode> _nodes = new List<IDiagramNode>();

    private string _identifier;
    private List<Refactorer> _refactorings = new List<Refactorer>();

    [NonSerialized]
    private ElementDiagramSettings _settings = new ElementDiagramSettings();

    [NonSerialized]
    private FilterState _filterState = new FilterState();

    private bool _errors;
    private FilterPositionData _positionData;
    private IDiagramFilter _rootFilter;
    private ICodePathStrategy _codePathStrategy;


   
    public ICodePathStrategy CodePathStrategy
    {
        get
        {
            if (_codePathStrategy != null) return _codePathStrategy;

            _codePathStrategy =
                InvertGraphEditor.Container.Resolve<ICodePathStrategy>(Settings.CodePathStrategyName ?? "Default");

            _codePathStrategy.Data = this;
            _codePathStrategy.AssetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));

            return _codePathStrategy;
        }
        set { _codePathStrategy = value; }
    }


    public FilterPositionData PositionData
    {
        get { return _positionData ?? (_positionData = new FilterPositionData()); }
        set { _positionData = value; }
    }

    public void RemoveItem(IDiagramNodeItem nodeItem)
    {
        
    }

    public void AddItem(IDiagramNodeItem item)
    {
        
    }

    public IDiagramFilter CurrentFilter
    {
        get
        {
            if (FilterState.FilterStack.Count < 1)
            {
                return RootFilter;
            }
            return FilterState.FilterStack.Peek();
        }
    }

    public IProjectRepository Repository { get; set; }

    public List<IDiagramNode> Nodes
    {
        get { return _nodes; }
        set { _nodes = value; }
    }

    public string Identifier
    {
        get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier; }
        set { _identifier = value; }
    }

    public ElementDiagramSettings Settings
    {
        get { return _settings; }
        set { _settings = value; }
    }

    public virtual string Name
    {
        get { return Regex.Replace(name, "[^a-zA-Z0-9_.]+", ""); }
    }
     
    public string Namespace { get; set; }
    public string Version { get; set; }
    public int RefactorCount { get; set; }

    public virtual IEnumerable<IDiagramNode> NodeItems
    {
        get
        {
            if (RootFilter is IDiagramNode)
                yield return RootFilter as IDiagramNode;
            foreach (var item in Nodes)
                yield return item;
        }
    }

    public FilterState FilterState
    {
        get { return _filterState; }
        set { _filterState = value; }
    }

    public virtual IDiagramFilter RootFilter
    {
        get
        {

            return _rootFilter ?? (_rootFilter = CreateDefaultFilter());
        }
        set { _rootFilter = value; }
    }

    protected virtual IDiagramFilter CreateDefaultFilter()
    {
        return null;
    }

    public bool Errors
    {
        get { return _errors; }
        set { _errors = value; }
    }

    public Exception Error { get; set; }

    public JSONNode Serialize()
    {
        return Serialize(this);
    }

    public static JSONNode Serialize(GraphData data)
    {
        // The root class for the diagram data
        var root = new JSONClass
        {
            {"Name", new JSONData(data.Name)}, // Name of the diagram
            {"Version", new JSONData(data.Version)},// Version of the diagram
            {"Identifier", new JSONData(data.Identifier)}// Version of the diagram
        };
        if (data.FilterState != null)
            // Add the filter state
            root.AddObject("FilterState", data.FilterState);
        if (data.Settings != null)
            // Add the settings
            root.AddObject("Settings", data.Settings);



        if (data.PositionData != null)
        {
            root.AddObject("PositionData", data.PositionData);
        }

        // Store the root filter
        root.AddObject("SceneFlow", data.RootFilter as IJsonObject);
        // Nodes
        root.AddObjectArray("Nodes", data.Nodes);
        return root;
    }

    public void Initialize()
    {
        //if (FilterState.FilterStack.Count < 1)
        //{
        //    FilterState.FilterStack.Push(SceneFlowFilter);
        //}
        FilterState.Reload(this);
    }

    public void AddNode(IDiagramNode data)
    {
        data.IsCollapsed = true;
        Nodes.Add(data);
    }

    public void RemoveNode(IDiagramNode enumData)
    {
        if (enumData == RootFilter) return;
        //foreach (var item in Nodes)
        //{
        //    if (item.Locations.Keys.Contains(item.Identifier))
        //    {
        //        item.Locations.Remove(item.Identifier);
        //    }
        //}
        foreach (var item in PositionData.Positions)
        {
            item.Value.Remove(enumData.Identifier);
        }
        Nodes.Remove(enumData);
    }

    public void OnBeforeSerialize()
    {

        var backup = _jsonData;
        try
        {
            _jsonData = Serialize().ToString();
        }
        catch (Exception ex)
        {
            _jsonData = backup;
            UnityEngine.Debug.LogException(ex);
        }

    }

    public void OnAfterDeserialize()
    {
        //Debug.Log("Deserialize");
        try
        {
            Deserialize(_jsonData);
            CleanUpDuplicates();
            Errors = false;
        }
        catch (Exception ex)
        {
            Debug.Log(_jsonData);
            Debug.Log(this.name + " has a problem.");

            Debug.LogException(ex);
            Errors = true;
            Error = ex;
        }

    }

    private void CleanUpDuplicates()
    {
        foreach (var nodes in Nodes.GroupBy(p => p.Identifier).ToArray())
        {
            if (nodes.Count() > 1)
            {
                var identifier = nodes.First();
                Nodes.Remove(identifier);
            }
        }
    }

    private void Deserialize(string jsonData)
    {

        if (jsonData == null)
        {
            return;
        }


        var jsonNode = JSONNode.Parse(jsonData);

        if (jsonNode == null)
        {

            Debug.Log("Couldn't parse file." + this.name);
            return;
        }

        Nodes.Clear();

        this.Version = jsonNode["Version"].Value;
        this._identifier = jsonNode["Identifier"].Value;

        if (jsonNode["Nodes"] is JSONArray)
        {

            //uFrameEditor.Log(this.name + jsonNode["Nodes"].ToString());
            Nodes.AddRange(jsonNode["Nodes"].AsArray.DeserializeObjectArray<IDiagramNode>(this));

        }
        else
        {
        }


        if (jsonNode["SceneFlow"] is JSONClass)
            RootFilter = jsonNode["SceneFlow"].DeserializeObject(this) as IDiagramFilter;

        if (jsonNode["PositionData"] != null)
            PositionData = jsonNode["PositionData"].DeserializeObject(this) as FilterPositionData;


        if (jsonNode["FilterState"] is JSONClass)
        {
            FilterState = new FilterState();
            FilterState.Deserialize(jsonNode["FilterState"].AsObject, this);
        }

        if (jsonNode["Settings"] is JSONClass)
        {
            Settings = new ElementDiagramSettings();
            Settings.Deserialize(jsonNode["Settings"].AsObject, this);
        }
        if (string.IsNullOrEmpty(Version))
        {
            foreach (var filter in NodeItems.OfType<IDiagramFilter>().Concat(new[] { RootFilter }).ToArray())
            {
                var index = 0;
                foreach (var itemLoction in filter.Locations.Keys)
                {
                    PositionData[filter, itemLoction] = filter.Locations.Values[index];
                    index++;
                }
            }
        }
    }

    public string Title
    {
        get
        {
            return Name;
        }
    }

    public string SearchTag
    {
        get
        {
            return Name;
        }
    }
}