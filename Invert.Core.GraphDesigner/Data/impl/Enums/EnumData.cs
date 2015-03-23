using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public class EnumData : DiagramNode,IDesignerType, IViewPropertyType
{
    private List<EnumItem> _enumItems = new List<EnumItem>();

    public List<EnumItem> EnumItems
    {
        get { return _enumItems; }
        set { _enumItems = value; }
    }

    public override IEnumerable<IDiagramNodeItem> PersistedItems
    {
        get { return EnumItems.Cast<IDiagramNodeItem>(); }
        set { EnumItems = value.OfType<EnumItem>().ToList(); }
    }

    public override void NodeItemRemoved(IDiagramNodeItem item)
    {
        EnumItems.Remove(item as EnumItem);
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

    //public Type CompiledType
    //{
    //    get { return Type.GetType(AssemblyQualifiedName); }
    //}

    public override string InfoLabel
    {
        get { return null; }
    }

    public override string Label { get { return Name; }}

    public override IEnumerable<IDiagramNodeItem> DisplayedItems
    {
        get { return EnumItems.Cast<IDiagramNodeItem>(); }
    }

}

public class EnumNode : GenericNode , IClassTypeNode
{
    public string ClassName
    {
        get { return Name; }
    }

    [Section("Enum Items",SectionVisibility.Always)]
    public IEnumerable<EnumChildItem> Items {
        get
        {
            return ChildItems.OfType<EnumChildItem>();
        }
    }

}

public class EnumChildItem : GenericNodeChildItem
{
    
}

[TemplateClass(MemberGeneratorLocation.DesignerFile)]
public class EnumNodeGenerator : IClassTemplate<EnumNode>
{
    public string OutputPath
    {
        get { return Path2.Combine(Ctx.Data.Graph.Name, "Enums"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

    public void TemplateSetup()
    {
        Ctx.CurrentDecleration.IsEnum = true;
        Ctx.CurrentDecleration.BaseTypes.Clear();
        foreach (var item in Ctx.Data.Items)
        {
            this.Ctx.CurrentDecleration.Members.Add(new CodeMemberField(this.Ctx.CurrentDecleration.Name, item.Name));
        }
    }

    public TemplateContext<EnumNode> Ctx { get; set; }
}