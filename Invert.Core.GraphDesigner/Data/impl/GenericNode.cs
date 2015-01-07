using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Invert.Json;

namespace Invert.Core.GraphDesigner
{
    public interface IMultiSlot { }
    public class MultiOutputSlot<TFor> : GenericSlot, IMultiSlot
    {
        [Browsable(false)]
        public IEnumerable<TFor> Items
        {
            get { return Outputs.Select(p => p.Input).OfType<TFor>(); }
        }

        public override bool Validate(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            if (a.Node == b.Node) return false;
            return base.Validate(a, b);
        }
    }
    public class SingleOutputSlot<TFor> : GenericSlot
    {
        [Browsable(false)]
        public TFor Item
        {
            get { return Outputs.Select(p => p.Input).OfType<TFor>().FirstOrDefault(); }
        }
        public override bool Validate(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            if (a.Node == b.Node) return false;
            return base.Validate(a, b);
        }
    }
    public class MultiInputSlot<TFor> : GenericSlot, IMultiSlot
    {
        [Browsable(false)]
        public IEnumerable<TFor> Items
        {
            get { return Inputs.Select(p=>p.Output).OfType<TFor>(); }
        }
        public override bool Validate(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            if (a.Node == b.Node) return false;
            return base.Validate(a, b);
        }
    }
    public class SingleInputSlot<TFor> : GenericSlot
    {
        [Browsable(false)]
        public TFor Item
        {
            get { return Inputs.Select(p => p.Output).OfType<TFor>().FirstOrDefault(); }
        }
        public override bool Validate(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            if (a.Node == b.Node) return false;
            return base.Validate(a, b);
        }

    }
    public class InheritanceSlot<TFor> : GenericSlot
    {
        [Browsable(false)]
        public TFor Item
        {
            get { return Inputs.Select(p => p.Output).OfType<TFor>().FirstOrDefault(); }
        }

        public override bool Validate(IDiagramNodeItem a, IDiagramNodeItem b)
        {
           
            var result = a is TFor && b is BaseClassReference && b.Node != a.Node && b.Node.GetType() == a.GetType();
            return result;
        }
    }
    public class ReferenceSection<TReference> : GenericReferenceItem<TReference>
    {
        
    }

    public class GenericSlot : GenericNodeChildItem
    {
        public virtual bool Validate(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            
           return a != b;
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonProperty : GeneratorProperty
    {

    }

    public class GeneratorProperty : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Property)]
    public class NodeProperty : InspectorProperty
    {
        public NodeProperty()
        {
        }

        public NodeProperty(InspectorType inspectorType)
            : base(inspectorType)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class InspectorProperty : Attribute
    {
        public InspectorType InspectorType { get; set; }

        public Type CustomDrawerType { get; set; }


        public InspectorProperty(Type customDrawerType)
        {
            CustomDrawerType = customDrawerType;
        }

        public InspectorProperty()
        {
            InspectorType = InspectorType.Auto;
        }

        public InspectorProperty(InspectorType inspectorType)
        {
            InspectorType = inspectorType;
        }
    }

    public enum InspectorType
    {
        Auto,
        TextArea,
        TypeSelection
    }
    [Browsable(false)]
    public class GenericNode : DiagramNode, IConnectable
    {
        private List<IDiagramNodeItem> _childItems = new List<IDiagramNodeItem>();
        private List<string> _connectedGraphItemIds = new List<string>();
        [Browsable(false)]
        public List<IDiagramNodeItem> ChildItems
        {
            get { return _childItems; }
            set { _childItems = value; }
        }
        [Browsable(false)]
        public NodeConfigBase Config
        {
            get
            {
                return InvertApplication.Container.Resolve<NodeConfigBase>(this.GetType().Name);
            }
        }

        public override bool ValidateInput(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            //return false;
            return base.ValidateInput(a, b);
        }

        public override bool ValidateOutput(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            //return false;
            return base.ValidateOutput(a, b);
        }

        [GeneratorProperty]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }
        //public List<string> ConnectedGraphItemIds
        //{
        //    get { return _connectedGraphItemIds; }
        //    set { _connectedGraphItemIds = value; }
        //}

        //public IEnumerable<IGraphItem> ConnectedGraphItems
        //{
        //    get
        //    {
        //        foreach (var item in Project.NodeItems)
        //        {
        //            if (ConnectedGraphItemIds.Contains(item.Identifier))
        //                yield return item;

