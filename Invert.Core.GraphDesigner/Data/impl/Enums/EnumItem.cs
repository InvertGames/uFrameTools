using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;
using UnityEngine;

[Serializable]
public class EnumItem : IDiagramNodeItem
{
    public string Title { get { return Name; } }
    public string SearchTag { get { return Name; } }

    public void Serialize(JSONClass cls)
    {
        cls.Add("Name", new JSONData(_name));
        cls.Add("Identifier", new JSONData(_identifier));
        cls.AddObject("DataBag", DataBag);
    }

    public void Deserialize(JSONClass cls, INodeRepository repository)
    {
        _name = cls["Name"].Value;
        _identifier = cls["Identifier"].Value;
        if (cls["DataBag"] is JSONClass)
        {
            var flags = cls["DataBag"].AsObject;
            DataBag = new DataBag();
            DataBag.Deserialize(flags, repository);
        }
    }

    private string _name;

    public string Name
    {
        get { return _name; }
        set { _name = Regex.Replace(value, @"[^a-zA-Z0-9_\.]+", ""); }
    }

    public string Highlighter
    {
        get { return null; }
    }

    public string FullLabel { get { return  Name; } }
    public string Identifier{ get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier;}}

    public bool IsValid
    {
        get { return true; }
    }

    public IGraphItem Copy()
    {
        var jsonNode = new JSONClass();
        Serialize(jsonNode);
        var copy = Activator.CreateInstance(this.GetType()) as EnumItem;
        copy.Deserialize(jsonNode, Node.Project);
        copy._identifier = null;
        return copy;
    }

    public bool IsSelectable { get { return true; } }
    public DiagramNode Node { get; set; }

    public DataBag DataBag
    {
        get { return _dataBag ?? (_dataBag = new DataBag()); }
        set { _dataBag = value; }
    }

    public bool IsEditing { get; set; }

    public FlagsDictionary Flags
    {
        get { return _flags ?? (_flags = new FlagsDictionary()); }
        set { _flags = value; }
    }

    private string _identifier;

    private DataBag _dataBag;
    private FlagsDictionary _flags;

    public void Remove(IDiagramNode diagramNode)
    {
        var data = diagramNode as EnumData;
        data.EnumItems.Remove(this);
        data.Dirty = true;
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

    public bool ValidateInput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
    {
        return false;
    }

    public bool ValidateOutput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
    {
        return false;
    }


    public Vector2[] ConnectionPoints { get; set; }

    public Rect Position { get; set; }

    public string Label
    {
        get { return Name; }
    }


    public bool IsSelected { get; set; }

}