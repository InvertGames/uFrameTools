using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using UnityEngine;

public class GraphData : ScriptableObject, IGraphData, ISerializationCallbackReceiver, IItem
{
    [SerializeField, HideInInspector]
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
    private List<ConnectionData> _connections;

    public bool CodeGenDisabled { get; set; }

    public ICodePathStrategy CodePathStrategy
    {
        get
        {
            if (_codePathStrategy != null) return _codePathStrategy;

            _codePathStrategy =
                InvertGraphEditor.Container.Resolve<ICodePathStrategy>(Settings.CodePathStrategyName ?? "Default");

            _codePathStrategy.Data = this;
            _codePathStrategy.AssetPath = Path.GetDirectoryName(InvertGraphEditor.Platform.GetAssetPath(this));

            return _codePathStrategy;
        }
        set { _codePathStrategy = value; }
    }

    public IEnumerable<IGraphItem> AllGraphItems 
    {
        get
        {
            foreach (var node in Nodes)
            {
                yield return node;
                foreach (var item in node.GraphItems)
                    yield return item;
                
            }
        }
    }

    public IEnumerable<ConnectionData> Connections
    {
        get { return ConnectedItems; }
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
            {"Identifier", new JSONData(data.Identifier)},// Version of the diagram
          // Version of the diagram
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

        root.AddObjectArray("ConnectedItems", data.ConnectedItems);
        return root;
    }

    public List<ConnectionData> ConnectedItems
    {
        get { return _connections ?? (_connections = new List<ConnectionData>()); }
        set { _connections = value; }
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
            if (!Errors)
            {
                _jsonData = Serialize().ToString();
            }
            
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

    public void AddConnection(IGraphItem output, IGraphItem input)
    {
        ConnectedItems.Add(new ConnectionData(output.Identifier,input.Identifier)
        {
            Graph = this,
            Output = output,
            Input = input
        });
    }

    public void RemoveConnection(IGraphItem output, IGraphItem input)
    {
        ConnectedItems.RemoveAll(p => p.OutputIdentifier == output.Identifier && p.InputIdentifier == input.Identifier);
    }
    public void ClearOutput(IGraphItem output)
    {
        ConnectedItems.RemoveAll(p => p.OutputIdentifier == output.Identifier);
    }
    public void ClearInput(IGraphItem input)
    {
        ConnectedItems.RemoveAll(p => p.InputIdentifier == input.Identifier);
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

        if (jsonNode["SceneFlow"] is JSONClass)
            RootFilter = jsonNode["SceneFlow"].DeserializeObject(this) as IDiagramFilter;

        if (jsonNode["PositionData"] != null)
            PositionData = jsonNode["PositionData"].DeserializeObject(this) as FilterPositionData;


        this.Version = jsonNode["Version"].Value;
        this._identifier = jsonNode["Identifier"].Value;
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
        if (jsonNode["Nodes"] is JSONArray)
        {
            Nodes.Clear();
            //uFrameEditor.Log(this.name + jsonNode["Nodes"].ToString());
            Nodes.AddRange(jsonNode["Nodes"].AsArray.DeserializeObjectArray<IDiagramNode>(this));
        }
        if (jsonNode["ConnectedItems"] is JSONArray)
        {
            ConnectedItems.Clear();
            ConnectedItems.AddRange(jsonNode["ConnectedItems"].AsArray.DeserializeObjectArray<ConnectionData>(this));

            
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

    public void SetProject(IProjectRepository project)
    {
        foreach (var item in ConnectedItems)
        {
            item.Graph = this;
            item.Input = project.AllGraphItems.FirstOrDefault(p => p.Identifier == item.InputIdentifier);
            item.Output = project.AllGraphItems.FirstOrDefault(p => p.Identifier == item.OutputIdentifier);
        }
        ConnectedItems.RemoveAll(p => p.Output == null || p.Input == null);

    }
}


[Serializable]
public class ConnectionData : IJsonObject
{
    [SerializeField]
    private string _outputIdentifier;

    [SerializeField]
    private string _inputIdentifier;

    public ConnectionData(string outputIdentifier, string inputIdentifier)
    {
        OutputIdentifier = outputIdentifier;
        InputIdentifier = inputIdentifier;
    }

    public ConnectionData()
    {
    }

    public string OutputIdentifier
    {
        get { return _outputIdentifier; }
        set { _outputIdentifier = value; }
    }

    public string InputIdentifier
    {
        get { return _inputIdentifier; }
        set { _inputIdentifier = value; }
    }

    public GraphData Graph { get; set; }
    public IGraphItem Output { get; set; }
    public IGraphItem Input { get; set; }

    public void Serialize(JSONClass cls)
    {
        cls.Add("OutputIdentifier", OutputIdentifier ?? string.Empty);
        cls.Add("InputIdentifier", InputIdentifier ?? string.Empty);
    }

    public void Deserialize(JSONClass cls, INodeRepository repository)
    {
        if (cls["InputIdentifier"] != null)
        {
            InputIdentifier = cls["InputIdentifier"].Value;
        }
        if (cls["OutputIdentifier"] != null)
        {
            OutputIdentifier = cls["OutputIdentifier"].Value;
        }
    }
}