        //            foreach (var child in item.ContainedItems)
        //            {
        //                if (ConnectedGraphItemIds.Contains(child.Identifier))
        //                {
        //                    yield return child;
        //                }
        //            }
        //        }
        //    }
        //}
        [Browsable(false)]
        public override IEnumerable<IDiagramNodeItem> PersistedItems
        {
            get { return ChildItems; }
            set
            {
                ChildItems = value.ToList();
                foreach (var item in ChildItems)
                {
                    item.Node = this;
                }
            }
        }
        [Browsable(false)]
        public IEnumerable<ConnectionData> Inputs
        {
            get
            {

                foreach (var connectionData in Node.Project.Connections)
                {
                    if (connectionData.InputIdentifier == this.Identifier)
                    {
                        yield return connectionData;
                    }
                }
            }
        }
        [Browsable(false)]
        public IEnumerable<ConnectionData> Outputs
        {
            get
            {
                foreach (var connectionData in Node.Project.Connections)
                {
                    if (connectionData.OutputIdentifier == this.Identifier)
                    {

                        yield return connectionData;
                    }
                }
            }
        }
        [Browsable(false)]
        public override IEnumerable<Refactorer> Refactorings
        {
            get
            {
                foreach (var refactorer in Config.Refactorers)
                {
                    var r = refactorer(this);
                    if (r != null)
                    {
                        yield return r;
                    }
                }
            }
        }

        [Browsable(false)]
        public override IEnumerable<IDiagramNodeItem> DisplayedItems
        {
            get
            {
                return ChildItems;
            }
        }
        [Browsable(false)]
        public override string Label
        {
            get { return Name; }
        }



        public void AddReferenceItem(IGraphItem item, NodeConfigSectionBase mirrorSection)
        {
            AddReferenceItem(ChildItems.Where(p => p.GetType() == mirrorSection.ReferenceType).Cast<GenericReferenceItem>().ToArray(), item, mirrorSection);
        }

