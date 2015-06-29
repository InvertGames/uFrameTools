using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;
using UnityEngine;

[Serializable]
public class ViewModelCommandData : DiagramNodeItem, IBindableTypedItem
{
    public string ViewFieldName
    {
        get { return string.Format("_{0}", Name); }
    }
    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ItemType", new JSONData(_parameterType));
        cls.Add("IsYield", new JSONData(_isYield));

    }

    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);
        _parameterType = cls["ItemType"].Value.Split(',')[0].Split('.').Last();
        _isYield = cls["IsYield"].AsBool;
    }

    [SerializeField]
    private bool _isYield;

    [SerializeField]
    private string _parameterType;

    [SerializeField]
    private string _transitionName;
    [NonSerialized]
    private ElementData _element;

    public bool AllowEmptyRelatedType
    {
        get { return true; }
    }

    public IEnumerable<string> BindingMethodNames
    {
        get
        {
            yield break;
        }
    }

    public void SetType(IDesignerType input)
    {
        this.RelatedType = input.Identifier;
    }

    public void RemoveType()
    {
        this.RelatedType = null;
    }

    public CodeTypeReference GetFieldType()
    {
        var relatedNode = this.RelatedNode();
        if (relatedNode != null)
        {
            return relatedNode.GetFieldType(this);
        }
        var t = new CodeTypeReference(uFrameEditor.UFrameTypes.Computed);
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

    public ElementData Element
    {
        get { return _element; }
        set { _element = value; }
    }

    public string FieldName
    {
        get
        {
            return string.Format("_{0}", Name);
        }
    }

    public string NameAsChangedMethod { get; set; }

    public override string FullLabel
    {
        get { return (string.IsNullOrEmpty(RelatedTypeName) ? " [ NONE ] " : RelatedTypeName) + Name; }
    }

    public override string Highlighter
    {
        get
        {
            if (IsYield)
            {
                return "Yield";
            }
            return null;
        }
    }

    public override bool IsSelectable { get { return true; } }

    [Obsolete]
    public bool IsYield
    {
        get { return _isYield; }
        set { _isYield = value; }
    }

    public override string Label
    {
        get
        {
            if (string.IsNullOrEmpty(RelatedTypeName))
                return Name;

            return RelatedTypeName + ":" + Name;
        }
    }

    public string NameAsExecuteMethod
    {
        get { return string.Format("Execute{0}", this.Name); }
    }

    public string NameAsSettingsField
    {
        get { return string.Format("_{0}Transition", Name); }
    }

    public Type ParameterType
    {
        get
        {
            if (string.IsNullOrEmpty(_parameterType))
                return null;
            return Type.GetType(_parameterType);
        }
        set
        {
            if (value != null)
                _parameterType = value.AssemblyQualifiedName;
        }
    }

    public string RelatedType
    {
        get { return _parameterType; }
        set { _parameterType = value; }
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

    public string TransitionToIdentifier
    {
        get { return _transitionName; }
        set { _transitionName = value; }
    }

    public override void Remove(IDiagramNode diagramNode)
    {
        var data = diagramNode as ElementDataBase;
        if (data != null)
        {
            data.Commands.Remove(this);
            foreach (var sceneManagerData in data.Graph.GetSceneManagers())
                sceneManagerData.Transitions.RemoveAll(p => p.CommandIdentifier == this.Identifier);
            data.Dirty = true;
        }
    }

    public override void Rename(IDiagramNode data, string name)
    {
        base.Rename(data, name);
        foreach (var sceneManagerData in data.Graph.GetSceneManagers())
        {
            foreach (var transition in sceneManagerData.Transitions.ToArray())
            {
                if (transition.CommandIdentifier == Identifier)
                { 
                    transition.Name = name;
                }
            }
        }
    }


}