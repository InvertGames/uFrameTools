using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class DiagramNode : IDiagramNode, IRefactorable
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
            return Data.LocalNodes.All(p => p.Identifier != Identifier);
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
        var filter = this as IDiagramFilter;
        if (filter != null)
        {
            cls.Add("Locations", filter.Locations.Serialize());
            cls.Add("CollapsedValues", filter.CollapsedValues.Serialize());
        }
        cls.AddObject("Flags", Flags);
        cls.AddObject("DataBag", DataBag);

        //var itemsArray = new JSONArray();
        //foreach (var diagramNodeItem in ContainedItems)
        //{
        //    var nodeItemClass = new JSONClass { { "Type", diagramNodeItem.GetType().Name } };
        //    diagramNodeItem.Serialize(nodeItemClass);

        //    itemsArray.Add(nodeItemClass);
        //}
        //cls.Add("Items", itemsArray);
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

    public virtual void Deserialize(JSONClass cls)
    {
        _name = cls["Name"].Value;
        _isCollapsed = cls["IsCollapsed"].AsBool;
        _identifier = cls["Identifier"].Value;
        IsNewNode = false;
        ContainedItems = cls["Items"].AsArray.DeserializeObjectArray<IDiagramNodeItem>();

        var filter = this as IDiagramFilter;
        if (filter != null)
        {
            filter.Locations.Deserialize(cls["Locations"].AsObject);
            filter.CollapsedValues.Deserialize(cls["CollapsedValues"].AsObject);

            //cls.Add("Locations", filter.Locations.Serialize());
            //cls.Add("CollapsedValues", filter.CollapsedValues.Serialize());
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

    public IEnumerable<Refactorer> AllRefactorers
    {
        get { return Refactorings.Concat(Items.OfType<IRefactorable>().SelectMany(p => p.Refactorings)); }
    }

    public virtual string AssemblyQualifiedName
    {
        get
        {
            return uFrameEditor.uFrameTypes.ViewModel.AssemblyQualifiedName.Replace("ViewModel", Name);
        }
    }

    public Vector2[] ConnectionPoints { get; set; }

    public virtual IElementDesignerData Data
    {
        get
        {
            return _data;
        }
        set
        {
            _data = value;
            if (value != null)
            {
                //if (!value.Locations.Keys.Contains(Identifier))
                //{
                //    value.Locations[this] = Location;
                //}
                _location = value.CurrentFilter.Locations[this];
                _isCollapsed = value.CurrentFilter.CollapsedValues[this];
                Dirty = true;
            }
        }
    }
    
    public virtual IElementDesignerData OwnerData
    {
        get { return Data; }
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
            //var count = AllRefactorers.Count();
            //if (count == 0) return null;
            //return string.Format("Refactors: {0}", count);
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
            //if (Filter == this)
            //{
            //    return new Vector2((Screen.width / 2f) - (Position.width / 2f), (Screen.height / 2f) - (Position.height / 2f));
            //}
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

    public abstract bool CanCreateLink(IGraphItem target);

    public abstract void CreateLink(IDiagramNode container, IGraphItem target);

    public virtual RenameRefactorer CreateRenameRefactorer()
    {
        return null;
    }

    public virtual bool EndEditing()
    {
        IsEditing = false;

        if (Data.LocalNodes.Count(p => p.Name == Name) > 1)
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
            Data.RefactorCount++;
        }
        return true;
    }

    public abstract IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] nodes);

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
        Data.RefactorCount--;
    }

    public void RemoveFromFilter(IElementDesignerData data)
    {
        Filter.Locations.Remove(this.Identifier);
    }

    public abstract void RemoveLink(IDiagramNode target);

    public void Rename(IDiagramNode data, string name)
    {
        Rename(name);
    }

    public virtual void Rename(string newName)
    {
        Name = newName;
    }
}