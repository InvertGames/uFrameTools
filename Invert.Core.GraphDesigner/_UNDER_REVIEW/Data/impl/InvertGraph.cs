using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Invert.Core.GraphDesigner
{
    public class InvertGraph : IGraphData, IItem, IJsonTypeResolver, IDataRecordRemoved
    {
        private FilterPositionData _positionData;

        private List<IDiagramNode> _nodes = new List<IDiagramNode>();

        private string _identifier;

        private ElementDiagramSettings _settings = new ElementDiagramSettings();

        //private FilterState _filterState = new FilterState();

        private IDiagramFilter _rootFilter;
        private bool _errors;
        private List<ConnectionData> _connections;
        private string _ns;
        private List<IChangeData> _changeData;
        private IDiagramFilter[] _filterStack;
        private string _rootFilterId;

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
            get
            {
                var connectionProvider = CurrentFilter as IConnectionDataProvider;

                if (connectionProvider != null)
                {
                    foreach (var item in connectionProvider.Connections)
                    {
                        yield return item;
                    }
                }
                foreach (var item in ConnectedItems)
                {
                    yield return item;
                }
            }
        }

        public FilterPositionData PositionData
        {
            get { return _positionData ?? (_positionData = new FilterPositionData()); }
            set { _positionData = value; }
        }

        public IDiagramFilter[] FilterStack
        {
            get
            {
                return _filterStack ?? (_filterStack = Repository.All<FilterStackItem>()
                  .Where(p => p.GraphId == this.Identifier)
                  .OrderByDescending(p => p.Index)
                  .Select(p => p.Filter).ToArray()
                  );
            }
            set { _filterStack = value; }
        }

        public void PushFilter(IDiagramFilter filter)
        {
            var filterStack = Repository.Create<FilterStackItem>();
            filterStack.GraphId = this.Identifier;
            filterStack.FilterId = filter.Identifier;
            filterStack.Index = FilterStack.Count();
            // Reset the lazy filter stack
            _filterStack = null;

        }

        public void PopFilter()
        {
            if (FilterStack.Length > 0)
                Repository.RemoveAll<FilterStackItem>(p => p.GraphId == this.Identifier && p.FilterId == CurrentFilter.Identifier);

            _filterStack = null;
        }
        public void PopToFilter(IDiagramFilter filter1)
        {
            if (filter1 == null)
            {
                filter1 = FilterStack.First();
            }
            foreach (var item in FilterStack)
            {
                if (item != filter1)
                {
                    InvertApplication.Log(string.Format("Removing filter {0}", item.Name));
                    var item1 = item;
                    Repository.RemoveAll<FilterStackItem>(p => p.GraphId == this.Identifier && p.FilterId == item1.Identifier);
                }
            }
            // Reset the lazy filter stack
            _filterStack = null;
        }

        public void PopToFilterById(string filterId)
        {
            PopToFilter(FilterStack.FirstOrDefault(p => p.Identifier == filterId));
        }

        public IDiagramFilter CurrentFilter
        {
            get
            {
                if (FilterStack.Length < 1)
                {
                    return RootFilter;
                }
                return FilterStack.First();
            }
        }

        public List<IChangeData> ChangeData
        {
            get { return _changeData ?? (_changeData = new List<IChangeData>()); }
            set { _changeData = value; }
        }

        [JsonProperty]
        public string Identifier
        {
            get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier; }
            set { _identifier = value; }
        }

        public bool Changed { get; set; }

        public ElementDiagramSettings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }

#if UNITY_DLL

        public string SystemPath
        {
            get { return Path.Combine(Application.dataPath, AssetPath.Substring(7)).Replace("\\", "/").ToLower(); }
            set
            {
            }
        }

        //[JsonProperty]
        public string Name
        {
            get
            {
                if (Repository != null)
                {
                    return RootFilter.Name;
                }
                return this.GetType().Name;
            }
            set
            {
                if (Repository != null)
                {
                    RootFilter.Name = value;
                }
            }
        }

