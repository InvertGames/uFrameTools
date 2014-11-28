using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using Invert.Core;
using Invert.uFrame.Editor;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IMultiSlot { }
    public class MultiOutputSlot<TFor> : GenericSlot, IMultiSlot
    {
        public IEnumerable<TFor> Items
        {
            get { return Outputs.Select(p => p.Input).OfType<TFor>(); }
        }

    }
    public class SingleOutputSlot<TFor> : GenericSlot
    {
        public TFor Item
        {
            get { return Outputs.Select(p => p.Input).OfType<TFor>().FirstOrDefault(); }
        }

    }
    public class MultiInputSlot<TFor> : GenericSlot, IMultiSlot
    {
        public IEnumerable<TFor> Items
        {
            get { return Inputs.Select(p=>p.Output).OfType<TFor>(); }
        }

    }
    public class SingleInputSlot<TFor> : GenericSlot
    {
        public TFor Item
        {
            get { return Inputs.Select(p => p.Output).OfType<TFor>().FirstOrDefault(); }
        }
    }
    public class InheritanceSlot<TFor> : GenericSlot
    {
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

    public class GenericNode : DiagramNode, IConnectable
    {
        private List<IDiagramNodeItem> _childItems = new List<IDiagramNodeItem>();
        private List<string> _connectedGraphItemIds = new List<string>();

        public virtual bool ValidateInput(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            return a != b && a.GetType() != b.GetType();
        }
        public virtual bool ValidateOutput(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            return a != b && a.GetType() != b.GetType();
        }
        public List<IDiagramNodeItem> ChildItems
        {
            get { return _childItems; }
            set { _childItems = value; }
        }

        public NodeConfigBase Config
        {
            get
            {
                return InvertApplication.Container.Resolve<NodeConfigBase>(this.GetType().Name);
            }
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


        public override IEnumerable<IDiagramNodeItem> DisplayedItems
        {
            get
            {
                return ChildItems;
            }
        }

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
                var propertyName = property.Name;
                if (cls[propertyName] == null) continue;
                var propertyType = property.PropertyType;
                if (typeof (Enum).IsAssignableFrom(property.PropertyType))
                {
                    var value = cls[propertyName].Value;
                    property.SetValue(this,Enum.Parse(propertyType,value),null);
                }else
                if (propertyType == typeof(int))
                {
                    property.SetValue(this, cls[propertyName].AsInt, null);
                }
                else if (propertyType == typeof(string))
                {
                    property.SetValue(this, cls[propertyName].Value, null);
                }
                else if (propertyType == typeof(float))
                {
                    property.SetValue(this, cls[propertyName].AsFloat, null);
                }
                else if (propertyType == typeof(bool))
                {
                    property.SetValue(this, cls[propertyName].AsBool, null);
                }
                else if (propertyType == typeof(double))
                {
                    property.SetValue(this, cls[propertyName].AsDouble, null);
                }
                else if (propertyType == typeof(Vector2))
                {
                    property.SetValue(this, cls[propertyName].AsVector2, null);
                }
                else if (propertyType == typeof(Vector3))
                {
                    property.SetValue(this, cls[propertyName].AsVector3, null);
                }
                else if (propertyType == typeof(Quaternion))
                {
                    property.SetValue(this, cls[propertyName].AsQuaternion, null);
                }
                else if (propertyType == typeof(Color))
                {
                    property.SetValue(this, (Color)cls[propertyName].AsVector4, null);
                }
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

            var inputSlotInfos = GetInputSlotInfos(this.GetType());
            foreach (var item in inputSlotInfos)
            {
                if (item.Key == null) continue;
                var propertyName = item.Key.Name;
                var slot = item.Key.GetValue(this, null) as GenericSlot;
                if (slot == null) continue;
                cls.AddObject(propertyName, slot);
            }
            var outputSlotInfos = GetOutputSlotInfos(this.GetType());
            foreach (var item in outputSlotInfos)
            {
                if (item.Key == null) continue;
                ;
                var propertyName = item.Key.Name;
                var slot = item.Key.GetValue(this, null) as GenericSlot;
                if (slot == null) continue;
                cls.AddObject(propertyName, slot);
            }

            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes(typeof(JsonProperty), true).Length < 1) continue;
                var value = property.GetValue(this, null);
                if (value != null)
                {
                    var propertyName = property.Name;
                    var propertyType = property.PropertyType;
                    if (typeof (Enum).IsAssignableFrom(propertyType))
                    {
                        cls.Add(propertyName, new JSONData(value.ToString()));
                    }else
                    if (propertyType == typeof(int)) 
                    {
                        cls.Add(propertyName, new JSONData((int)value));
                    }
                    else if (propertyType == typeof(string))
                    {
                        cls.Add(propertyName, new JSONData((string)value));
                    }
                    else if (propertyType == typeof(float))
                    {
                        cls.Add(propertyName, new JSONData((float)value));
                    } 
                    else if (propertyType == typeof(bool))
                    {
                        cls.Add(propertyName, new JSONData((bool)value));
                    }
                    else if (propertyType == typeof(double))
                    {
                        cls.Add(propertyName, new JSONData((double)value));
                    }
                    else if (propertyType == typeof(Color))
                    {
                        var vCls = new JSONClass();
                        var color = (Color)value;
                        vCls.AsVector4 = new Vector4(color.r, color.g, color.b, color.a);
                        cls.Add(propertyName, vCls);
                    }
                    else if (propertyType == typeof(Vector2))
                    {
                        var vCls = new JSONClass();
                        vCls.AsVector2 = (Vector2)value;
                        cls.Add(propertyName, vCls);
                    }
                    else if (propertyType == typeof(Vector3))
                    { 
                        var vCls = new JSONClass();
                        vCls.AsVector3 = (Vector3)value;
                        cls.Add(propertyName, vCls);
                    }
                    else if (propertyType == typeof(Quaternion))
                    {
                        var vCls = new JSONClass();
                        vCls.AsQuaternion = (Quaternion)value;
                        cls.Add(propertyName, vCls);
                    }
                    else
                    {
                        throw new Exception(string.Format("{0} property can't be serialized. Override Serialize method to serialize it.", propertyName));
                    }
                }
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
                ChildItems.RemoveAll(p => p.GetType() == section.ChildType && p is GenericReferenceItem && !newItemIds.Contains(((GenericReferenceItem)p).SourceIdentifier));
            }
        }

        internal void AddReferenceItem(GenericReferenceItem[] mirrorItems, IGraphItem item, NodeConfigSectionBase mirrorSection)
        {
            var current = mirrorItems.FirstOrDefault(p => p.SourceIdentifier == item.Identifier);
            if (current != null && !mirrorSection.AllowDuplicates) return;

            var newMirror = Activator.CreateInstance(mirrorSection.ChildType) as GenericReferenceItem;
            newMirror.SourceIdentifier = item.Identifier;
            newMirror.Node = this;
            ChildItems.Add(newMirror);
        }

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

        public virtual IEnumerable<GenericSlot> AllSlots
        {
            get
            {
                foreach (var slot in Config.Inputs)
                {
                    yield return slot.GetDataObject(this) as GenericSlot;
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
        public TSourceType SourceItem
        {
            get { return (TSourceType)SourceItemObject; }
        }
    }

    public class GenericReferenceItem : GenericSlot
    {
        private string _sourceIdentifier;

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
        }

        public string SourceIdentifier
        {
            get { return _sourceIdentifier; }
            set { _sourceIdentifier = value; }
        }

        public IDiagramNodeItem SourceItemObject
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