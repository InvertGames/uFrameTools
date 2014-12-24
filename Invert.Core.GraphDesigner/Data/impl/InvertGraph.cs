using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;
using UnityEngine;

public class InvertGraph : IGraphData, IItem
{

    private FilterPositionData _positionData;

    private List<IDiagramNode> _nodes = new List<IDiagramNode>();

    private string _identifier;

    private ElementDiagramSettings _settings = new ElementDiagramSettings();

    private FilterState _filterState = new FilterState();

    private IDiagramFilter _rootFilter;
    private bool _errors;
    private List<ConnectionData> _connections;
    private string _ns;
#if !UNITY_DLL
    public FileInfo GraphFileInfo { get; set; }

    public InvertGraph(FileInfo graphFile)
    {
        GraphFileInfo = graphFile;
    }
#endif
    public InvertGraph()
    {
    }

    public InvertGraph(string name, string json)
    {

        Name = name;
        Deserialize(json);
    }
    public InvertGraph(string name, JSONClass json)
    {

        Name = name;
        Deserialize(json);
    }

    public ICodePathStrategy CodePathStrategy { get; set; }

    public IEnumerable<IGraphItem> AllGraphItems 
    {
        get
        {
            foreach (var node in NodeItems)
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
#if UNITY_DLL
        public string Path { get; set; }
    public string Name { get; set; }
#else
    public string Path
    {
        get { return GraphFileInfo.FullName; }
        set
        {
            GraphFileInfo = new FileInfo(value);
        }
    }

    public string Name
    {
        get { return System.IO.Path.GetFileNameWithoutExtension(GraphFileInfo.Name); }
        set
        {
            if (GraphFileInfo == null)
            {
                GraphFileInfo = new FileInfo(value);
            }
            else
            {
                GraphFileInfo = new FileInfo(System.IO.Path.Combine(GraphFileInfo.Directory.FullName, value));
            }
            
            
        }
    }
#endif

    public string Version { get; set; }
    public int RefactorCount { get; set; }

    public virtual IEnumerable<IDiagramNode> NodeItems
    {
        get
        {
            if (RootFilter is IDiagramNode && !Nodes.Contains(RootFilter as IDiagramNode))
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
        set
        {
            _rootFilter = value;
            var asNode = value as IDiagramNode;
            if (asNode != null)
            {
                asNode.Graph = this;
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

    public bool CodeGenDisabled { get; set; }
    public INodeRepository Repository { get; set; }

    public List<IDiagramNode> Nodes
    {
        get { return _nodes; }
        set { _nodes = value; }
    }

    public string Namespace
    {
        get
        {
            if (_ns == null && Project != null)
            {
                return Project.Namespace;
            }
            return _ns;
        }
        set { _ns = value; }
    }

    public bool Errors
    {
        get { return _errors; }
        set { _errors = value; }
    }

    public Exception Error { get; set; }


    public List<ConnectionData> ConnectedItems
    {
        get { return _connections ?? (_connections = new List<ConnectionData>()); }
        set { _connections = value; }
    }

    public void RemoveItem(IDiagramNodeItem nodeItem)
    {

    }

    public void AddItem(IDiagramNodeItem item)
    {

    }

    public void Save()
    {
        File.WriteAllText(Path, Serialize().ToString());
    }

    public void RecordUndo(INodeRepository data, string title)
    {
        
    }

    public void MarkDirty(INodeRepository data)
    {
        
    }

    public void SetItemLocation(IDiagramNode node, Vector2 position)
    {
        PositionData[CurrentFilter, node] = position;
    }

    public Vector2 GetItemLocation(IDiagramNode node)
    {
        return PositionData[CurrentFilter, node];
    }

    public void HideNode(string identifier)
    {
        PositionData.Remove(CurrentFilter, identifier);
    }

    public virtual IDiagramFilter CreateDefaultFilter()
    {
        return null;
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
        data.Graph = this;
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

    public IProjectRepository Project { get; private set; }
    public void SetProject(IProjectRepository project)
    {
        Project = project;
        foreach (var item in NodeItems)
        {
            item.Graph = this;
        }
        foreach (var item in ConnectedItems)
        {
            item.Graph = this;
            item.Input = project.AllGraphItems.FirstOrDefault(p => p.Identifier == item.InputIdentifier);
            item.Output = project.AllGraphItems.FirstOrDefault(p => p.Identifier == item.OutputIdentifier);
        }
        ConnectedItems.RemoveAll(p => p.Output == null || p.Input == null);

    }

    public JSONNode Serialize()
    {
        return Serialize(this);
    }

    public static JSONNode Serialize(IGraphData data)
    {
        // The root class for the diagram data
        var root = new JSONClass
        {
            {"Name", new JSONData(data.Name)}, // Name of the diagram
            {"Version", new JSONData(data.Version)},// Version of the diagram
            {"Identifier", new JSONData(data.Identifier)},// Version of the diagram
            {"Type", new JSONData(data.GetType().FullName)}
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
        root.AddObject("RootNode", data.RootFilter as IJsonObject);
        // Nodes
        root.AddObjectArray("Nodes", data.NodeItems.Where(p=>p.Identifier != data.RootFilter.Identifier));

        root.AddObjectArray("ConnectedItems", data.Connections);
        return root;
    }

    public void CleanUpDuplicates()
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

    public void Deserialize(string jsonData)
    {

        if (jsonData == null)
        {
            return;
        }


        var jsonNode = JSONNode.Parse(jsonData);

        DeserializeFromJson(jsonNode);
    }

    public void DeserializeFromJson(JSONNode jsonNode)
    {
        if (jsonNode == null)
        {
            InvertApplication.Log("Couldn't parse file." + Name);
            return;
        }
        if (jsonNode["Name"] != null)
        Name = jsonNode["Name"].Value;

        if (jsonNode["SceneFlow"] is JSONClass)
        {
            RootFilter = jsonNode["SceneFlow"].DeserializeObject(this) as IDiagramFilter;
            var node = RootFilter as IDiagramNode;
            if (node != null)
            {
                node.Graph = this;
            }
        }
        if (jsonNode["RootNode"] is JSONClass)
        {
            RootFilter = jsonNode["RootNode"].DeserializeObject(this) as IDiagramFilter;
            var node = RootFilter as IDiagramNode;
            if (node != null)
            {
                node.Graph = this;
            }
        }

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
            Nodes.AddRange(JsonExtensions.DeserializeObjectArray<IDiagramNode>(jsonNode["Nodes"].AsArray,
                (INodeRepository) this));
        }
        if (jsonNode["ConnectedItems"] is JSONArray)
        {
            ConnectedItems.Clear();
            ConnectedItems.AddRange(JsonExtensions.DeserializeObjectArray<ConnectionData>(
                jsonNode["ConnectedItems"].AsArray, (INodeRepository) this));
        }
        foreach (var item in NodeItems)
        {
            item.Graph = this;
        }
        if (string.IsNullOrEmpty(Version))
        {
            foreach (var filter in NodeItems.OfType<IDiagramFilter>().Concat(new[] {RootFilter}).ToArray())
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
}