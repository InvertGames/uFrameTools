using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

[Serializable]
public class ViewModelPropertyData : DiagramNodeItem, ITypeDiagramItem,ISerializeablePropertyData
{

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ItemType", new JSONData(_type));
        cls.Add("IsRealTime", new JSONData(_isRealTimeProperty));
        cls.AddPrimitiveArray("DependantOn", _dependantPropertyIdentifiers, i => new JSONData(i));
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        _type = cls["ItemType"].Value.Split(',')[0].Split('.').Last();
        _isRealTimeProperty = cls["IsRealTime"].AsBool;
        if (cls["DependantOn"] != null)
        {
            _dependantPropertyIdentifiers = cls["DependantOn"].DeserializePrimitiveArray(n => n.Value).ToList();
        }
        
    }

    [SerializeField]
    private string _type = string.Empty;

    [SerializeField]
    private bool _isRealTimeProperty;

    private List<string> _dependantPropertyIdentifiers = new List<string>();

    public Type Type
    {
        get
        {
            if (string.IsNullOrEmpty(_type)) return null;
            return Type.GetType(_type);
        }
    }

    public object DefaultValue { get; set; }

    public string NameAsChangedMethod
    {
        get { return string.Format("{0}Changed", Name); }
    }
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

    public string RelatedType
    {
        get { return _type; }
        set { _type = value; }
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

    public string RelatedTypeName
    {
        get
        {
            var relatedNode = this.RelatedNode();
            if (relatedNode != null)
                return relatedNode.Name;

            return RelatedType;
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
    public IDiagramNode TypeNode()
    {
        return this.RelatedNode();
    }

    public bool AllowEmptyRelatedType
    {
        get { return false; }
    }

    public IEnumerable<string> BindingMethodNames
    {
        get
        {
            yield return NameAsChangedMethod;
        }
    }

    public void SetType(IDesignerType input)
    {
        this.RelatedType = input.Identifier;
    }

    public void RemoveType()
    {
        this.RelatedType = typeof(string).Name;
    }

    public CodeTypeReference GetFieldType()
    {
        var relatedNode = this.RelatedNode();
        if (relatedNode != null)
        {
            return relatedNode.GetFieldType(this);
        }
        var t = new CodeTypeReference(uFrameEditor.UFrameTypes.P);
        t.TypeArguments.Add(new CodeTypeReference(RelatedTypeName));
        return t;
    }

    public CodeTypeReference GetPropertyType()
    {
        var relatedNode = this.RelatedNode();
        if (relatedNode != null)
        {
            return relatedNode.GetPropertyType(this);
        }
        return new CodeTypeReference(RelatedTypeName);
    }


    public string FieldName
    {
        get
        {
            return string.Format("_{0}Property", Name);
        }
    }

    public string ViewFieldName
    {
        get
        {
            return string.Format("_{0}", Name);
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

    public override void NodeRemoved(IDiagramNode item)
    {
       base.NodeRemoved(item);
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
