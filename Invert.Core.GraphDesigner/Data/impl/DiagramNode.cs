using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using System.Text.RegularExpressions;
using Invert.Data;
using Invert.Json;
using UnityEngine;
namespace Invert.Core.GraphDesigner
{
    /// <summary>
    /// The base data class for all diagram nodes.
    /// </summary>
    [Browsable(false)]
    public abstract class DiagramNode : IDiagramNode, IDiagramFilter
    {

        public IEnumerable<ConnectionData> Inputs
        {
            get
            {
                foreach (var connectionData in Repository.All<ConnectionData>())
                {
                    if (connectionData.InputIdentifier == this.Identifier)
                    {
                        yield return connectionData;
                    }
                }
            }
        }

        public IEnumerable<ConnectionData> Outputs
        {
            get
            {

                foreach (var connectionData in Repository.All<ConnectionData>())
                {
                    if (connectionData.OutputIdentifier == this.Identifier)
                    {
                        yield return connectionData;
                    }
                }
            }
        }

        public virtual bool AllowInputs
        {
            get { return true; }
        }

        public virtual bool AllowOutputs
        {
            get { return true; }
        }

        public virtual bool AllowMultipleInputs
        {
            get { return true; }
        }

        public virtual bool AllowMultipleOutputs
        {
            get { return true; }
        }

        public virtual void OnConnectionApplied(IConnectable output, IConnectable input)
        {

        }

        public virtual bool CanOutputTo(IConnectable input)
        {
            if (!AllowMultipleOutputs && this.Outputs.Any())
            {
                return false;
            }
            return true;
        }

        public virtual bool CanInputFrom(IConnectable output)
        {
            if (!AllowMultipleInputs && this.Inputs.Any())
            {
                return false;
            }
            return true;
        }

        public virtual void OnOutputConnectionRemoved(IConnectable input)
        {

        }

        public virtual void OnInputConnectionRemoved(IConnectable output)
        {

        }

        public virtual void OnConnectedToInput(IConnectable input)
        {

        }

        public virtual void OnConnectedFromOutput(IConnectable output)
        {

        }

        private FilterCollapsedDictionary _collapsedValues = new FilterCollapsedDictionary();

        private IGraphData _data;

        private DataBag _dataBag = new DataBag();

        private FlagsDictionary _flags = new FlagsDictionary();


        private string _identifier;

        private bool _isCollapsed;

        private Vector2 _location = new Vector2(45f, 45f);


        private FilterLocations _locations = new FilterLocations();


        private string _name;

        private Rect _position;
        private string _graphId;


        public bool this[string flag]
        {
            get
            {
                if (Flags.ContainsKey(flag))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value == false)
                {
                    Flags.Remove(flag);
                    return;
                }
                if (Flags.ContainsKey(flag))
                {

                    Flags[flag] = true;
                }
                else
                {
                    Flags.Add(flag, true);
                }
            }
        }

        [Browsable(false)]
        public FilterCollapsedDictionary CollapsedValues
        {
            get { return _collapsedValues; }
            set { _collapsedValues = value; }
        }

        [JsonProperty, InspectorProperty(InspectorType.TextArea)]
        public string Comments { get; set; }

        [Browsable(false)]
        public virtual bool IsValid
        {
            get { return Validate().Count < 1; }
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
            foreach (var child in this.PersistedItems)
            {
                child.Validate(errors);
            }

        }

        public IGraphItem Copy()
        {
            var jsonNode = new JSONClass();
            Serialize(jsonNode);
            var copy = Activator.CreateInstance(this.GetType()) as DiagramNode;
            copy.Deserialize(jsonNode);
            copy._identifier = null;
            return copy;
        }
        /// <summary>
        /// The items that should be persisted with this diagram node.
        /// </summary>
        public virtual IEnumerable<IDiagramNodeItem> PersistedItems
        {
            get
            {
                return Repository.AllOf<IDiagramNodeItem>().Where(p => p.NodeId == this.Identifier);
            }
            set
            {

            }
        }
        [Browsable(false)]
        public virtual IEnumerable<IGraphItem> GraphItems
        {
            get { return PersistedItems.OfType<IGraphItem>(); }
        }
        [Browsable(false)]
        public Type CurrentType
        {
            get
            {
                return InvertApplication.FindType(FullName);
            }
        }
        [Browsable(false)]
        public DataBag DataBag
        {
            get { return _dataBag ?? (_dataBag = new DataBag()); }
            set { _dataBag = value; }
        }
        [Browsable(false)]
        public Vector2 DefaultLocation
        {
            get { return _location; }
        }

        [JsonProperty]
        public string GraphId
        {
            get { return _graphId; }
            set { _graphId = value;
                Changed = true;
            }
        }

