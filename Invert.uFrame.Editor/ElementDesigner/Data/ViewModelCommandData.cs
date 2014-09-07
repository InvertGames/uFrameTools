using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

[Serializable]
public class ViewModelCommandData : DiagramNodeItem, IViewModelItem
{
    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ItemType", new JSONData(_parameterType));
        cls.Add("IsYield", new JSONData(_isYield));

    }

    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);
        _parameterType = cls["ItemType"].Value;
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

    public void SetType(ElementData input)
    {
        this.RelatedType = input.AssemblyQualifiedName;
    }

    public void RemoveType()
    {
        this.RelatedType = null;
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

    [DiagramContextMenu("Is Yield Command", 0)]
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
            if (string.IsNullOrEmpty(RelatedType))
                return null;
            return RelatedType.Split(',').FirstOrDefault() ?? "No Type";
        }
    }

    public string TransitionToIdentifier
    {
        get { return _transitionName; }
        set { _transitionName = value; }
    }

    public override bool CanCreateLink(IGraphItem target)
    {
        return target is ElementDataBase || target is SceneManagerData || target is EnumData || (target != null && target.CanCreateLink(this));
    }

    public override void CreateLink(IDiagramNode container, IGraphItem target)
    {
        var sceneManagerData = target as SceneManagerData;
        if (sceneManagerData != null)
        {
            var transition = sceneManagerData.Transitions.FirstOrDefault(p => p.CommandIdentifier == this.Identifier);
            transition.ToIdentifier = sceneManagerData.Identifier;
        } else if (target is EnumData || target is ElementDataBase)
        {
            var element = target as IDiagramNode;
            RelatedType = element.AssemblyQualifiedName;
        }
        else
        {
            target.CreateLink(container, this);
        }
    }

    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameCommandRefactorer(this);
    }


    public override IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] diagramNode)
    {
        foreach (var viewModelData in diagramNode)
        {
            if (viewModelData.Name == null) continue;
            //var scd = viewModelData as SceneManagerData;
            //if (scd != null)
            //{
            //    if (TransitionToIdentifier == scd.Identifier)
            //    {
            //        yield return new TransitionLink()
            //        {
            //            From = this,
            //            To = scd
            //        };
            //    }
            //} else
            if (viewModelData.Name == RelatedTypeName)
            {
                yield return new AssociationLink()
                {
                    Item = this,
                    Element = viewModelData
                };
            }
        }
    }

    [DiagramContextMenu("Delete", 0)]
    public override void Remove(IDiagramNode diagramNode)
    {
        var data = diagramNode as ElementDataBase;
        if (data != null)
        {
            data.Commands.Remove(this);
            foreach (var sceneManagerData in data.Data.GetSceneManagers())
                sceneManagerData.Transitions.RemoveAll(p => p.CommandIdentifier == this.Identifier);
            data.Dirty = true;
        }
    }

    public override void RemoveLink(IDiagramNode target)
    {
        
        RelatedType = null;

    }

    public override void Rename(IDiagramNode data, string name)
    {
        base.Rename(data, name);
        foreach (var sceneManagerData in data.Data.GetSceneManagers())
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

    public string Title { get { return Name; } }
    public string SearchTag { get { return Name; } }
}