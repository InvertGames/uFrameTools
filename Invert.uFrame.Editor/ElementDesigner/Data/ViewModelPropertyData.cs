using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

[Serializable]
public class ViewModelPropertyData : DiagramNodeItem, IViewModelItem,ISerializeablePropertyData
{

    public string Title { get { return Name; } }
    public string SearchTag { get { return Name; } }
    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ItemType", new JSONData(_type));
        cls.Add("IsRealTime", new JSONData(_isRealTimeProperty));
        cls.AddPrimitiveArray("DependantOn", _dependantPropertyIdentifiers, i => new JSONData(i));
    }

    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);
        _type = cls["ItemType"].Value;
        _isRealTimeProperty = cls["IsRealTime"].AsBool;
        if (cls["DependantOn"] != null)
        {
            _dependantPropertyIdentifiers = cls["DependantOn"].DeserializePrimitiveArray(n => n.Value).ToList();
        }
        
    }

    //string IDiagramNodeItem.Highlighter
    //{
    //    get
    //    {
    //        return IsComputed ? "Computed" : null;
    //    }
    //}

    [SerializeField]
    private string _type = string.Empty;

    [SerializeField]
    private bool _isRealTimeProperty;

    private List<string> _dependantPropertyIdentifiers = new List<string>();

    public Type Type
    {
        get
        {
            if (_type == null)
            {
                return typeof(string);
            }
            return Type.GetType(_type);
        }
        set { _type = value.AssemblyQualifiedName; }
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

    public override void CreateLink(IDiagramNode container, IGraphItem target)
    {
        var element = target as IDiagramNode;
        if (element != null)
        {
            RelatedType = element.AssemblyQualifiedName;
        }
    }

    public override bool CanCreateLink(IGraphItem target)
    {
        return target is ElementDataBase || target is EnumData;
    }

    public override void RemoveLink(IDiagramNode target)
    {
        RelatedType = typeof(string).AssemblyQualifiedName;
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

    public bool IsComputed
    {
        get { return DependantPropertyIdentifiers.Count > 0; }
    }

    public List<string> DependantPropertyIdentifiers
    {
        get { return _dependantPropertyIdentifiers; }
        set { _dependantPropertyIdentifiers = value; }
    }

    public IEnumerable<ViewModelPropertyData> DependantProperties
    {
        get
        {
            var properties = Node.Data.GetElements().SelectMany(p => p.Properties).ToArray();
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

            return RelatedType.Split(',').FirstOrDefault() ?? "No Type";
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

    public override IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] diagramNode)
    {
        foreach (var viewModelData in diagramNode)
        {
            if (viewModelData.Name == null) continue;
            if (viewModelData.Name == RelatedTypeName)
            {
                yield return new AssociationLink()
                {
                    Item = this,
                    Element = viewModelData
                };
            }
        }
        if (!Node.IsCollapsed)
        {
            foreach (var viewModelPropertyData in DependantProperties)
            {
                yield return new DependencyLink()
                {
                    Item = this,
                    To = viewModelPropertyData
                };
            }
            
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
        var properties = data.Data.GetElements().SelectMany(p=>p.Properties);
        foreach (var property in properties)
        {
            if (property.DependantPropertyIdentifiers.Contains(this.Identifier))
            {
                property.DependantPropertyIdentifiers.Remove(this.Identifier);
            }
        }

        data.Dirty = true;
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

    public string NameAsComputeMethod
    {
        get { return string.Format("Compute{0}",Name); }
    }
}