        public override void Deserialize(JSONClass cls, INodeRepository repository)
        {
            base.Deserialize(cls, repository);
            var inputSlotInfos = GetInputSlotInfos(this.GetType());
            foreach (var item in inputSlotInfos)
            {
                var propertyName = item.Key.Name;
                if (cls[propertyName] == null) continue;
                var slotObject = cls[propertyName].DeserializeObject(repository, item.Key.PropertyType.GetGenericArguments().FirstOrDefault()) as GenericSlot;
                if (slotObject == null) continue;

                slotObject.Node = this;
                slotObject.Name = item.Value.Name;
                if (item.Key.PropertyType.IsAssignableFrom(slotObject.GetType()))
                {
                    item.Key.SetValue(this, slotObject, null);
                }
                else
                {
                    // If the type has changed lets keep that same identifier for connections
                    var value = Activator.CreateInstance(item.Key.PropertyType) as GenericSlot;
                    value.Identifier = slotObject.Identifier;
                    value.Node = this;
                    
                    item.Key.SetValue(this,value,null);
                }
                    
            }
            var outputSlotInfos = GetOutputSlotInfos(this.GetType());
            foreach (var item in outputSlotInfos)
            {
                var propertyName = item.Key.Name;
                if (cls[propertyName] == null) continue;
                var slotObject = cls[propertyName].DeserializeObject(repository, item.Key.PropertyType.GetGenericArguments().FirstOrDefault()) as GenericSlot;
                if (slotObject == null) continue;
                slotObject.Node = this;
                slotObject.Name = item.Value.Name;
                if (item.Key.PropertyType.IsAssignableFrom(slotObject.GetType()))
                {
                    item.Key.SetValue(this, slotObject, null);
                }
                else
                {
                    
                    // If the type has changed lets keep that same identifier for connections
                    var value = Activator.CreateInstance(item.Key.PropertyType) as GenericSlot;
                    value.Identifier = slotObject.Identifier;
                    value.Node = this;
                    item.Key.SetValue(this, value, null);
                }
            }

            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(typeof(JsonProperty), true).Length < 1) continue;
                this.DeserializeProperty(property, cls);
            }
        }

       

        //public TItem GetConnection<TConnectionType, TItem>() where TConnectionType : GenericConnectionReference, new()
        //{
        //    return (TItem)GetConnectionReference<TConnectionType>().ConnectedGraphItems.FirstOrDefault();
        //}

        public TType GetConnectionReference<TType>()
            where TType : GenericSlot, new()
        {
            return (TType)GetConnectionReference(typeof(TType));
        }

        public GenericSlot GetConnectionReference(Type inputType)
        {
            var item = ChildItems.FirstOrDefault(p => inputType.IsAssignableFrom(p.GetType()));
            if (item == null)
            {
                var input = Activator.CreateInstance(inputType) as GenericSlot;
                input.Node = this;
                ChildItems.Add(input);
                return input;
            }

            return item as GenericSlot;
        }

        //public IEnumerable<TItem> GetConnections<TConnectionType, TItem>() where TConnectionType : GenericConnectionReference, new()
        //{
        //    return GetConnectionReference<TConnectionType>().ConnectedGraphItems.Cast<TItem>();
        //}

        //public IEnumerable<TChildItem> GetInputChildItems<TSourceNode, TChildItem>()
        //    where TSourceNode : GenericNode
        //{
        //    return InputGraphItems.OfType<TSourceNode>().SelectMany(p => p.ContainedItems.OfType<TChildItem>());
        //}

        //public IEnumerable<TChildItem> GetInputInheritedChildItems<TSourceNode, TChildItem>()
        //  where TSourceNode : GenericInheritableNode
        //{
        //    return InputGraphItems.OfType<TSourceNode>().SelectMany(p => p.ChildItemsWithInherited.OfType<TChildItem>());
        //}

        public override void NodeAddedInFilter(IDiagramNode newNodeData)
        {
            base.NodeAddedInFilter(newNodeData);
        }

        public override void NodeItemAdded(IDiagramNodeItem data)
        {
            base.NodeItemAdded(data);
            UpdateReferences();
        }

        public override void NodeItemRemoved(IDiagramNodeItem diagramNodeItem)
        {
            base.NodeItemRemoved(diagramNodeItem);
            //UpdateReferences();
            ChildItems.RemoveAll(
                p =>
                    p.Identifier == diagramNodeItem.Identifier ||
                    (p is GenericReferenceItem && ((GenericReferenceItem)p).SourceIdentifier == diagramNodeItem.Identifier));
        }
        [Browsable(false)]
        public override bool IsValid
        {
            get { return Config.IsValid(this); }
        }
        public override void NodeRemoved(IDiagramNode nodeData)
        {
            base.NodeRemoved(nodeData);

        }


        public override void Serialize(JSONClass cls)
        {
            base.Serialize(cls);
            if (Config == null)
            {
                throw new Exception(string.Format("Config for node {0} couldn't be found.", this.GetType().Name));
            }
            var inputSlotInfos = Config.InputSlots;
            foreach (var item in inputSlotInfos)
            {
                if (item.Key == null) continue;
                var propertyName = item.Key.Name;
                var slot = item.Key.GetValue(this, null) as GenericSlot;
                if (slot == null) continue;
                cls.AddObject(propertyName, slot);
            }
            var outputSlotInfos =Config.OutputSlots;
            foreach (var item in outputSlotInfos)
            {
                if (item.Key == null) continue;
                ;
                var propertyName = item.Key.Name;
                var slot = item.Key.GetValue(this, null) as GenericSlot;
                if (slot == null) continue;
                cls.AddObject(propertyName, slot);
            }
             
           // var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in Config.SerializedProperties)
            {
                // if (property.GetCustomAttributes(typeof(JsonProperty), true).Length < 1) continue;
                this.SerializeProperty(property, cls);
            }
        }

       

        private void UpdateReferences()
        {
            foreach (var mirrorSection in Config.Sections.Where(p => p.ReferenceType != null && !p.AllowAdding))
            {
                NodeConfigSectionBase section = mirrorSection;
                var mirrorItems = ChildItems.Where(p => p.GetType() == section.ReferenceType).Cast<GenericReferenceItem>().ToArray();
                var newItems = mirrorSection.GenericSelector(this).ToArray();
                var newItemIds = newItems.Select(p => p.Identifier);

                foreach (var item in newItems)
                {
                    if (ChildItems.OfType<GenericReferenceItem>().Any(p => p.SourceIdentifier == item.Identifier)) continue;
                    AddReferenceItem(mirrorItems, item, mirrorSection);
                }
                ChildItems.RemoveAll(p => p.GetType() == section.SourceType && p is GenericReferenceItem && !newItemIds.Contains(((GenericReferenceItem)p).SourceIdentifier));
            }
        }

        internal void AddReferenceItem(GenericReferenceItem[] mirrorItems, IGraphItem item, NodeConfigSectionBase mirrorSection)
        {
            var current = mirrorItems.FirstOrDefault(p => p.SourceIdentifier == item.Identifier);
            if (current != null && !mirrorSection.AllowDuplicates) return;

            var newMirror = Activator.CreateInstance(mirrorSection.SourceType) as GenericReferenceItem;
            newMirror.SourceIdentifier = item.Identifier;
            newMirror.Node = this;
            ChildItems.Add(newMirror);
        }
        [Browsable(false)]
        public override IEnumerable<IGraphItem> GraphItems
        {
            get
            {
                foreach (var item in this.PersistedItems)
                {
                    yield return item;
                }
                foreach (var item in AllSlots)
                {
                    yield return item;
                }
            }
        }
        [Browsable(false)]
        public virtual IEnumerable<GenericSlot> AllInputSlots
        {
            get
            {
                foreach (var slot in Config.GraphItemConfigurations.OfType<NodeInputConfig>())
                {
                    if (!slot.IsInput) continue;
                    yield return slot.GetDataObject(this) as GenericSlot;
                }

            }
        }
        [Browsable(false)]
        public virtual IEnumerable<GenericSlot> AllOutputSlots
        {
            get
            {
                foreach (var slot in Config.GraphItemConfigurations.OfType<NodeInputConfig>())
                {
                    if (!slot.IsOutput) continue;
                    yield return slot.GetDataObject(this) as GenericSlot;
                }

            }
        }
        [Browsable(false)]
        public virtual IEnumerable<GenericSlot> AllSlots
        {
            get
            {
                foreach (var slot in Config.GraphItemConfigurations.OfType<NodeInputConfig>())
                {
                    yield return slot.GetDataObject(this) as GenericSlot;
                }
                
            }
        }
        [Browsable(false)]
        public virtual IEnumerable<NodeInputConfig> SlotConfigurations
        {
            get
            {
                foreach (var slot in Config.GraphItemConfigurations.OfType<NodeInputConfig>())
                {
                    yield return slot;
                }

            }
        } 
        public static IEnumerable<KeyValuePair<PropertyInfo, InputSlot>> GetInputSlotInfos(Type nodeType)
        {
            
            return nodeType.GetPropertiesWithAttribute<InputSlot>();
        }
        public static IEnumerable<KeyValuePair<PropertyInfo, OutputSlot>> GetOutputSlotInfos(Type nodeType)
        {
            return nodeType.GetPropertiesWithAttribute<OutputSlot>();
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, Section>> GetSections(Type nodeType)
        {
            return nodeType.GetPropertiesWithAttribute<Section>();
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, ReferenceSection>> GetReferenceSections(Type nodeType)
        {
            return nodeType.GetPropertiesWithAttribute<ReferenceSection>();
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, ReferenceSection>> GetProxySections(Type nodeType)
        {
            return nodeType.GetPropertiesWithAttribute<ReferenceSection>();
        }
    }

    public class GenericReferenceItem<TSourceType> : GenericReferenceItem
    {
        [Browsable(false)]
        public TSourceType SourceItem
        {
            get { return (TSourceType)SourceItemObject; }
        }
    }

    public class GenericReferenceItem : GenericSlot
    {
        private string _sourceIdentifier;
        [Browsable(false)]
        public override string Label
        {
            get { return SourceItemObject.Name + ": " + base.Label; }
        }

        public override string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(base.Name))
                {
                    return base.Name;
                }
                if (SourceItemObject == null)
                {
                    return "Missing";
                }
                return SourceItemObject.Name;
            }
            set { base.Name = value; }
        }

        public string SourceIdentifier
        {
            get { return _sourceIdentifier; }
            set { _sourceIdentifier = value; }
        }
        [Browsable(false)]
        public virtual IDiagramNodeItem SourceItemObject
        {
            get
            {
                return Node.Project.AllGraphItems.FirstOrDefault(p => p.Identifier == SourceIdentifier) as IDiagramNodeItem;
            }
        }

        public override void Deserialize(JSONClass cls, INodeRepository repository)
        {
            base.Deserialize(cls, repository);
            SourceIdentifier = cls["SourceIdentifier"].Value;
        }

        public override void Serialize(JSONClass cls)
        {
            base.Serialize(cls);
            if (!string.IsNullOrEmpty(SourceIdentifier))
                cls.Add("SourceIdentifier", SourceIdentifier);
        }
    }


    public class TypeReferenceNode : GenericNode, IClassTypeNode, ITypedItem
    {
        private string _fullName;

        [JsonProperty]
        public override string FullName
        {
            get { return _fullName; }
            set { _fullName = value; }
        }

        public string ClassName
        {
            get { return Name; }
            set { Name = value; }
        }
        
        
    }

    public class TypeReferenceNodeViewModel : DiagramNodeViewModel<TypeReferenceNode>
    {
        public TypeReferenceNodeViewModel(TypeReferenceNode graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
        {
        }

        public override bool IsEditable
        {
            get { return false; }
        }

        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();

        }
    }

    public class TypeReferenceNodeDrawer : DiagramNodeDrawer<TypeReferenceNodeViewModel>
    {
        public TypeReferenceNodeDrawer(TypeReferenceNodeViewModel viewModel) : base(viewModel)
        {
        }
    }

    //public class GenericInheritanceReference : GenericNodeChildItem
    //{
    //    public string InputName { get; set; }

    //    public override string Name
    //    {
    //        get { return InputName; }
    //        set { base.Name = value; }
    //    }
    //}
}