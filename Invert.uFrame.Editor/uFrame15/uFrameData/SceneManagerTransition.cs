using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;
using UnityEngine;

[Serializable]
public class SceneManagerTransition : IDiagramNodeItem
{
    public string Title { get { return Name; } }
    public string SearchTag { get { return Name; } }
    public virtual void Serialize(JSONClass cls)
    {
        cls.Add("ToIdentifier",_toIdentifier);
        cls.Add("Name",_name);
        cls.Add("FromCommand",_fromCommand);
        cls.AddObject("DataBag", DataBag);
    }

    public void Deserialize(JSONClass cls, INodeRepository repository)
    {
        _name = cls["Name"].Value;
        _toIdentifier = cls["ToIdentifier"].Value;
        _fromCommand = cls["FromCommand"].Value;
        if (cls["DataBag"] is JSONClass)
        {
            var flags = cls["DataBag"].AsObject;
            DataBag = new DataBag();
            DataBag.Deserialize(flags, repository);
        }

    }
    [SerializeField]
    private string _toIdentifier;
    [SerializeField]
    private string _name;

    [SerializeField]
    private string _fromCommand;

    public string CommandIdentifier
    {
        get { return _fromCommand; }
        set { _fromCommand = value; }
    }

    public ViewModelCommandData Command
    {
        get { return Node.Project.GetElements().SelectMany(p=>p.Commands).FirstOrDefault(p=>p.Identifier == CommandIdentifier); }
    }
    public string ToIdentifier
    {
        get { return _toIdentifier; }
        set { _toIdentifier = value; }
    }

    public Rect Position { get; set; }

    public string Label
    {
        get { return Name; }
    }

    public bool IsSelected { get; set; }
    public void RemoveLink(IDiagramNode target)
    {
        ToIdentifier = null;
    }

    public string Name
    {
        get { return _name; }
        set { _name = Regex.Replace(value, "[^a-zA-Z0-9_.]+", ""); }
    }

    public string Highlighter { get; private set; }
    public string FullLabel { get { return Name; } }
    [SerializeField]
    private string _identifier;

    private DataBag _dataBag = new DataBag();
    private FlagsDictionary _flags;
    public string Identifier{ get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier;}}

    public bool IsValid
    {
        get { return true; }
    }

    public IGraphItem Copy()
    {
        return null;
    }

    public bool IsSelectable { get { return false; } }
    public DiagramNode Node { get; set; }

    public DataBag DataBag
    {
        get { return _dataBag; }
        set { _dataBag = value; }
    }

    public bool IsEditing { get; set; }

    public FlagsDictionary Flags
    {
        get { return _flags ?? (_flags = new FlagsDictionary()); }
        set { _flags = value; }
    }

    public string NameAsSettingsField
    {
        get { return string.Format("_{0}Transition", Name); }
    }

    public void Remove(IDiagramNode diagramNode)
    {
        var sceneManagerData = Node as SceneManagerData;
        if (sceneManagerData != null) 
            sceneManagerData.Transitions.Remove(this);
    }

    public void Rename(IDiagramNode data, string name)
    {
        Name = name;
    }

    public void NodeRemoved(IDiagramNode nodeData)
    {
        
    }

    public void NodeItemRemoved(IDiagramNodeItem nodeItem)
    {
        
    }

    public void NodeAdded(IDiagramNode data)
    {
        
    }

    public void NodeItemAdded(IDiagramNodeItem data)
    {
        
    }

    public void Validate(List<ErrorInfo> info)
    {
        
    }

    public bool ValidateInput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
    {
        return false;
    }

    public bool ValidateOutput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
    {
        return false;
    }


    public void Disconnect()
    {
        ToIdentifier = null;
    }

    public void ConnectTo(SceneManagerData input)
    {
        ToIdentifier = input.Identifier;
    }

    public IEnumerable<ConnectionData> Inputs
    {
        get { yield break; }
    }

    public IEnumerable<ConnectionData> Outputs
    {
        get { yield break; }
    }

    public bool AllowMultipleInputs
    {
        get { return false; }
    }

    public bool AllowMultipleOutputs
    {
        get { return false; }
    }

    public void OnConnectionApplied(IConnectable output, IConnectable input)
    {
        
    }
}

