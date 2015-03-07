using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;
using UnityEngine;

public class InvertGraph : IGraphData, IItem, IJsonTypeResolver
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
                GraphFileInfo = new FileInfo(value + ".ufgraph");
            }
            else
            {
                GraphFileInfo = new FileInfo(System.IO.Path.Combine(GraphFileInfo.Directory.FullName, value + ".ufgraph"));
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
            List<IDiagramNode> precompiledList;
            
    
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
            if (_rootFilter != null)
            {
                return _rootFilter;
            }
            
            RootFilter = CreateDefaultFilter();
            return _rootFilter;
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

    public string Group
    {
        get { return "Graphs"; }
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
        //File.WriteAllText(Path, Serialize().ToString());
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

    public void RemoveNode(IDiagramNode node)
    {
        if (node == RootFilter) return;
        //foreach (var item in Nodes)
        //{
        //    if (item.Locations.Keys.Contains(item.Identifier))
        //    {
        //        item.Locations.Remove(item.Identifier);
        //    }
        //}
        foreach (var item in PositionData.Positions)
        {
            item.Value.Remove(node.Identifier);
        }
        Nodes.Remove(node);
    }

    public bool DocumentationMode { get; set; }

    public virtual void Document(IDocumentationBuilder docs)
    {
        docs.Title(Name);
        foreach (var node in NodeItems)
        {
            var node1 = node;
            docs.Rows(()=>docs.LinkToNode(node1 as DiagramNode));
        }
        foreach (var node in NodeItems)
        {
            node.Document(docs);
        }
    }

    public void AddConnection(IConnectable output, IConnectable input)
    {
        var connection = new ConnectionData(output.Identifier, input.Identifier)
        {
            Graph = this,
            Output = output,
            Input = input
        };
     

        output.OnConnectionApplied(output, input);
        input.OnConnectionApplied(output, input);
        ConnectedItems.Add(connection);
    }

    public void RemoveConnection(IConnectable output, IConnectable input)
    {
        ConnectedItems.RemoveAll(p => p.OutputIdentifier == output.Identifier && p.InputIdentifier == input.Identifier);
    }

    public void ClearOutput(IConnectable output)
    {
        ConnectedItems.RemoveAll(p => p.OutputIdentifier == output.Identifier);
    }

    public void ClearInput(IConnectable input)
    {
        ConnectedItems.RemoveAll(p => p.InputIdentifier == input.Identifier);
    }

    public IProjectRepository Project { get;  set; }

    public bool Precompiled { get; set; }

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
            item.Input = project.AllGraphItems.FirstOrDefault(p => p.Identifier == item.InputIdentifier) as IConnectable;
            item.Output = project.AllGraphItems.FirstOrDefault(p => p.Identifier == item.OutputIdentifier) as IConnectable;
        }
        //foreach (var item in ConnectedItems)
        //{
        //    if (item.Output == null)
        //    {
        //        InvertApplication.Log(string.Format("Output Identifier {0} couldn't be found on a connection in graph {1}", item.OutputIdentifier, item.Graph.Name));
        //    }
        //    if (item.Input == null)
        //    {
        //        InvertApplication.Log(string.Format("Input Identifier {0} couldn't be found on a connection in graph {1}", item.OutputIdentifier, item.Graph.Name));
        //    }
        //}
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
            {"Type", new JSONData(data.GetType().FullName)},
            {"DocumentationMode", new JSONData(data.DocumentationMode) }
            // Version of the diagram
        };
        // Store the root filter
        root.AddObject("RootNode", data.RootFilter as IJsonObject);
        
        // Nodes
        root.AddObjectArray("Nodes", data.NodeItems.Where(p => p.Identifier != data.RootFilter.Identifier && !p.Precompiled));
        root.AddObjectArray("ConnectedItems", data.Connections);

        if (data.PositionData != null)
        {
            root.AddObject("PositionData", data.PositionData);
        }

        if (data.FilterState != null)
            // Add the filter state
            root.AddObject("FilterState", data.FilterState);

        if (data.Settings != null)
            // Add the settings
            root.AddObject("Settings", data.Settings);

        
     
    
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

    /// <summary>
    /// Gets a list of errors about this node or its children
    /// </summary>
    /// <returns></returns>
    public List<ErrorInfo> Validate()
    {
        var list = new List<ErrorInfo>();
        Validate(list);
        return list;
    }


    /// <summary>
    /// Validates this node decorating a list of errors
    /// </summary>
    /// <param name="errors"></param>
    public virtual void Validate(List<ErrorInfo> errors)
    {
        foreach (var child in this.NodeItems)
        {
            child.Validate(errors);
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
        JsonExtensions.TypeResolver = this;
        if (jsonNode == null)
        {
            InvertApplication.Log("Couldn't parse file." + Name);
            return;
        }
        if (jsonNode["Name"] != null)
        Name = jsonNode["Name"].Value;
        if (jsonNode["DocumentationMode"] != null)
            DocumentationMode = jsonNode["DocumentationMode"].AsBool;
        if (jsonNode["SceneFlow"] is JSONClass)
        {
            RootFilter = jsonNode["SceneFlow"].DeserializeObject() as IDiagramFilter;
            var node = RootFilter as IDiagramNode;
            if (node != null)
            {
                node.Graph = this;
            }
        }
        if (jsonNode["RootNode"] is JSONClass)
        {
            RootFilter = jsonNode["RootNode"].DeserializeObject() as IDiagramFilter;
            var node = RootFilter as IDiagramNode;
            if (node != null)
            {
                node.Graph = this;
            }
        }

        if (jsonNode["PositionData"] != null)
            PositionData = jsonNode["PositionData"].DeserializeObject() as FilterPositionData;


        this.Version = jsonNode["Version"].Value;
        this._identifier = jsonNode["Identifier"].Value;
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
        if (jsonNode["Nodes"] is JSONArray)
        {
            Nodes.Clear();
            //uFrameEditor.Log(this.name + jsonNode["Nodes"].ToString());
            Nodes.AddRange(jsonNode["Nodes"].AsArray.DeserializeObjectArray<IDiagramNode>( (node =>
            {
                var missingNode = new MissingNodeData();
                missingNode.Deserialize(node.AsObject);
                return missingNode;
            })));
        }
        if (jsonNode["ConnectedItems"] is JSONArray)
        {
            ConnectedItems.Clear();
            ConnectedItems.AddRange(jsonNode["ConnectedItems"].AsArray.DeserializeObjectArray<ConnectionData>());
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
        if (Project != null)
        {
            SetProject(Project);
        }
    }

    //public List<IGraphItemEvents> Listeners
    //{
    //    get { return _listeners ?? (_listeners = new List<IGraphItemEvents>()); }
    //    set { _listeners = value; }
    //}

    //public Action Subscribe(IGraphItemEvents handler)
    //{
    //    Listeners.Add(handler);
    //    return () => Unsubscribe(handler);
    //}

    //public void Unsubscribe(IGraphItemEvents handler)
    //{
    //    Listeners.Add(handler);
    //}

    public virtual Type FindType(string clrTypeString)
    {
        var name = clrTypeString.Split(',').FirstOrDefault();
        if (name != null)
        {
            return InvertApplication.FindType(name);
        }
        return null;
        return null;
    }
}