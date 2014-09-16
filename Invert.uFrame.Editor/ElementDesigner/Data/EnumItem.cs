using System;
using System.Collections.Generic;
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

  
    [SerializeField]
    private string _name;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public string Highlighter
    {
        get { return null; }
    }

    public string FullLabel { get { return  Name; } }
    public string Identifier{ get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier;}}
    public bool IsSelectable { get { return true; } }
    public DiagramNode Node { get; set; }

    public DataBag DataBag
    {
        get { return _dataBag ?? (_dataBag = new DataBag()); }
        set { _dataBag = value; }
    }

    public bool IsEditing { get; set; }

    [SerializeField]
    private string _identifier;

    private DataBag _dataBag;

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

    public void NodeRemoved(IDiagramNode item)
    {
        
    }


    public Vector2[] ConnectionPoints { get; set; }

    public Rect Position { get; set; }

    public string Label
    {
        get { return Name; }
    }


    public bool IsSelected { get; set; }

}