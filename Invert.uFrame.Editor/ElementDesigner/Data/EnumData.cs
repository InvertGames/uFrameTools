using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnumData : DiagramNode
{
    
    [SerializeField]
    private List<EnumItem> _enumItems = new List<EnumItem>();

    public List<EnumItem> EnumItems
    {
        get { return _enumItems; }
        set { _enumItems = value; }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { return EnumItems.Cast<IDiagramNodeItem>(); }
        set { EnumItems = value.OfType<EnumItem>().ToList(); }
    }

    public override void MoveItemDown(IDiagramNodeItem nodeItem)
    {
        base.MoveItemDown(nodeItem);
        EnumItems.Move(EnumItems.IndexOf(nodeItem as EnumItem),false);
    }
    public override void MoveItemUp(IDiagramNodeItem nodeItem)
    {
        base.MoveItemDown(nodeItem);
        EnumItems.Move(EnumItems.IndexOf(nodeItem as EnumItem), true);
    }
    public override bool EndEditing()
    {
        if (!base.EndEditing()) return false;
        foreach (var item in Data.GetElements().SelectMany(p => p.Properties).Where(p => p.RelatedTypeName == OldName))
        {
            item.RelatedType = AssemblyQualifiedName;
        }
        foreach (var item in Data.GetElements().SelectMany(p => p.Commands).Where(p => p.RelatedTypeName == OldName))
        {
            item.RelatedType = AssemblyQualifiedName;
        }
        foreach (var item in Data.GetElements().SelectMany(p => p.Collections).Where(p => p.RelatedTypeName == OldName))
        {
            item.RelatedType = AssemblyQualifiedName;
        }
        return true;
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();

        Data.RemoveNode(this);
    }

    public Type CompiledType
    {
        get { return Type.GetType(AssemblyQualifiedName); }
    }

    public override string InfoLabel
    {
        get { return null; }
    }

    public override string Label { get { return Name; }}

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get { return EnumItems.Cast<IDiagramNodeItem>(); }
    }

    public override void CreateLink(IDiagramNode container, IGraphItem target)
    {
        
    }

    public override bool CanCreateLink(IGraphItem target)
    {
        return false;
    }

    public override IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] nodes)
    {
        yield break;
    }

    public override void RemoveLink(IDiagramNode target)
    {
        
    }
}