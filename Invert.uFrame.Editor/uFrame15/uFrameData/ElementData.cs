using System.CodeDom;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ElementData : ElementDataBase, IDesignerType
{
    [SerializeField]
    private List<ViewModelCollectionData> _collections = new List<ViewModelCollectionData>();

    [SerializeField]
    private List<ViewModelCommandData> _commands = new List<ViewModelCommandData>();

    [SerializeField]
    private bool _isTemplate;

    [SerializeField]
    private List<ViewModelPropertyData> _properties = new List<ViewModelPropertyData>();

    public IEnumerable<RegisteredInstanceData> RegisteredInstances
    {
        get
        {
            return
                Graph.GetSubSystems().SelectMany(p => p.Instances).Where(p => p.RelatedType == this.Identifier);
        }
    }

    public IEnumerable<ViewModelPropertyData> AllProperties
    {
        get
        {
            var element = this;
            while (element != null)
            {
                foreach (var property in element.Properties)
                {
                    yield return property;
                }
                element = element.BaseElement;
            }
        }
    }

    public IEnumerable<IBindableTypedItem> SubscribableProperties
    {
        get
        {
            foreach (var item in ComputedProperties)
            {
                yield return item;
            }
            foreach (var item in Properties)
            {
                yield return item;
            }
            //foreach (var item in Collections)
            //{
            //    yield return item;
            //}
        }
    }

    public override string BaseTypeName
    {
        get
        {
            var baseElement = BaseElement;
            if (baseElement != null)
            {
                return baseElement.Name;
            }
            return "ViewModel";
        }
    }

    //public IEnumerable<string> BindingMethodNames
    //{
    //    get
    //    {
    //        return ViewModelItems.SelectMany(p => p.BindingMethodNames);
    //    }
    //}

    public override ICollection<ViewModelCollectionData> Collections
    {
        get { return _collections; }
        set { _collections = value.ToList(); }
    }

    public override ICollection<ViewModelCommandData> Commands
    {
        get { return _commands; }
        set { _commands = value.ToList(); }
    }

    public override IEnumerable<IDiagramNodeItem> DisplayedItems
    {
        get
        {

           


            foreach (var item in Properties)
                yield return item;
            foreach (var item in Collections)
                yield return item;
            foreach (var item in Commands)
                yield return item;
        }
    }
    public  IEnumerable<IDiagramNodeItem> AllItems
    {
        get
        {

            var baseElement = BaseElement;
            if (baseElement != null)
                foreach (var baseItem in baseElement.DisplayedItems)
                {
                    yield return baseItem;
                }


            foreach (var item in Properties)
                yield return item;
            foreach (var item in Collections)
                yield return item;
            foreach (var item in Commands)
                yield return item;
        }
    }
    public override IEnumerable<IDiagramNodeItem> PersistedItems
    {
        get
        {
            foreach (var property in Properties)
            {
                yield return property;
            }
            foreach (var command in Commands)
            {
                yield return command;
            }
            foreach (var collection in Collections)
            {
                yield return collection;
            }
        }
        set
        {
            var stuff = value.ToArray();
            Properties = stuff.OfType<ViewModelPropertyData>().ToList();
            Commands = stuff.OfType<ViewModelCommandData>().ToList();
            Collections = stuff.OfType<ViewModelCollectionData>().ToList();
        }
    }

    public override string FullName
    {
        get
        {
            if (string.IsNullOrEmpty(Namespace)) return NameAsViewModel;

            return string.Format("{0}.{1}", Namespace, NameAsViewModel);
        }
    }

    public override string InfoLabel
    {
        get { return string.Format("Items: [{0}] {1}", Locations.Keys.Count - 1, base.InfoLabel ?? string.Empty); }
    }

    public bool IsTemplate
    {
        get { return _isTemplate; }
        set { _isTemplate = value; }
    }

    public string NameAsAssetClass
    {
        get { return string.Format("{0}Asset", Name); }
    }

    public string NameAsModelInterfaceClass
    {
        get { return string.Format("I{0}Model", Name); }
    }

    public override void NodeAddedInFilter(IDiagramNode newNodeData)
    {
        base.NodeAddedInFilter(newNodeData);
        var view = newNodeData as ViewData;
        if (view != null)
        {
            view.ForElementIdentifier = this.Identifier;
        }
    }

    public IEnumerable<ElementData> ParentElements
    {
        get
        {
            HashSet<ElementData> set = new HashSet<ElementData>();
            foreach (ElementData element in Graph.GetElements())
            {
                foreach (IBindableTypedItem item in element.ViewModelItems)
                {
                    if (item.RelatedType == this.Identifier)
                    {
                        if (set.Add(element)) yield return element;
                        break;
                    }
                }
            }
        }
    }

    public override ICollection<ViewModelPropertyData> Properties
    {
        get { return _properties; }
        set { _properties = value.ToList(); }
    }

    public IEnumerable<ISerializeablePropertyData> SerializedProperties
    {
        get
        {
            foreach (var property in Properties)
            {
                yield return property;
            }
        }
    }

    public override string SubTitle
    {
        get
        {
            return string.Empty;
        }
    }

    public override void NodeRemoved(IDiagramNode nodeData)
    {
        base.NodeRemoved(nodeData);
        if (BaseIdentifier == nodeData.Identifier)
        {
            BaseIdentifier = null;
        }
    }

    public override void NodeItemRemoved(IDiagramNodeItem item)
    {
        Properties.Remove(item as ViewModelPropertyData);
        Collections.Remove(item as ViewModelCollectionData);
        Commands.Remove(item as ViewModelCommandData);

    }

    public override CodeTypeReference GetPropertyType(ITypedItem itemData)
    {
        return new CodeTypeReference(this.NameAsViewModel);
    }

    public override CodeTypeReference GetFieldType(ITypedItem itemData)
    {
        var tRef = new CodeTypeReference(uFrameEditor.UFrameTypes.P);
        tRef.TypeArguments.Add(this.NameAsViewModel);
        return tRef;
    }

    public IEnumerable<ViewPropertyData> ViewProperties
    {
        get
        {
            return Views.SelectMany(p => p.Properties);
        }
    }

    public IEnumerable<ViewData> Views
    {
        get
        {
            foreach (var v in Graph.GetViews())
            {
                if (v.ForElementIdentifier == this.Identifier)
                {
                    yield return v;
                }
            }
        }
    }

    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameElementRefactorer(this);
    }

    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);
        if (cls["BaseType"] != null)
        {
            var assemblyQualifiedName = cls["BaseType"].Value;
            BaseType = assemblyQualifiedName;
        }
        else
        {
            _baseIdentifier = cls["BaseIdentifier"].Value;
        }

        IsTemplate = cls["IsTemplate"].AsBool;
        _isMultiInstance = cls["IsMultiInstance"].AsBool;
    }

    [Obsolete("For Upgrading Old Projects Only")]
    public string BaseType { get; set; }

    public bool IsRegistered
    {
        get
        {
            return
                Graph.NodeItems.OfType<SubSystemData>()
                    .SelectMany(p => p.Instances).Any(p => p.RelatedType == this.Identifier);

        }
    }

    public bool IsAllowed(object item, Type t)
    {
        if (item == this) return true;
        if (t == typeof(EnumData)) return true;
        if (t == typeof(SubSystemData)) return false;
        //if (t == typeof(ExternalSubsystem)) return false;
        if (t == typeof(SceneManagerData)) return false;
        if (t == typeof(ViewComponentData)) return true;
        if (t == typeof(ViewData)) return true;
        if (t == typeof(ElementData)) return false;

        if (t == typeof(IDiagramNodeItem)) return false;
        return true;
    }

    public bool IsItemAllowed(object item, Type t)
    {
        //if (typeof(IViewModelItem).IsAssignableFrom(t)) return true;

        return true;
    }

    public void MoveItem(IDiagramNodeItem nodeItem, bool up)
    {
        var commandNode = nodeItem as ViewModelCommandData;
        if (commandNode != null)
            _commands.Move(_commands.IndexOf(commandNode), up);

        var collectionNode = nodeItem as ViewModelCollectionData;
        if (collectionNode != null)
            _collections.Move(_collections.IndexOf(collectionNode), up);

        var propertyNode = nodeItem as ViewModelPropertyData;
        if (propertyNode != null)
            _properties.Move(_properties.IndexOf(propertyNode), up);
    }

    public override void MoveItemDown(IDiagramNodeItem nodeItem)
    {
        base.MoveItemDown(nodeItem);
        MoveItem(nodeItem, false);
    }

    public override void MoveItemUp(IDiagramNodeItem nodeItem)
    {
        base.MoveItemUp(nodeItem);
        MoveItem(nodeItem, true);
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Project.RemoveNode(this);

        foreach (var elementData in Graph.GetElements())
        {
            foreach (var diagramSubItem in elementData.ViewModelItems)
            {
                if (diagramSubItem.RelatedTypeName == this.Name)
                {
                    diagramSubItem.RemoveType();
                }
            }
        }
        //foreach (var viewData in Data.GetViews())
        //{
        //    if (viewData.ForElementIdentifier == this.Identifier)
        //    {
        //        viewData.ForElementIdentifier = null;
        //    }
        //}
        //foreach (var viewComponentData in Data.GetViewComponents())
        //{
        //    if (viewComponentData.ElementIdentifier == this.Identifier)
        //    {
        //        viewComponentData.ElementIdentifier = null;
        //    }
        //}
    }


    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("IsTemplate", new JSONData(IsTemplate));
        cls.Add("BaseIdentifier", new JSONData(_baseIdentifier));
        cls.Add("IsMultiInstance", new JSONData(_isMultiInstance));
    }

    public void SetBaseElement(ElementData output)
    {
        BaseIdentifier = output.Identifier;
    }

    public void RemoveBaseElement()
    {
        BaseIdentifier = null;
    }
}