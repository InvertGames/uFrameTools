using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Invert.Data;
using Invert.Json;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IMultiSlot { }
    public class MultiOutputSlot<TFor> : GenericSlot, IMultiSlot
    {
        public override bool AllowMultipleInputs
        {
            get { return false; }
        }

        public override bool AllowMultipleOutputs
        {
            get { return true; }
        }
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
        public override bool AllowMultipleInputs
        {
            get { return false; }
        }

        public override bool AllowMultipleOutputs
        {
            get { return false; }
        }
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
        public override bool AllowMultipleInputs
        {
            get { return true; }
        }

        public override bool AllowMultipleOutputs
        {
            get { return false; }
        }

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
        public override bool AllowMultipleInputs
        {
            get { return false; }
        }

        public override bool AllowMultipleOutputs
        {
            get { return false; }
        }
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
    //public class InheritanceSlot<TFor> : GenericSlot
    //{
    //    [Browsable(false)]
    //    public TFor Item
    //    {
    //        get { return Inputs.Select(p => p.Output).OfType<TFor>().FirstOrDefault(); }
    //    }

    //    public override bool Validate(IDiagramNodeItem a, IDiagramNodeItem b)
    //    {
           
    //        var result = a is TFor && b is BaseClassReference && b.Node != a.Node && b.Node.GetType() == a.GetType();
    //        return result;
    //    }
    //}
    public class ReferenceSection<TReference> : GenericReferenceItem<TReference> where TReference : class
    {
        
    }

    public class GenericSlot : GenericNodeChildItem
    {
        
        public virtual bool Validate(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            
           return a != b;
        }
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
        TypeSelection,
        GraphItems
    }
    [Browsable(false)]
    public partial class GenericNode : DiagramNode, IConnectable
    {
        public override void MoveItemDown(IDiagramNodeItem nodeItem)
        {
            base.MoveItemDown(nodeItem);
            // TODO 2.0 ORDERING
            //var items = ChildItems.Where(p => p.GetType() == nodeItem.GetType()).ToList();
            //ChildItems.RemoveAll(p => items.Contains(p));

            //items.Move(items.IndexOf(nodeItem),false);
            //ChildItems.AddRange(items);

        }

        public override void MoveItemUp(IDiagramNodeItem nodeItem)
        {
            base.MoveItemUp(nodeItem);
            // TODO 2.0 Children re-ordering
            //var items = ChildItems.Where(p => p.GetType() == nodeItem.GetType()).ToList();
            //ChildItems.RemoveAll(p => items.Contains(p));

            //items.Move(items.IndexOf(nodeItem), true);
            //ChildItems.AddRange(items);

        }


        private List<string> _connectedGraphItemIds = new List<string>();

        [Browsable(false)]
        public NodeConfigBase Config
        {
            get
            {
                var config = InvertApplication.Container.Resolve<NodeConfigBase>(this.GetType().Name);
                if (config == null)
                {
                    throw new Exception("Config for type " + this.GetType().Name + " couldn't be found.");
                }
                return config;
            }
        }

        public override string SubTitle
        {
            get { return Config.Name; }
        }

        [GeneratorProperty, JsonProperty]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value;
                Changed = true;
            }
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
        public override string Label
        {
            get { return Name; }
        }



        public void AddReferenceItem(IGraphItem item, NodeConfigSectionBase mirrorSection)
        {
            AddReferenceItem(PersistedItems.Where(p => p.GetType() == mirrorSection.ReferenceType).Cast<GenericReferenceItem>().ToArray(), item, mirrorSection);
        }

        public override void Deserialize(JSONClass cls)
        {
            base.Deserialize(cls);
            var inputSlotInfos = GetInputSlotInfos(this.GetType());
            foreach (var item in inputSlotInfos)
            {
                var propertyName = item.Key.Name;
                if (cls[propertyName] == null) continue;
                var slotObject = cls[propertyName].DeserializeObject( item.Key.PropertyType.GetGenericArguments().FirstOrDefault()) as GenericSlot;
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
                var slotObject = cls[propertyName].DeserializeObject( item.Key.PropertyType.GetGenericArguments().FirstOrDefault()) as GenericSlot;
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

        public TType GetConnectionReference<TType>()
            where TType : GenericSlot, new()
        {
            return (TType)GetConnectionReference(typeof(TType));
        }

        public GenericSlot GetConnectionReference(Type inputType)
        {
            var item = PersistedItems.FirstOrDefault(p => inputType.IsAssignableFrom(p.GetType()));
            if (item == null)
            {
                var input = Activator.CreateInstance(inputType) as GenericSlot;
                Repository.Add(input);
                input.Node = this;
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
            // TODO 2.0
            //UpdateReferences();
            //Repository.RemoveAll<GenericReferenceItem>(
            //    p =>
            //        p.Identifier == diagramNodeItem.Identifier || p.SourceIdentifier == diagramNodeItem.Identifier));
        }
        [Browsable(false)]
        public override bool IsValid
        {
            get { return Config.IsValid(this); }
        }

        public override void Validate(List<ErrorInfo> errors)
        {
            base.Validate(errors);
            foreach (var slot in AllInputSlots)
            {
                slot.Validate(errors);
            }
            
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
            // TODO 2.0: Reference Handling
            //foreach (var mirrorSection in Config.Sections.Where(p => p.ReferenceType != null && !p.AllowAdding))
            //{
            //    NodeConfigSectionBase section = mirrorSection;
            //    var mirrorItems = ChildItems.Where(p => p.GetType() == section.ReferenceType).Cast<GenericReferenceItem>().ToArray();
            //    var newItems = mirrorSection.GenericSelector(this).ToArray();
            //    var newItemIds = newItems.Select(p => p.Identifier);

            //    foreach (var item in newItems)
            //    {
            //        if (PersistedItems.OfType<GenericReferenceItem>().Any(p => p.SourceIdentifier == item.Identifier)) continue;
            //        AddReferenceItem(mirrorItems, item, mirrorSection);
            //    }
            //    var items = ChildItems.Where(p => p.GetType() == section.SourceType && p is GenericReferenceItem && !newItemIds.Contains(((GenericReferenceItem)p).SourceIdentifier)).ToArray();
            //    foreach (var item in items)
            //    {
            //        Node.Project.RemoveItem(item);
            //    }
            //}
        }

        internal void AddReferenceItem(GenericReferenceItem[] mirrorItems, IGraphItem item, NodeConfigSectionBase mirrorSection)
        {
            
            var current = mirrorItems.FirstOrDefault(p => p.SourceIdentifier == item.Identifier);
            if (current != null && !mirrorSection.AllowDuplicates) return;

            var newMirror = Activator.CreateInstance(mirrorSection.SourceType) as GenericReferenceItem;
            Node.Repository.Add(newMirror);
            newMirror.SourceIdentifier = item.Identifier;
            newMirror.Node = this;
     
            //Node.Project.AddItem(newMirror);
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

    public class GenericReferenceItem<TSourceType> : GenericReferenceItem where TSourceType : class 
    {
        
        [Browsable(false)]
        public TSourceType SourceItem
        {
            get
            {
                var sourceItem = SourceItemObject;
                if (sourceItem is TSourceType)
                {
                    return (TSourceType)sourceItem;
                }
                return null;
            }
        }
    }

    public class GenericReferenceItem : GenericSlot, ITypedItem, IDataRecordRemoved
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
            set { base.Name = value;
                Changed = true;
            }
        }
        [JsonProperty]
        public string SourceIdentifier
        {
            get { return _sourceIdentifier; }
            set { _sourceIdentifier = value;
                Changed = true;
            }
        }
        [Browsable(false)]
        public virtual IDiagramNodeItem SourceItemObject
        {
            get
            {
                return Repository.GetById<IDiagramNodeItem>(SourceIdentifier);
            }
        }

        public override void Deserialize(JSONClass cls)
        {
            base.Deserialize(cls);
            SourceIdentifier = cls["SourceIdentifier"].Value;
        }

        public override void Serialize(JSONClass cls)
        {
            base.Serialize(cls);
            if (!string.IsNullOrEmpty(SourceIdentifier))
                cls.Add("SourceIdentifier", SourceIdentifier);
        }

        public string RelatedType
        {
            get
            {
                var source = SourceItemObject;
                if (source == null)
                {
                    return "Missing";
                }
                var classItem = source as IClassTypeNode;
                if (classItem != null)
                {
                    return classItem.ClassName;
                }
                return source.Name;
            }
            set
            {
                
            }
        }

        public string RelatedTypeName
        {
            get
            {
                return RelatedType;
            }
        }

        public void RemoveType()
        {
            Repository.Remove(this);
        }

        public void RecordRemoved(IDataRecord record)
        {
            if (record.Identifier == this.SourceIdentifier)
            {
                Repository.Remove(this);
            }
        }
    }


    public class TypeReferenceNode : GenericNode, IClassTypeNode, ITypedItem
    {
        
        private string _name1;

        public override string FullName
        {
            get { return Name; }
            set { Name = value; }
        }

        [JsonProperty]
        public override string Name
        {
            get { return _name1 ; }
            set
            {
                _name1 = value;
            }
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

        public override IEnumerable<string> Tags
        {
            get { yield return "Type Reference"; }
        }

        //public override bool IsEditable
        //{
        //    get { return false; }
        //}

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

    public class ScreenshotNode : GenericNode
    {
        private int _width = 100;
        private int _height = 100;
        [InspectorProperty]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        [JsonProperty, InspectorProperty]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        [JsonProperty, InspectorProperty]
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
    }

    public class ScreenshotNodeViewModel : DiagramNodeViewModel<ScreenshotNode>
    {
        public ScreenshotNodeViewModel(ScreenshotNode graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
        {
        }

        public float Width
        {
            get { return GraphItem.Width; }
            set { GraphItem.Width = Mathf.RoundToInt(value); }
        }
        public float Height
        {
            get { return GraphItem.Height; }
            set { GraphItem.Height = Mathf.RoundToInt(value); }
        }
    }

    public class ScreenshotNodeDrawer : DiagramNodeDrawer<ScreenshotNodeViewModel>
    {
        public ScreenshotNodeDrawer(ScreenshotNodeViewModel viewModel) : base(viewModel)
        {
        }

        public override void Refresh(IPlatformDrawer platform)
        {
            //base.Refresh(platform);
            this.Bounds = new Rect(ViewModel.Position.x, ViewModel.Position.y, NodeViewModel.Width, NodeViewModel.Height);
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            //base.Refresh(platform, position, hardRefresh);
            
        }

        public override void RefreshContent()
        {
            //base.RefreshContent();
        }

        public override void OnMouseDown(MouseEvent mouseEvent)
        {
            base.OnMouseDown(mouseEvent);
            ViewModel.SaveImage = true;
        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            //base.Draw(platform, scale);
            platform.DrawStretchBox(this.Bounds,CachedStyles.BoxHighlighter1,50f);
            
            if (ViewModel.SaveImage)
            {
                
               // Texture2D texture2D = new Texture2D(NodeViewModel.GraphItem.Width,NodeViewModel.GraphItem.Height, (TextureFormat)3, false);
               // var bounds = new Rect(this.Bounds);
               // bounds.y += editorWindow.position.height;
               //// bounds.y = lastRect.height - bounds.y - bounds.height;
               // texture2D.ReadPixels(bounds, 0, 0);
               // texture2D.Apply();
               // //string fullPath = Path.GetFullPath(Application.get_dataPath() + "/../" + Path.Combine(this.screenshotsSavePath, actionName) + ".png");
               // //if (!FsmEditorUtility.CreateFilePath(fullPath))
               // //    return;
               // byte[] bytes = texture2D.EncodeToPNG();
               // Object.DestroyImmediate((Object)texture2D, true);
               // File.WriteAllBytes("image.png", bytes);
               // ViewModel.SaveImage = false;
               // Debug.Log(this.Bounds.x.ToString() + " : " + this.Bounds.y);
            }

        }
    }



}