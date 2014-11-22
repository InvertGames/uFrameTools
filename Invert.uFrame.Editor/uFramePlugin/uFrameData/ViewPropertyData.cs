using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using UnityEngine;

public class ViewPropertyData : DiagramNodeItem,ISerializeablePropertyData,IBindableTypedItem
{
    private string _componentTypeName = "";
    private string _componentTypeShortName = "";

    private string _componentProperty = "";
    private Type _componentAssemblyType = null;
    private string _type;

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ItemType", new JSONData(_type));
    }


    public override void NodeRemoved(IDiagramNode nodeData)
    {
        base.NodeRemoved(nodeData);
        

    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);

        _type = cls["ItemType"].Value.Split(',')[0].Split('.').Last();
    }

    string ISerializeablePropertyData.Name
    {
        get { return NameAsProperty; }
    }
    public override string FullLabel
    {
        get { return Label; }
    }

    public Type Type
    {
        get { return MemberType; }
    }


    public string RelatedType
    {
        get { return _type ?? (_type = typeof(string).Name); }
        set { _type = value; }
    }

    public string RelatedTypeName
    {
        get { return RelatedType; }
    }

    public bool AllowEmptyRelatedType { get { return false; } }
    public string FieldName { get { return NameAsField; } }

    public string NameAsChangedMethod
    {
        get { return "Get" + Name; }
    }

    public string ViewFieldName
    {
        get { return NameAsField; }
    }

    public void SetType(IDesignerType input)
    {
        RelatedType = input.Identifier;
    }

    public void RemoveType()
    {
        RelatedType = typeof (string).Name;
    }

    public CodeTypeReference GetFieldType()
    {
        throw new NotImplementedException();
    }

    public CodeTypeReference GetPropertyType()
    {
        throw new NotImplementedException();
    }

    public IDiagramNode TypeNode()
    {
        return null;
    }

    public override string Label
    {
        get
        {
            return string.Format("{0}.{1}",_componentTypeShortName ?? string.Empty,ComponentProperty);
        }
    }

    public string Expression
    {
        get { return string.Format("{0}.{1}", NameAsCachedProperty, ComponentProperty); }
    }

    public string NameAsCachedProperty
    {
        get
        {
            var name = ComponentTypeName.Split('.').Last();

            return name.Substring(0, 1).ToUpper() + name.Substring(1);
        }
    }
    public string NameAsCachedPropertyField
    {
        get
        {
            var name = ComponentTypeName.Split('.').Last();

            return string.Format("_{0}{1}", name.Substring(0, 1).ToLower(), name.Substring(1));
        }
    }
    public string ComponentTypeName
    {
        get { return _componentTypeName; }
        set
        {
            _componentTypeName = value;
            ComponentAssemblyType = InvertApplication.FindType(_componentTypeName);
            //if (_componentAssemblyType != null)
            //{
            //    _componentTypeShortName = ComponentAssemblyType.Name;
            //}
        }
    }

    public Type ComponentAssemblyType
    {
        get
        {
            if (_componentAssemblyType == null)
            {
                ComponentAssemblyType = typeof (Transform);
            }
            return _componentAssemblyType;
        }
        set
        {
            _componentAssemblyType = value ?? typeof(Transform);
            if (value != null)
            {
                _componentTypeName = value.FullName;
                _componentTypeShortName = value.Name;
            }
            
            
        }
    }

    public Type MemberType
    {
        get
        {
            var mi = MemberInfo;
            if (mi == null) return null;
            var propertyInfo = mi as PropertyInfo;
            if (propertyInfo == null)
            {
                var fieldInfo = mi as FieldInfo;
                if (fieldInfo == null) return null;
                return fieldInfo.FieldType;
            }
            return propertyInfo.PropertyType;
        }
    }
    public MemberInfo MemberInfo
    {
        get
        {
            var cat = ComponentAssemblyType;
            if (cat == null) return null;
            return cat.GetMember(ComponentProperty).FirstOrDefault();
        }
    }

    public string ComponentProperty
    {
        get
        {
            return _componentProperty;
        }
        set
        {
            _componentProperty = value;
        }
    }

    public string NameAsProperty
    {
        get
        {
            return string.Format("{0}{1}{2}",_componentTypeShortName, ComponentProperty.Substring(0, 1).ToUpper(), ComponentProperty.Substring(1));
        }
    }

    public string NameAsField
    {
        get
        {
            return string.Format("_{0}{1}", ComponentProperty.Substring(0, 1).ToLower(), ComponentProperty.Substring(1)); 
        }
    }

    
    public override void Remove(IDiagramNode diagramNode)
    {
        var view = diagramNode as ViewData;
        if (view != null)
        {
            view.Properties.Remove(this);
        }
    }

}