using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.uFrame.Editor;
using UnityEngine;

[Serializable]
public class SceneManagerTransition : IDiagramNodeItem
{
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
    public string Identifier{ get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier;}}
    public bool IsSelectable { get { return false; } }
    public DiagramNode Node { get; set; }

    public DataBag DataBag
    {
        get { return _dataBag; }
        set { _dataBag = value; }
    }

    public bool IsEditing { get; set; }

    public string NameAsSettingsField
    {
        get { return string.Format("_{0}Transition", Name); }
    }

    public void Remove(IDiagramNode diagramNode)
    {
        //var SceneManagerData = data as SceneManagerData;
        //if (SceneManagerData != null) 
        //    SceneManagerData.Transitions.Remove(this);
    }

    public void Rename(IDiagramNode data, string name)
    {
        Name = name;
    }

    public void Disconnect()
    {
        ToIdentifier = null;
    }

    public void ConnectTo(SceneManagerData input)
    {
        ToIdentifier = input.Identifier;
    }
}

