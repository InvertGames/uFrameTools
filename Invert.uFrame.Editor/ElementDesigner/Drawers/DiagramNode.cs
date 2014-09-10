using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
/// <summary>
/// The base data class for all diagram nodes.
/// </summary>
public abstract class DiagramNode : IDiagramNode, IRefactorable, IDiagramFilter
{
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
                Flags[flag] = value;
            }
            else
            {
                Flags.Add(flag, value);
            }
        }
    }

    public bool IsNewNode { get; set; }

    public bool IsExternal
    {
        get
        {
            return Data.NodeItems.All(p => p.Identifier != Identifier);
        }
    }

    public Vector2 DefaultLocation
    {
        get { return _location; }
    }

    public virtual void Serialize(JSONClass cls)
    {
        cls.Add("Name", new JSONData(_name));
        cls.Add("IsCollapsed", new JSONData(_isCollapsed));
        cls.Add("Identifier", new JSONData(_identifier));

        cls.AddObjectArray("Items", ContainedItems);
        
        cls.Add("Locations", Locations.Serialize());
        cls.Add("CollapsedValues", CollapsedValues.Serialize());

        cls.AddObject("Flags", Flags);
        cls.AddObject("DataBag", DataBag);
    }

    public virtual void MoveItemDown(IDiagramNodeItem nodeItem)
    {
    }

    public virtual void MoveItemUp(IDiagramNodeItem nodeItem)
    {
    }

    public DataBag DataBag
    {
        get { return _dataBag ?? (_dataBag = new DataBag()); }
        set { _dataBag = value; }
    }

    private DataBag _dataBag = new DataBag();

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

    public FlagsDictionary Flags
    {
        get { return _flags ?? (_flags = new FlagsDictionary()); }
        set { _flags = value; }
    }

    /// <summary>
    /// The items that should be persisted with this diagram node.
    /// </summary>
    public abstract IEnumerable<IDiagramNodeItem> ContainedItems { get; set; }

    [NonSerialized]
    private IElementDesignerData _data;

    [SerializeField]
    private string _identifier;

    [SerializeField]
    private bool _isCollapsed;

    private Vector2 _location;

    [SerializeField]
    private string _name;

    private Rect _position;

    private List<Refactorer> _refactorings;
    private FlagsDictionary _flags = new FlagsDictionary();

    [SerializeField]
    private FilterCollapsedDictionary _collapsedValues = new FilterCollapsedDictionary();

    [SerializeField]
    private FilterLocations _locations = new FilterLocations();

    public IEnumerable<Refactorer> AllRefactorers
    {
        get { return Refactorings.Concat(Items.OfType<IRefactorable>().SelectMany(p => p.Refactorings)); }
    }

    public virtual string AssemblyQualifiedName
    {
        get
        {
            return uFrameEditor.UFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", Name);
        }
    }

    public Vector2[] ConnectionPoints { get; set; }


    public virtual INodeRepository Data
    {
        get
        {
            return uFrameEditor.CurrentProject;
        }
    }

    public bool Dirty { get; set; }

    public IDiagramFilter Filter
    {
        get { return Data.CurrentFilter; }
    }

    public string FullLabel { get { return Name; } }

    public Rect HeaderPosition { get; set; }

    public string Highlighter { get { return null; } }

    public virtual string Identifier { get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier; } }

    public virtual string SubTitle { get { return string.Empty; } }

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

    public bool IsSelectable { get { return true; } }

    public DiagramNode Node { get; set; }

    public bool IsSelected { get; set; }

    public abstract IEnumerable<IDiagramNodeItem> Items { get; }

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

    public virtual IEnumerable<Refactorer> Refactorings
    {
        get
        {
            if (RenameRefactorer != null)
                yield return RenameRefactorer;
        }
    }

    public RenameRefactorer RenameRefactorer { get; set; }

    public virtual bool ShouldRenameRefactor { get { return true; } }

    public FilterCollapsedDictionary CollapsedValues
    {
        get { return _collapsedValues; }
        set { _collapsedValues = value; }
    }

    public bool ImportedOnly
    {
        get { return true; }
    }

    public FilterLocations Locations
    {
        get { return _locations; }
        set { _locations = value; }
    }

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

    public virtual bool EndEditing()
    {
        IsEditing = false;

        if (Data.NodeItems.Count(p => p.Name == Name) > 1)
        {
            Name = OldName;
            return false;
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

    public virtual void RefactorApplied()
    {
        RenameRefactorer = null;
    }

    public void Remove(IDiagramNode diagramNode)
    {
        Filter.Locations.Remove(this.Identifier);
    }

    public virtual void RemoveFromDiagram()
    {
        //Data.RefactorCount--;
    }

    public void RemoveFromFilter(INodeRepository data)
    {
        Filter.Locations.Remove(this.Identifier);
    }

    public void Rename(IDiagramNode data, string name)
    {
        Rename(name);
    }

    public virtual void Rename(string newName)
    {
        Name = newName;
    }
}