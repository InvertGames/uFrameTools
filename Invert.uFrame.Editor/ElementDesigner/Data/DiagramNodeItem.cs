using Invert.MVVM;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class DiagramNodeItem : IDiagramNodeItem
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


    public FlagsDictionary Flags
    {
        get { return _flags ?? (_flags = new FlagsDictionary()); }
        set { _flags = value; }
    }
    private FlagsDictionary _flags = new FlagsDictionary();


    public DataBag DataBag
    {
        get { return _dataBag ?? (_dataBag = new DataBag()); }
        set { _dataBag = value; }
    }

    public bool IsEditing { get; set; }
    private DataBag _dataBag = new DataBag();



    public virtual void Serialize(JSONClass cls)
    {
        cls.Add("Name", new JSONData(_name));
        cls.Add("Identifier", new JSONData(_identifier));
        cls.AddObject("Flags", Flags);
        cls.AddObject("DataBag", DataBag);
    }

    public virtual void Deserialize(JSONClass cls, INodeRepository repository)
    {
        _name = cls["Name"].Value;
        _identifier = cls["Identifier"].Value;
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
    }

    [SerializeField]
    private string _identifier;

    [NonSerialized]
    private bool _isSelected;

    [SerializeField]
    private string _name = string.Empty;

    [NonSerialized]
    private RenameRefactorer _renameRefactorer;
    [NonSerialized]
    private string _oldName;
    [NonSerialized]
    private Rect _position;

    public abstract string FullLabel { get; }

    public virtual string Highlighter { get { return null; } }

    public string Identifier { get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier; } }

    public virtual bool IsSelectable { get { return true; } }
    public DiagramNode Node  { get; set; }

    private P<bool> _IsSelectedProperty = new P<bool>(false);

    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            //if (value == false && _isSelected)
            //{
            //    EndEditing();
            //}
            //else if (value == true && !_isSelected)
            //{
            //    BeginEditing();
            //}
            _isSelected = value;
        }
    }

    public abstract string Label { get; }

    public virtual string Name
    {
        get { return _name; }
        set { _name = Regex.Replace(value, "[^a-zA-Z0-9_.]+", ""); }
    }

    public string OldName
    {
        get { return _oldName; }
        set { _oldName = value; }
    }

    public Rect Position
    {
        get { return _position; }
        set { _position = value; }
    }

    public IEnumerable<Refactorer> Refactorings
    {
        get { yield break; }
    }

    public RenameRefactorer RenameRefactorer
    {
        get { return _renameRefactorer; }
        set { _renameRefactorer = value; }
    }

    public virtual void BeginEditing()
    {
        if (RenameRefactorer == null)
        {
            RenameRefactorer = CreateRenameRefactorer();
        }
        OldName = Name;
    }

    public virtual RenameRefactorer CreateRenameRefactorer()
    {
        return null;
    }

    public virtual void EndEditing()
    {
        if (OldName != Name)
        {
            if (RenameRefactorer == null)
            {
                return;
            }

            RenameRefactorer.Set(this);
        }
    }

    //public abstract IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] diagramNode);

    public void RefactorApplied()
    {
        RenameRefactorer = null;
    }

    public abstract void Remove(IDiagramNode diagramNode);

    public virtual void Rename(IDiagramNode data, string name)
    {
        Name = name;
    }

    public virtual void NodeRemoved(IDiagramNode item)
    {
        if (this is ITypeDiagramItem)
        {
            var typeItem = this as ITypeDiagramItem;
            if (typeItem != null && typeItem.RelatedType == item.Identifier)
            {
                typeItem.RemoveType();
            }
        }
        
    }


}