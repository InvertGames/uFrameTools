using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// The base data class for all diagram nodes.
/// </summary>
public abstract class DiagramNode : IDiagramNode, IRefactorable, IDiagramFilter
{
    [SerializeField]
    private FilterCollapsedDictionary _collapsedValues = new FilterCollapsedDictionary();

    [NonSerialized]
    private IGraphData _data;

    private DataBag _dataBag = new DataBag();

    private FlagsDictionary _flags = new FlagsDictionary();

    [SerializeField]
    private string _identifier;

    [SerializeField]
    private bool _isCollapsed;

    private Vector2 _location;

    [SerializeField]
    private FilterLocations _locations = new FilterLocations();

    [SerializeField]
    private string _name;

    private Rect _position;

    private List<Refactorer> _refactorings;

    public bool this[string flag]
    {
        get
        {
            if (Flags.ContainsKey(flag))
            {
                return Flags[flag];
            }
            else
            {
                return false;
            }
        }
        set
        {
            if (Flags.ContainsKey(flag))
            {
                if (value == false)
                {
                    Flags.Remove(flag);
                    return;
                }
                Flags[flag] = true;
            }
            else
            {
                Flags.Add(flag, value);
            }
        }
    }

    public IEnumerable<Refactorer> AllRefactorers
    {
        get { return Refactorings.Concat(Items.OfType<IRefactorable>().SelectMany(p => p.Refactorings)); }
    }

    [Obsolete]
    public virtual string AssemblyQualifiedName
    {
        get
        {
            return uFrameEditor.UFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", Name);
        }
    }

    public FilterCollapsedDictionary CollapsedValues
    {
        get { return _collapsedValues; }
        set { _collapsedValues = value; }
    }

    /// <summary>
    /// The items that should be persisted with this diagram node.
    /// </summary>
    public abstract IEnumerable<IDiagramNodeItem> ContainedItems { get; set; }

    public Type CurrentType
    {
        get
        {
            return uFrameEditor.FindType(FullName);
        }
    }

    public DataBag DataBag
    {
        get { return _dataBag ?? (_dataBag = new DataBag()); }
        set { _dataBag = value; }
    }

    public Vector2 DefaultLocation
    {
        get { return _location; }
    }

    /// <summary>
    /// Gets the diagram file that this node belongs to
    /// </summary>
    public virtual IGraphData Diagram
    {
        get { return Project.Diagrams.FirstOrDefault(p => p.NodeItems.Contains(this)); }
    }

    public bool Dirty { get; set; }

    public IDiagramFilter Filter
    {
        get
        {
            if (Project.CurrentFilter == this)
                return this;
            return Project.CurrentFilter;
        }
    }

    public FlagsDictionary Flags
    {
        get { return _flags ?? (_flags = new FlagsDictionary()); }
        set { _flags = value; }
    }

    public string FullLabel { get { return Name; } }

    public virtual string FullName
    {
        get
        {
            if (!string.IsNullOrEmpty(Namespace))
                return string.Format("{0}.{1}", Namespace, Name);

            return Name;
        }
    }

    public Rect HeaderPosition { get; set; }

    public string Highlighter { get { return null; } }

