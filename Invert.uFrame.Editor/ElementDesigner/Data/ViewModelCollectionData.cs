using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ViewModelCollectionData : DiagramNodeItem, IViewModelItem
{
    public string Title { get { return Name; } }
    public string SearchTag { get { return Name; } }
    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ItemType", new JSONData(_itemType));
       // nodeItemClass.Add("ItemType", new JSONData(_itemType));
        //nodeItemClass.Add("IsRealTime", new JSONData(_isRealTimeProperty));
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        _itemType = cls["ItemType"].Value;

    }

    [SerializeField]
    private string _itemType;

    public bool AllowEmptyRelatedType
    {
        get { return false; }
    }

    public IEnumerable<string> BindingMethodNames
    {
        get
        {
            yield return NameAsAddHandler;
            yield return NameAsCreateHandler;
        }
    }

    public void SetType(ElementData input)
    {
        this.RelatedType = input.AssemblyQualifiedName;
    }

    public void RemoveType()
    {
        this.RelatedType = typeof (string).AssemblyQualifiedName;
    }

    public string FieldName
    {
        get
        {
            return string.Format("_{0}Property", Name);
        }
    }

    public override string FullLabel { get { return RelatedTypeName + Name; } }

    public Type ItemType
    {
        get { return Type.GetType(_itemType); }
        set { _itemType = value.AssemblyQualifiedName; }
    }

    public override string Label
    {
        get { return RelatedTypeName + "[]: " + Name; }
    }

    public string NameAsAddHandler
    {
        get { return string.Format("{0}Added", Name); }
    }
    public string NameAsRemoveHandler
    {
        get { return string.Format("{0}Removed", Name); }
    }
    public string NameAsBindingOption
    {
        get { return string.Format("_Bind{0}", Name); }
    }

    public string NameAsContainerBindingOption
    {
        get { return string.Format("_{0}Prefab", Name); }
    }

    public string NameAsCreateHandler
    {
        get { return string.Format("Create{0}View", Name); }
    }

    public string NameAsListBindingOption
    {
        get { return string.Format("_{0}List", Name); }
    }

    public string NameAsSceneFirstBindingOption
    {
        get { return string.Format("_{0}SceneFirst", Name); }
    }

    public string NameAsUseArrayBindingOption
    {
        get { return string.Format("_{0}UseArray", Name); }
    }

    public string RelatedType
    {
        get { return _itemType; }
        set { _itemType = value; }
    }

    public string RelatedTypeName
    {
        get
        {
            return RelatedType.Split(',').FirstOrDefault() ?? "No Type";
        }
    }

    public override RenameRefactorer CreateRenameRefactorer()
    {
        return new RenameCollectionRefactorer(this);
    }

    public override void Remove(IDiagramNode diagramNode)
    {
        var data = diagramNode as ElementDataBase;
        data.Collections.Remove(this);
        data.Dirty = true;
    }
}