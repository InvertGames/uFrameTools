using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;
using UnityEngine;

public class ScaffoldGraph : InvertGraph
{
    public ScaffoldGraph BeginNode<TNode>(string name) where TNode : GenericNode, new()
    {
        CurrentNode = new TNode()
        {
            Name = name
        };
        AddNode(CurrentNode);
        return this;
    }
    public ScaffoldGraph AddItem<TNodeItem>(string name, out TNodeItem nodeItem, string type = null) where TNodeItem : IDiagramNodeItem, new()
    {
        var item = new TNodeItem()
        {
            Name = name,
            Node = CurrentNode,
        };
        if (type != null)
        {
            var typedItem = item as ITypedItem;
            typedItem.RelatedType = type;
        }
        CurrentNode.ChildItems.Add(item);
        nodeItem = item;
        return this;
    }
    public ScaffoldGraph AddItem<TNodeItem>(string name, string type = null) where TNodeItem : IDiagramNodeItem, new()
    {
        var item = new TNodeItem()
        {
            Name = name,
            Node = CurrentNode,
        };
        if (type != null)
        {
            var typedItem = item as ITypedItem;
            typedItem.RelatedType = type;
        }
        CurrentNode.ChildItems.Add(item);
        return this;
    }
    public GenericNode CurrentNode { get; set; }

    public GenericNode EndNode()
    {
        return CurrentNode;
    }
}
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
    private List<IChangeData> _changeData;


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

    public List<IChangeData> ChangeData
    {
        get { return _changeData ?? (_changeData = new List<IChangeData>()); }
        set { _changeData = value; }
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
        if (PositionData == null || CurrentFilter == null || node == null)
            return Vector2.zero;

        return PositionData[CurrentFilter, node];
    }

    public void HideNode(string identifier)
    {
        PositionData.Remove(CurrentFilter, identifier);
    }

    IEnumerable<ErrorInfo> INodeRepository.Validate()
    {
        return Validate();
    }

    public void TrackChange(IChangeData data)
    {
        if (Project != null)
            Project.TrackChange(data);
        var existing = ChangeData.FirstOrDefault(p => p.Item == data.Item && p.GetType() == data.GetType());
        if (existing != null)
        {
            existing.Update(data);
        }
        else
        {
            ChangeData.Add(data);
        }
        ChangeData.RemoveAll(p => !p.IsValid);
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
        TrackChange(new GraphItemAdded()
        {
            Item = data,
            ItemIdentifier = data.Identifier
        });
        Nodes.Add(data);
    }

    public void RemoveNode(IDiagramNode node, bool removePositionData = true)
    {
        if (node == RootFilter) return;
        //foreach (var item in Nodes)
        //{
        //    if (item.Locations.Keys.Contains(item.Identifier))
        //    {
        //        item.Locations.Remove(item.Identifier);
        //    }
        //}
        if (removePositionData)
        {
            foreach (var item in PositionData.Positions)
            {
                item.Value.Remove(node.Identifier);
            }
        }
          TrackChange(new GraphItemRemoved()
            {
                Item = node,
                ItemIdentifier = node.Identifier
            });
        Nodes.Remove(node);
    }

    public bool DocumentationMode { get; set; }

    public virtual void Document(IDocumentationBuilder docs)
    {
        docs.Title(Name);
        foreach (var node in NodeItems)
        {
            var node1 = node;
            docs.Rows(() => docs.LinkToNode(node1 as DiagramNode));
        }
        foreach (var node in NodeItems)
        {
            node.Document(docs);
        }
    }



    public void AddConnection(IConnectable output, IConnectable input)
    {
        if (ConnectedItems.Any(p => p.InputIdentifier == input.Identifier && p.OutputIdentifier == output.Identifier)) return;
        var connection = new ConnectionData(output.Identifier, input.Identifier)
        {
            Graph = this,
            Output = output,
            Input = input
        };
        if (!output.AllowMultipleOutputs)
        {
            ClearOutput(output);
        }
        if (!input.AllowMultipleInputs)
        {
            ClearInput(input);
        }
        output.OnConnectedToInput(input);
        input.OnConnectedFromOutput(output);
        ConnectedItems.Add(connection);
        if (Project != null)
        {
            Project.MarkDirty(this);
            if (output.Graph != this)
            {
                Project.MarkDirty(output.Graph);
            }
            if (input.Graph != this)
            {
                Project.MarkDirty(input.Graph);
            }
        }
    }

    /// <summary>
    /// DO NOT USE! For upgrading projects only! Use other overload
    /// </summary>
    /// <param name="output"></param>
    /// <param name="input"></param>
    public void AddConnection(string output, string input)
    {
        if (ConnectedItems.Any(p => p.InputIdentifier == input && p.OutputIdentifier == output)) return;
        var connection = new ConnectionData(output, input)
        {
            Graph = this,
            Output = AllGraphItems.FirstOrDefault(p => p.Identifier == output) as IConnectable,
            Input = AllGraphItems.FirstOrDefault(p => p.Identifier == input) as IConnectable
        };
      
        ConnectedItems.Add(connection);
    }


    /// <summary>
    /// Removes a connection from this graph
    /// </summary>
    /// <param name="output">The output of the connection.</param>
    /// <param name="input">The input of the connection.</param>
    public void RemoveConnection(IConnectable output, IConnectable input)
    {
        output.OnOutputConnectionRemoved(input);
        input.OnInputConnectionRemoved(output);
        ConnectedItems.RemoveAll(p => p.OutputIdentifier == output.Identifier && p.InputIdentifier == input.Identifier);
    }

    /// <summary>
    /// Removes all connections from an output
    /// </summary>
    /// <param name="output"></param>
    public void ClearOutput(IConnectable output)
    {
        ConnectedItems.RemoveAll(p => p.OutputIdentifier == output.Identifier);
    }
    /// <summary>
    /// Removes all connections to an input
    /// </summary>
    /// <param name="output"></param>
    public void ClearInput(IConnectable input)
    {
        ConnectedItems.RemoveAll(p => p.InputIdentifier == input.Identifier);
    }

    public IProjectRepository Project { get; set; }

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
        this.Prepare();

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

        root.AddObjectArray("Changes", data.ChangeData);



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
            Nodes.AddRange(jsonNode["Nodes"].AsArray.DeserializeObjectArray<IDiagramNode>((node =>
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
        if (jsonNode["Changes"] is JSONArray)
        {
            ChangeData.Clear();
            ChangeData.AddRange(jsonNode["Changes"].AsArray.DeserializeObjectArray<IChangeData>());
            foreach (var item in ChangeData)
            {
                item.Item = AllGraphItems.OfType<IDiagramNodeItem>().FirstOrDefault(p => p.Identifier == item.ItemIdentifier);
            }
            ChangeData.RemoveAll(p => p.Item == null);
        }
        foreach (var item in NodeItems)
        {
            item.Graph = this;
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