#else
    public string SystemPath
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

        public string SystemDirectory
        {
            get
            {
                return Path.GetDirectoryName(SystemPath);
            }
        }

        [JsonProperty]
        public string Version { get; set; }

        public int RefactorCount { get; set; }

        public virtual IEnumerable<IDiagramNode> NodeItems
        {
            get
            {
                // TODO 2.0 Handler RootFilter
                //if (RootFilter is IDiagramNode && !Nodes.Contains(RootFilter as IDiagramNode))
                //    yield return RootFilter as IDiagramNode;

                foreach (var item in Repository.AllOf<IDiagramNode>().Where(p => p.GraphId == this.Identifier))
                    yield return item;
            }
        }

        //public FilterState FilterState
        //{
        //    get { return _filterState; }
        //    set { _filterState = value; }
        //}

        [JsonProperty]
        public string RootFilterId
        {
            get { return _rootFilterId; }
            set {
                this.Changed("RootFilterId", ref _rootFilterId, value);
            }
        }

        public virtual IDiagramFilter RootFilter
        {
            get
            {

                if (_rootFilter != null)
                {
                    return _rootFilter;
                }
                if (!string.IsNullOrEmpty(RootFilterId))
                {
                    _rootFilter = Repository.GetById<IDiagramFilter>(RootFilterId);
                    var asNode = _rootFilter as IDiagramNode;
                    if (asNode != null)
                    {
                        asNode.GraphId = this.Identifier;
                    }
                }
                else
                {
                    RootFilter = CreateDefaultFilter();
                }
                return _rootFilter;
            }
            set
            {
                _rootFilter = value;
                if (value != null)
                {
                    RootFilterId = value.Identifier;
                }
                var asNode = value as IDiagramNode;
                if (asNode != null)
                {
                    asNode.GraphId = this.Identifier;
                    asNode.Name = this.Name;
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

        public IRepository Repository { get; set; }

        [Obsolete]
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

#if UNITY_DLL

        public string AssetPath
        {
            get
            {
                var proj = Project as DefaultProjectRepository;

                var graph = proj.Graphs.FirstOrDefault(p => p.Identifier == this.Identifier);
                return UnityEditor.AssetDatabase.GetAssetPath(graph as ScriptableObject);
            }
        }

        public string AssetDirectory
        {
            get { return System.IO.Path.GetDirectoryName(AssetPath); }
        }

#endif

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

        }

        public void AddNode(IDiagramNode data)
        {
            data.GraphId = this.Identifier;
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


        public string Directory
        {
            get { return System.IO.Path.GetDirectoryName(SystemPath); }
        }

        public void AddConnection(IConnectable output, IConnectable input)
        {


            if (!output.AllowMultipleOutputs)
            {
                ClearOutput(output);
            }
            if (!input.AllowMultipleInputs)
            {
                ClearInput(input);
            }

            var connect = Repository.Create<ConnectionData>();
            connect.OutputIdentifier = output.Identifier;
            connect.InputIdentifier = input.Identifier;
            output.OnConnectedToInput(input);
            input.OnConnectedFromOutput(output);
        }

        public void AddConnection(string output, string input)
        {
            if (output == null) throw new ArgumentNullException("output");
            if (input == null) throw new ArgumentNullException("input");

            var connect = Repository.Create<ConnectionData>();
            connect.OutputIdentifier = output;
            connect.InputIdentifier = input;
        }

        /// <summary>
        /// Removes a connection from this graph
        /// </summary>
        /// <param name="output">The output of the connection.</param>
        /// <param name="input">The input of the connection.</param>
        public void RemoveConnection(IConnectable output, IConnectable input)
        {
            if (output == null) throw new ArgumentNullException("output");
            if (input == null) throw new ArgumentNullException("input");
            Repository.RemoveAll<ConnectionData>(p => p != null && p.OutputIdentifier == output.Identifier && p.InputIdentifier == input.Identifier);

            output.OnOutputConnectionRemoved(input);
            input.OnInputConnectionRemoved(output);
            //        ConnectedItems.RemoveAll(p => p.OutputIdentifier == output.Identifier && p.InputIdentifier == input.Identifier);
        }

        /// <summary>
        /// Removes all connections from an output
        /// </summary>
        /// <param name="output"></param>
        public void ClearOutput(IConnectable output)
        {
            Repository.RemoveAll<ConnectionData>(_ => _.OutputIdentifier == output.Identifier);
            //ConnectedItems.RemoveAll(p => p.OutputIdentifier == output.Identifier);
        }

        /// <summary>
        /// Removes all connections to an input
        /// </summary>
        /// <param name="input"></param>
        public void ClearInput(IConnectable input)
        {
            Repository.RemoveAll<ConnectionData>(_ => _.InputIdentifier == input.Identifier);
            //        ConnectedItems.RemoveAll(p => p.InputIdentifier == input.Identifier);
        }

        public IProjectRepository Project { get; set; }

        public bool Precompiled { get; set; }

        public void SetProject(IProjectRepository project)
        {
            //Project = project;
            //foreach (var item in NodeItems)
            //{
            //    item.Graph = this;
            //}
            //foreach (var item in ConnectedItems)
            //{
            //    item.Graph = this;
            //    item.Input = project.AllGraphItems.FirstOrDefault(p => p.Identifier == item.InputIdentifier) as IConnectable;
            //    item.Output = project.AllGraphItems.FirstOrDefault(p => p.Identifier == item.OutputIdentifier) as IConnectable;
            //}
            //foreach (var item in ChangeData)
            //{
            //    item.Item = AllGraphItems.OfType<IDiagramNodeItem>().FirstOrDefault(p => p.Identifier == item.ItemIdentifier);
            //}
            //ChangeData.RemoveAll(p => p.Item == null);
            //ConnectedItems.RemoveAll(p => p.Output == null || p.Input == null);
            //this.Prepare();
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
       
            // Version of the diagram
        };

            // Store the root filter
            root.AddObject("RootNode", data.RootFilter as IJsonObject);

            // Nodes
            root.AddObjectArray("Nodes", data.NodeItems.Where(p => p.Identifier != data.RootFilter.Identifier && !p.Precompiled));
            if (data.PositionData != null)
            {
                root.AddObject("PositionData", data.PositionData);
            }

            //if (data.FilterState != null)
            //    // Add the filter state
            //    root.AddObject("FilterState", data.FilterState);

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

            if (jsonNode["RootNode"] is JSONClass)
            {
                RootFilter = jsonNode["RootNode"].DeserializeObject() as IDiagramFilter;
                var node = RootFilter as IDiagramNode;
                if (node != null)
                {
                    node.GraphId = this.Identifier;
                }
            }

            if (jsonNode["PositionData"] != null)
                PositionData = jsonNode["PositionData"].DeserializeObject() as FilterPositionData;

            this.Version = jsonNode["Version"].Value;
            this._identifier = jsonNode["Identifier"].Value;


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
            }
            if (Project != null)
            {
                SetProject(Project);
            }
        }

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

        public void RecordRemoved(IDataRecord record)
        {
            if (CurrentFilter == record)
            {
                PopFilter();
            }
        }
    }


    public class FilterStackItem : IDataRecord, IDataRecordRemoved
    {
        private string _graphId;
        private string _filterId;
        private int _index;

        public IRepository Repository { get; set; }
        public string Identifier { get; set; }

        public bool Changed { get; set; }

        [JsonProperty]
        public string GraphId
        {
            get { return _graphId; }
            set {
                this.Changed("GraphId",ref  _graphId, value);
            }
        }

        [JsonProperty]
        public string FilterId
        {
            get { return _filterId; }
            set {
                this.Changed("FilterId",ref _filterId, value);
            }
        }

        public IGraphData Graph
        {
            get { return Repository.GetById<IGraphData>(GraphId); }
        }

        public IDiagramFilter Filter
        {
            get { return Repository.GetById<IDiagramFilter>(FilterId); }
        }

        [JsonProperty]
        public int Index
        {
            get { return _index; }
            set {
                this.Changed("Index", ref _index, value);
            }
        }

        public void RecordRemoved(IDataRecord record)
        {
            if (record.Identifier == GraphId || record.Identifier == FilterId)
            {
                Repository.Remove(this);
            }
        }
    }
}