using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class JsonElementDesignerData : ScriptableObject, IElementDesignerData, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    public string _jsonData;

    [NonSerialized]
    private List<IDiagramNode> _nodes = new List<IDiagramNode>();

    [NonSerialized]
    private SceneFlowFilter _sceneFlowFilter = new SceneFlowFilter();

    [NonSerialized]
    private List<IDiagramLink> _links = new List<IDiagramLink>();

    public IDiagramFilter CurrentFilter
    {
        get
        {
            if (FilterState.FilterStack.Count < 1)
            {
                return SceneFlowFilter;
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

    public JSONNode Serialize()
    {
        return Serialize(this);
    }

    public static JSONNode Serialize(IElementDesignerData data)
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
        // Store the root filter
        root.AddObject("SceneFlow", data.SceneFlowFilter);
        // Nodes
        root.AddObjectArray("Nodes", data.NodeItems);
        return root;
    }

    private string _identifier;

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

    public string Name
    {
        get { return Regex.Replace(name, "[^a-zA-Z0-9_.]+", ""); }
    }

    public string Version { get; set; }

    public int RefactorCount { get; set; }

    public IEnumerable<IDiagramNode> NodeItems
    {
        get
        {
            return Nodes;
        }
    }

    public FilterState FilterState
    {
        get { return _filterState; }
        set { _filterState = value; }
    }

    private List<Refactorer> _refactorings = new List<Refactorer>();

    [NonSerialized]
    private ElementDiagramSettings _settings = new ElementDiagramSettings();

    [NonSerialized]
    private FilterState _filterState = new FilterState();

    private bool _errors;
    private List<string> _externalReferences;

   

    public SceneFlowFilter SceneFlowFilter
    {
        get { return _sceneFlowFilter; }
        set { _sceneFlowFilter = value; }
    }


    public List<IDiagramLink> Links
    {
        get { return _links; }
        set { _links = value; }
    }

    public bool Errors
    {
        get { return _errors; }
        set { _errors = value; }
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
        Nodes.Remove(enumData);
    }

    public void OnBeforeSerialize()
    {
        if (!Errors)
        {
            _jsonData = Serialize().ToString();
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

    public Exception Error { get; set; }

    private void Deserialize(string jsonData)
    {
        Debug.Log(jsonData);
        if (jsonData == null) return;

        var jsonNode = JSONNode.Parse(jsonData);
     
        if (jsonNode == null)
        {
            Debug.Log("Couldn't parse file.");
            return;
        }

        Nodes.Clear();

        this.Version = jsonNode["Version"].Value;
        this._identifier = jsonNode["Identifier"].Value;

        if (jsonNode["Nodes"] is JSONArray)
            Nodes.AddRange(jsonNode["Nodes"].AsArray.DeserializeObjectArray<IDiagramNode>());

        if (jsonNode["SceneFlow"] is JSONClass)
            SceneFlowFilter = jsonNode["SceneFlow"].DeserializeObject() as SceneFlowFilter;

        if (jsonNode["FilterState"] is JSONClass)
        {
            FilterState = new FilterState();
            FilterState.Deserialize(jsonNode["FilterState"].AsObject);
        }

        if (jsonNode["Settings"] is JSONClass)
        {
            Settings = new ElementDiagramSettings();
            Settings.Deserialize(jsonNode["Settings"].AsObject);
        }
    }
}