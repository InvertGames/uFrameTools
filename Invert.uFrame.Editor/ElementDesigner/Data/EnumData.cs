using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class EnumData : DiagramNode,IDesignerType
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
        return true;
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();

        Project.RemoveNode(this);
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

}