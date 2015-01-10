using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

[Serializable]
public class ViewModelPropertyData : BindableTypedNodeItem
{
    public string ViewIdentifier
    {
        get { return DataBag["ViewIdentifier"]; }
        set { DataBag["ViewIdentifier"] = value; }
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
     
        cls.Add("IsRealTime", new JSONData(_isRealTimeProperty));
        
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        _isRealTimeProperty = cls["IsRealTime"].AsBool;
        
    }

    [SerializeField]
    private bool _isRealTimeProperty;

    private List<string> _dependantPropertyIdentifiers = new List<string>();

    public object DefaultValue { get; set; }

    public string NameAsTwoWayMethod
    {
        get { return string.Format("Get{0}TwoWayValue", Name); }
    }
    public override string Label
    {
        get { return RelatedTypeName + ": " + Name; }
    }

    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenamePropertyRefactorer(this);
    }

    public bool IsRealTimeProperty
    {
        get { return _isRealTimeProperty; }
        set { _isRealTimeProperty = value; }
    }

    //public bool IsComputed
    //{
    //    get { return DependantPropertyIdentifiers.Count > 0; }
    //}

    public List<string> DependantPropertyIdentifiers
    {
        get { return _dependantPropertyIdentifiers; }
        set { _dependantPropertyIdentifiers = value; }
    }

    public IEnumerable<ViewModelPropertyData> DependantProperties
    {
        get
        {
            var properties = Node.Project.GetElements().SelectMany(p => p.Properties).ToArray();
            foreach (var property in DependantPropertyIdentifiers)
            {
                var result = properties.FirstOrDefault(p => p.Identifier == property);
                if (result != null)
                    yield return result;
            }
        }
    }

    public string RelatedTypeNameOrViewModel
    {
        get
        {
            var relatedNode = this.RelatedNode();
            if (relatedNode != null)
            {
                var element = relatedNode as ElementData;
                if (element != null)
                    return element.NameAsViewModel;

                return relatedNode.Name;
            }
                

            return RelatedType;
        }
    }

    public IEnumerable<string> BindingMethodNames
    {
        get
        {
            yield return NameAsChangedMethod;
        }
    }


    public override string Highlighter
    {
        get { return null; }
    }

    public override string FullLabel { get { return RelatedTypeName + Name; } }

    public override bool IsSelectable { get { return true; } }

    public override void Remove(IDiagramNode diagramNode)
    {
        var data = diagramNode as ElementData;
        if (data == null) return;

        data.Properties.Remove(this);

        // Make sure we remove any properties that are dependent on this
        var properties = data.Project.GetElements().SelectMany(p=>p.Properties);
        foreach (var property in properties)
        {
            if (property.DependantPropertyIdentifiers.Contains(this.Identifier))
            {
                property.DependantPropertyIdentifiers.Remove(this.Identifier);
            }
        }

        data.Dirty = true;
    }

    public override void NodeRemoved(IDiagramNode nodeData)
    {
       base.NodeRemoved(nodeData);
        if (ViewIdentifier == nodeData.Identifier)
        {
            ViewIdentifier = null;
        }

    }

    public string NameAsPrefabBindingOption
    {
        get { return string.Format("_{0}Prefab", Name); }
    }

    public string NameAsTwoWayBindingOption
    {
        get { return string.Format("_{0}IsTwoWay", Name); }
    }
    public string NameAsBindingOption
    {
        get { return string.Format("_Bind{0}", Name); }
    }



    public string TransitionId { get; set; }
}