    public virtual string Identifier { get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier; } }

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

    public virtual bool IsCollapsed
    {
        get { return _isCollapsed; }
        set
        {
            _isCollapsed = value;
            Filter.CollapsedValues[this] = value;
            Dirty = true;
        }
    }

    public bool IsEditing { get; set; }

    public bool IsExternal
    {
        get
        {
            return Project.NodeItems.All(p => p.Identifier != Identifier);
        }
    }

    public bool IsNewNode { get; set; }

    public bool IsSelectable { get { return true; } }

    public bool IsSelected { get; set; }

    public virtual IEnumerable<IDiagramNodeItem> Items
    {
        get { return ContainedItems; }
    }

    public abstract string Label { get; }

    public Vector2 Location
    {
        get
        {
            return Filter.Locations[this];
        }
        set
        {
            _location = value;

            if (_location.x < 0)
                _location.x = 0;
            if (_location.y < 0)
                _location.y = 0;
            Filter.Locations[this] = _location;
            Dirty = true;
        }
    }

    public FilterLocations Locations
    {
        get { return _locations; }
        set { _locations = value; }
    }

    public virtual string Name
    {
        get { return _name; }
        set
        {
            if (value == null) return;
            _name = Regex.Replace(value, "[^a-zA-Z0-9_.]+", "");
            Dirty = true;
        }
    }

    public virtual string Namespace
    {
        get { return uFrameEditor.CurrentProject.GeneratorSettings.NamespaceProvider.RootNamespace; }
    }

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

    /// <summary>
    /// The project this node belongs to
    /// </summary>
    public virtual IProjectRepository Project
    {
        get
        {
            return uFrameEditor.CurrentProject;
        }
    }

    public virtual IEnumerable<Refactorer> Refactorings
    {
        get
        {
            if (RenameRefactorer != null)
                yield return RenameRefactorer;
        }
    }

    public RenameRefactorer RenameRefactorer { get; set; }

    public string SearchTag { get { return Name; } }

    public virtual bool ShouldRenameRefactor { get { return true; } }

    public virtual string SubTitle { get { return string.Empty; } }

    public string Title { get { return Name; } }

    protected DiagramNode()
    {
        IsNewNode = true;
    }

    public virtual void BeginEditing()
    {
        if (!IsNewNode)
        {
            if (RenameRefactorer == null)
            {
                RenameRefactorer = CreateRenameRefactorer();
            }
        }

        OldName = Name;
        IsEditing = true;
    }

    public void BeginRename()
    {
        BeginEditing();
    }

    public virtual RenameRefactorer CreateRenameRefactorer()
    {
        return null;
    }

    public virtual void Deserialize(JSONClass cls, INodeRepository repository)
    {
        _name = cls["Name"].Value;
        _isCollapsed = cls["IsCollapsed"].AsBool;
        _identifier = cls["Identifier"].Value;
        IsNewNode = false;
        ContainedItems = cls["Items"].AsArray.DeserializeObjectArray<IDiagramNodeItem>(repository);

        if (cls["Locations"] != null)
        {
            Locations.Deserialize(cls["Locations"].AsObject);
            CollapsedValues.Deserialize(cls["CollapsedValues"].AsObject, repository);
        }
        if (cls["Flags"] is JSONClass)
        {
            var flags = cls["Flags"].AsObject;
            Flags = new FlagsDictionary();
            Flags.Deserialize(flags, repository);
        }
        if (cls["DataBag"] is JSONClass)
        {
            var flags = cls["DataBag"].AsObject;
            DataBag = new DataBag();
            DataBag.Deserialize(flags, repository);
        }
        if (ContainedItems != null)
        {
            foreach (var item in ContainedItems)
            {
                item.Node = this;
            }
        }
    }

    public virtual bool EndEditing()
    {
        IsEditing = false;
        if (Project != null)
        {
            if (Project.NodeItems.Count(p => p.Name == Name) > 1)
            {
                Name = OldName;
                return false;
            }
        }
        if (OldName != Name)
        {
            if (RenameRefactorer == null)
            {
                return true;
            }

            RenameRefactorer.Set(this);
            //Data.RefactorCount++;
        }
        return true;
    }

    public virtual CodeTypeReference GetFieldType(ITypeDiagramItem itemData)
    {
        var tRef = new CodeTypeReference(uFrameEditor.UFrameTypes.P);
        tRef.TypeArguments.Add(this.Name);
        return tRef;
    }

    public virtual void NodeAddedInFilter(IDiagramNode newNodeData)
    {
        
    }


    public virtual void NodeItemRemoved(IDiagramNodeItem diagramNodeItem)
    {
        DataBag[diagramNodeItem.Identifier] = null;
        Flags[diagramNodeItem.Identifier] = false;
    }


    public virtual CodeTypeReference GetPropertyType(ITypeDiagramItem itemData)
    {
        return new CodeTypeReference(this.Name);
    }

    public virtual void MoveItemDown(IDiagramNodeItem nodeItem)
    {
    }

    public virtual void MoveItemUp(IDiagramNodeItem nodeItem)
    {
    }

    public virtual void NodeRemoved(IDiagramNode enumData)
    {
        foreach (var item in ContainedItems)
        {
            if (item != this)
                item.NodeRemoved(enumData);
        }

        DataBag[enumData.Identifier] = null;
        this[enumData.Identifier] = false;
    }

    void IDiagramNodeItem.NodeItemRemoved(IDiagramNodeItem nodeItem)
    {
        NodeItemRemoved(nodeItem);
    }

    public virtual void RefactorApplied()
    {
        RenameRefactorer = null;
    }

    public void Remove(IDiagramNode diagramNode)
    {
        Project.PositionData.Remove(Project.CurrentFilter, diagramNode.Identifier);
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
        cls.Add("IsCollapsed", new JSONData(_isCollapsed));
        cls.Add("Identifier", new JSONData(_identifier));

        cls.AddObjectArray("Items", ContainedItems);

        //cls.Add("Locations", Locations.Serialize());
        cls.Add("CollapsedValues", CollapsedValues.Serialize());

        cls.AddObject("Flags", Flags);
        cls.AddObject("DataBag", DataBag);
    }

    public void RemoveItem(IDiagramNodeItem item)
    {

    }
}