        /// <summary>
        /// Gets the diagram file that this node belongs to
        /// </summary>
        public virtual IGraphData Graph
        {
            get
            {
                if (string.IsNullOrEmpty(GraphId)) return null;
                return Repository.GetById<IGraphData>(GraphId);
            }
            set
            {
                if (value != null) GraphId = value.Identifier;
            }
        }

        public bool Dirty { get; set; }


        [Browsable(false)]
        public FlagsDictionary Flags
        {
            get { return _flags ?? (_flags = new FlagsDictionary()); }
            set { _flags = value; }
        }
        [Browsable(false)]
        public string FullLabel { get { return Name; } }
        [Browsable(false)]
        public virtual string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(Namespace))
                    return string.Format("{0}.{1}", Namespace, Name);

                return Name;
            }
            set { throw new NotImplementedException(); }
        }

  
        public Rect HeaderPosition { get; set; }

        public string Highlighter { get { return null; } }

        public IRepository Repository { get; set; }

 
        public virtual string Identifier
        {
            get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier; }
            set { _identifier = value; }
        }

        public bool Changed { get; set; }

     
        public virtual bool ImportedOnly
        {
            get { return true; }
        }

        public bool IsExplorerCollapsed { get; set; }

        public virtual string InfoLabel
        {
            get
            {
                return null;
            }
        }

  
        public bool IsEditing { get; set; }

        public bool IsExternal
        {
            get
            {
                return Repository.AllOf<IDiagramNode>().All(p => p.Identifier != Identifier);
            }
        }

        public bool IsNewNode { get; set; }

        public bool IsSelectable { get { return true; } }

        public bool IsSelected { get; set; }
    
        public virtual IEnumerable<IDiagramNodeItem> DisplayedItems
        {
            get { return PersistedItems; }
        }

        public abstract string Label { get; }

        public bool Precompiled
        {
            get;
            set;
        }

        [JsonProperty, InspectorProperty]
        public virtual string Name
        {
            get
            {
                //if (this.Graph != null && this.Graph.RootFilter == this)
                //{
                //    return this.Graph.Name;
                //}
                return _name;
            }
            set
            {
                var previous = _name;
                if (value == null) return;
                _name = Regex.Replace(value, "[^a-zA-Z0-9_.]+", "");
                Changed = true;
                if (Graph != null)
                {
                    TrackChange(new NameChange(this, previous, _name));
                    if (this.Graph != null && this.Graph.RootFilter == this)
                    {
                        this.Graph.Name = _name;
                    }
                }
                


                Dirty = true;
            }
        }

        public virtual bool UseStraightLines
        {
            get { return false; }
        }

        public virtual string Namespace
        {
            get
            {
                return Graph.Namespace;
            }
        }

        public string NodeId
        {
            get
            {
                return this.Identifier;
            }
            set
            {
                throw new Exception("Can't set node id on node. Property NodeId ");
            }
        }

        [Browsable(false)]
        public virtual DiagramNode Node
        {
            get
            {
                return this;
            }
            set
            {
            }
        }

        public virtual string OldName
        {
            get;
            set;
        }

        public Rect Position
        {
            get
            {
                return _position;
            }
            set { _position = value; }
        }




        public string Group
        {
            get { return Graph.Name; }
        }

        [Browsable(false)]
        public string SearchTag { get { return Name; } }
        [Browsable(false)]
        public virtual bool ShouldRenameRefactor { get { return true; } }

        public void TrackChange(IChangeData data)
        {
            if (Graph != null)
                Graph.TrackChange(data);
        }

        [Browsable(false)]
        public virtual string SubTitle { get { return string.Empty; } }
        [Browsable(false)]
        public string Title { get { return Name; } }

        protected DiagramNode()
        {
            IsNewNode = true;
        }

        public virtual void BeginEditing()
        {

            OldName = Name;
            IsEditing = true;
        }

        public void BeginRename()
        {
            BeginEditing();
        }


        public virtual void Deserialize(JSONClass cls)
        {
            _name = cls["Name"].Value;
            _isCollapsed = cls["IsCollapsed"].AsBool;
            _identifier = cls["Identifier"].Value;
            PersistedItems = cls["Items"].AsArray.DeserializeObjectArray<IDiagramNodeItem>();

            if (cls["CollapsedValues"] != null)
            {
                CollapsedValues.Deserialize(cls["CollapsedValues"].AsObject);
            }
            if (cls["Flags"] is JSONClass)
            {
                var flags = cls["Flags"].AsObject;
                Flags = new FlagsDictionary();
                Flags.Deserialize(flags);
            }
            if (cls["DataBag"] is JSONClass)
            {
                var flags = cls["DataBag"].AsObject;
                DataBag = new DataBag();
                DataBag.Deserialize(flags);
            }
            if (PersistedItems != null)
            {
                foreach (var item in PersistedItems)
                {
                    item.Node = this;
                }
            }
        }

        public bool EnsureUniqueNames { get; set; }
        public virtual bool EndEditing()
        {
            IsEditing = false;
            // TODO 2.0: Double check
            if (EnsureUniqueNames)
            {
                if (Graph.NodeItems.Count(p => p.Name == Name) > 1)
                {
                    Name = OldName;
                    return false;
                }
            }
            return true;
        }

        public virtual CodeTypeReference GetFieldType(ITypedItem itemData)
        {
            var tRef = new CodeTypeReference(this.Name);
            //tRef.TypeArguments.Add(this.Name);
            return tRef;
        }

        public virtual void NodeAddedInFilter(IDiagramNode newNodeData)
        {

        }


        public virtual void NodeItemRemoved(IDiagramNodeItem diagramNodeItem)
        {
            if (diagramNodeItem.Node == this)
            {
                TrackChange(new GraphItemRemoved()
                {
                    Item = diagramNodeItem,
                    ItemIdentifier = diagramNodeItem.Identifier
                });
            }
            DataBag[diagramNodeItem.Identifier] = null;
            Flags[diagramNodeItem.Identifier] = false;

        }

        public virtual void NodeAdded(IDiagramNode data)
        {

        }

        public virtual void NodeItemAdded(IDiagramNodeItem data)
        {
            if (data.Node == this)
            {
                TrackChange(new GraphItemAdded()
                {
                    Item = data,
                    ItemIdentifier = data.Identifier
                });
            }
        }


        public virtual CodeTypeReference GetPropertyType(ITypedItem itemData)
        {
            return new CodeTypeReference(this.Name);
        }

        public virtual void MoveItemDown(IDiagramNodeItem nodeItem)
        {
        }

        public virtual void MoveItemUp(IDiagramNodeItem nodeItem)
        {

        }

        public virtual void NodeRemoved(IDiagramNode nodeData)
        {
            foreach (var item in PersistedItems)
            {
                if (item != this)
                    item.NodeRemoved(nodeData);
            }

            DataBag[nodeData.Identifier] = null;
            this[nodeData.Identifier] = false;
        }

        void IDiagramNodeItem.NodeItemRemoved(IDiagramNodeItem nodeItem)
        {

            NodeItemRemoved(nodeItem);

        }

        public virtual void RefactorApplied()
        {

        }

        public void Remove(IDiagramNode diagramNode)
        {
            Repository.Remove(diagramNode);
        }

        public virtual void RemoveFromDiagram()
        {
            //Data.RefactorCount--;
        }

        public void RemoveFromFilter(INodeRepository data)
        {
            data.PositionData.Remove(data.CurrentFilter, this.Identifier);
        }

        public void Rename(IDiagramNode data, string name)
        {
            Rename(name);
        }

        public virtual void Rename(string newName)
        {
            Name = newName;
        }

        public virtual void Serialize(JSONClass cls)
        {
            cls.Add("Name", new JSONData(_name));
            cls.Add("Identifier", new JSONData(_identifier));

            cls.AddObjectArray("Items", PersistedItems.Where(p => !p.Precompiled));

            //cls.Add("Locations", Locations.Serialize());
            cls.Add("CollapsedValues", CollapsedValues.Serialize());

            cls.AddObject("Flags", Flags);
            cls.AddObject("DataBag", DataBag);
        }


        public void RemoveItem(IDiagramNodeItem item)
        {

        }
        
        public virtual string RelatedType
        {
            get { return this.Identifier; }
            set
            {

            }
        }
        
        public virtual string RelatedTypeName
        {
            get { return this.Name; }
            set { throw new NotImplementedException(); }
        }

        public void RemoveType()
        {

        }

        public string AssemblyQualifiedName { get; set; }

        public CodeTypeReference GetFieldType()
        {
            return new CodeTypeReference(this.Name);
        }

        public CodeTypeReference GetPropertyType()
        {
            return new CodeTypeReference(this.Name);
        }



        public virtual void Document(IDocumentationBuilder docs)
        {
            docs.Section(this.Node.Name);
            docs.NodeImage(this);
            docs.Paragraph(this.Comments);
            foreach (var item in PersistedItems)
                item.Document(docs);
        }
    }

    public enum NodeScope
    {
        None,
        Node,
        Graph,
        Project,
        NodeAndType,
        GraphAndType,
        ProjectAndType
    }
}

