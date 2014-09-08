using Invert.MVVM;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UFListExtensions
{
    public static void Move<T>(this IList<T> list, int iIndexToMove, bool up = true)
    {
        if (up)
        {
            var move = iIndexToMove - 1;
            if (move < 0) return;
            var old = list[move];
            list[move] = list[iIndexToMove];
            list[iIndexToMove] = old;
        }
        else
        {
            var move = iIndexToMove + 1;
            if (move >= list.Count) return;
            var old = list[move];
            list[move] = list[iIndexToMove];
            list[iIndexToMove] = old;
        }
    }
}

public interface IDesignerType
{
    string Identifier { get; }
}
[Serializable]
public class ElementData : ElementDataBase, IDiagramFilter, IDesignerType
{
    [SerializeField]
    private string _baseIdentifier;

    [SerializeField]
    private FilterCollapsedDictionary _collapsedValues = new FilterCollapsedDictionary();

    [SerializeField]
    private List<ViewModelCollectionData> _collections = new List<ViewModelCollectionData>();

    [SerializeField]
    private List<ViewModelCommandData> _commands = new List<ViewModelCommandData>();

    [SerializeField]
    private bool _isTemplate;

    [SerializeField]
    private FilterLocations _locations = new FilterLocations();

    [SerializeField]
    private List<ViewModelPropertyData> _properties = new List<ViewModelPropertyData>();




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

    public IEnumerable<string> BindingMethodNames
    {
        get
        {
            return ViewModelItems.SelectMany(p => p.BindingMethodNames);
        }
    }

    public FilterCollapsedDictionary CollapsedValues
    {
        get { return _collapsedValues; }
        set { _collapsedValues = value; }
    }

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

    public override IEnumerable<IDiagramNodeItem> ContainedItems
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

    public Type CurrentType
    {
        get
        {
            return Type.GetType(AssemblyQualifiedName);
        }
    }

    public bool ImportedOnly
    {
        get { return true; }
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

    public FilterLocations Locations
    {
        get { return _locations; }
        set { _locations = value; }
    }

    public string NameAsAssetClass
    {
        get { return string.Format("{0}Asset", Name); }
    }

    public string NameAsModelInterfaceClass
    {
        get { return string.Format("I{0}Model", Name); }
    }

    public IEnumerable<ElementData> ParentElements
    {
        get
        {
            foreach (var element in Data.GetElements())
            {
                foreach (var item in element.ViewModelItems)
                {
                    if (item.RelatedType == this.Identifier)
                    {
                        yield return element;
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
            return
                Properties.Cast<ISerializeablePropertyData>().Concat(ViewProperties.Cast<ISerializeablePropertyData>());
        }
    }


    public override string SubTitle
    {
        get
        {
            return string.Empty;
        }
    }

    public IEnumerable<ViewComponentData> ViewComponents
    {
        get
        {
            foreach (var viewComponentData in Data.GetViewComponents())
            {
                if (viewComponentData.ElementIdentifier == this.Identifier ||
                    AllBaseTypes.Any(p => p.Identifier == viewComponentData.ElementIdentifier))
                {
                    yield return viewComponentData;
                }
            }
        }
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
            foreach (var v in Data.GetViews())
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

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["BaseType"] != null)
        {
            var assemblyQualifiedName = cls["BaseType"].Value;
            var foundElement =
                repository.GetElements().FirstOrDefault(p => p.AssemblyQualifiedName == assemblyQualifiedName);
            if (foundElement != null)
            {
                Debug.Log("Found Element base identifier");
                _baseIdentifier = foundElement.Identifier;
            }
            else
            {
                Debug.Log("Couldn't find base identifier");
            }
        }
        else
        {
            _baseIdentifier = cls["BaseIdentifier"];
        }
        
        IsTemplate = cls["IsTemplate"].AsBool;
        _isMultiInstance = cls["IsMultiInstance"].AsBool;
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
        Data.RemoveNode(this);

        foreach (var elementData in Data.GetElements())
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

    public string BaseIdentifier
    {
        get { return _baseIdentifier; }
        set { _baseIdentifier = value; }
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