using System.CodeDom;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ViewModelCollectionData : DiagramNodeItem, IBindableTypedItem
{
  
    public string ViewFieldName
    {
        get { return string.Format("_{0}", Name); }
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ItemType", new JSONData(_itemType));
       // nodeItemClass.Add("ItemType", new JSONData(_itemType));
        //nodeItemClass.Add("IsRealTime", new JSONData(_isRealTimeProperty));
    }

    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);

        _itemType = cls["ItemType"].Value.Split(',')[0].Split('.').Last();

    }

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

    public void SetType(IDesignerType input)
    {
        this.RelatedType = input.Identifier;
    }

    public void RemoveType()
    {
        this.RelatedType = typeof (string).Name;
    }

    public CodeTypeReference GetFieldType()
    {
        var t = new CodeTypeReference(uFrameEditor.UFrameTypes.ModelCollection);
        t.TypeArguments.Add(new CodeTypeReference(RelatedTypeName));
        return t;
    }

    public CodeTypeReference GetPropertyType()
    {
        var t = new CodeTypeReference(uFrameEditor.UFrameTypes.ModelCollection);
        t.TypeArguments.Add(new CodeTypeReference(RelatedTypeName));
        return t;
    }

    public string FieldName
    {
        get
        {
            return string.Format("_{0}Property", Name);
        }
    }

    public string NameAsChangedMethod
    {
        get { return string.Format("{0}Changed", Name); }
    }

    public override string FullLabel { get { return RelatedTypeName + Name; } }

    public Type ItemType
    {
        get
        {
            
            return Type.GetType(_itemType);
        }
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
            var relatedNode = this.RelatedNode();
            if (relatedNode != null)
            {
                return relatedNode.Name;
            }
            return RelatedType ?? string.Empty;
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