using Invert.Core.GraphDesigner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.Json;
using UnityEngine;

public abstract class DiagramNodeItem : IDiagramNodeItem
{
    public virtual string Title { get { return Name; } }

    public virtual string Group
    {
        get { return Node.Name; }
    }

    public virtual string SearchTag { get { return Name; } }
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

    string IGraphItem.Label
    {
        get { return Name; }
    }

    public bool IsValid
    {
        get { return true; }
    }

    public IGraphItem Copy()
    {
        var jsonNode = new JSONClass();
        Serialize(jsonNode);
        var copy = Activator.CreateInstance(this.GetType()) as DiagramNodeItem;
        copy.Deserialize(jsonNode);
        copy._identifier = null;
        return copy;
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
        cls.Add("Precompiled", new JSONData(Precompiled));
        cls.AddObject("Flags", Flags);
        cls.AddObject("DataBag", DataBag);
    }

    public virtual void Deserialize(JSONClass cls)
    {
        _name = cls["Name"].Value;
        _identifier = cls["Identifier"].Value;
        if (cls["Precompiled"] != null)
        {
            Precompiled = cls["Precompiled"].AsBool;
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
    }

    private string _identifier;

    private bool _isSelected;
    private string _name = string.Empty;


    private RenameRefactorer _renameRefactorer;

    private string _oldName;

    private Rect _position;

    public abstract string FullLabel { get; }

    public virtual string Highlighter { get { return null; } }
    [InspectorProperty]
    public string Identifier
    {
        get
        {
            return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier;
        }
        set { _identifier = value; }
    }

    public virtual bool IsSelectable { get { return true; } }
    public DiagramNode Node  { get; set; }

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

    public virtual string Label
    {
        get { return Name; }
    }

    public bool Precompiled { get; set; }

    public virtual string Name
    {
        get { return _name; }
        set
        {
            var oldName = _name;
            if (AutoFixName)
                _name = Regex.Replace(value, @"[^a-zA-Z0-9_\.]+", "");
            else
            {
                _name = value;
            }
            if (Node != null)
            Node.TrackChange(new NameChange(this,oldName, _name));
        }
    }

    public virtual bool AutoFixName
    {
        get { return true; }
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
        
    }

    //public abstract IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] diagramNode);

    public void RefactorApplied()
    {
        RenameRefactorer = null;
    }

    public abstract void Remove(IDiagramNode diagramNode);

    public string Namespace { get; set; }

    public virtual void Rename(IDiagramNode data, string name)
    {
        Name = name;
    }

    public virtual void NodeRemoved(IDiagramNode nodeData)
    {
        if (this is ITypedItem)
        {
            var typeItem = this as IBindableTypedItem;
            if (typeItem != null && typeItem.RelatedType == nodeData.Identifier)
            {
                typeItem.RemoveType();
            }
        }

        if (this[Identifier])
        {
            this[Identifier] = false;
        }
        
    }
    
    public virtual void NodeItemRemoved(IDiagramNodeItem nodeItem)
    {
        if (this[Identifier])
        {
            this[Identifier] = false;
        }
    }

    public virtual void NodeAdded(IDiagramNode data)
    {
       
    }

    public virtual void NodeItemAdded(IDiagramNodeItem data)
    {
        
    }

    public virtual void Validate(List<ErrorInfo> info)
    {
        
    }

    public virtual bool ValidateInput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
    {
        //if (arg1.GetType() == arg2.GetType()) return false;

        return true;
    }

    public virtual bool ValidateOutput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
    {
        //if (arg1.GetType() == arg2.GetType()) return false;
        return true;
    }

    public virtual void Document(IDocumentationBuilder docs)
    {
        docs.Title3(Name);
    }

    
    public IGraphData Graph
    {
        get { return this.Node.Graph; }
    }

    public IEnumerable<ConnectionData> Inputs
    {
        get
        {
            if (Node == null)
            {
                throw new Exception("NODE IS NULL");
            }
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
            //if (Node == null) yield break;
            //if (Node.Project == null) yield break;
            if (Node == null)
            {
                throw new Exception("NODE IS NULL");
            }
            if (Node.Project == null) yield break;
            foreach (var connectionData in Node.Project.Connections)
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

}