using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

[TemplateClass("Nodes",MemberGeneratorLocation.Both,ClassNameFormat = "{0}Node",IsEditorExtension = true)]
public class ShellNodeTypeTemplate : GenericNode, IClassTemplate<ShellNodeTypeNode>
{
    public TemplateContext<ShellNodeTypeNode> Ctx { get; set; }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.Data.Inheritable && Ctx.IsDesignerFile)
        {
            Ctx.CurrentDecleration.BaseTypes.Clear();
            Ctx.CurrentDecleration.BaseTypes.Add(typeof (GenericInheritableNode));
        }
        Ctx.AddIterator("PossibleReferenceItems", _ => _.Sections.Where(p => p.SourceItem is ShellNodeTypeReferenceSection));
        Ctx.AddIterator("ReferenceSectionItems", _ => _.Sections.Where(p => p.SourceItem is ShellNodeTypeReferenceSection));
        Ctx.AddIterator("SectionItems", _ => _.Sections.Where(p=>p.SourceItem is ShellSectionNode));
        Ctx.AddIterator("InputSlot", _ => _.InputSlots);
        Ctx.AddIterator("OutputSlot", _ => _.OutputSlots);
        Ctx.AddIterator("CustomSelectorItems", _ => _.CustomSelectors);
        foreach (var item in Ctx.Data.IncludedInSections)
        {
            Ctx.CurrentDecleration.BaseTypes.Add(item.ReferenceClassName);
        }

    }

    [TemplateProperty("Possible{0}",AutoFillType.NameOnly)]
    public virtual IEnumerable<GenericReferenceItem> PossibleReferenceItems
    {
        get
        {
            var referenceName = Ctx.ItemAs<ShellNodeSectionsSlot>().SourceItem.ReferenceClassName;
            Ctx.SetTypeArgument(referenceName);
            Ctx._("return this.Project.AllGraphItems.OfType<{0}>()",referenceName);
            //Ctx.AddAttribute(typeof (ReferenceSection), Ctx.Item.Name);
            return null;
        }
    }

    [TemplateProperty("{0}", AutoFillType.NameOnly)]
    public virtual IEnumerable<GenericReferenceItem> SectionItems
    {
        get
        {
            var item = Ctx.ItemAs<ShellNodeSectionsSlot>();
            var section = item.SourceItem as ShellSectionNode;
            Ctx.SetTypeArgument(section.ReferenceType.ClassName);

            Ctx.AddAttribute(typeof (Section))
                .AddArgument(new CodePrimitiveExpression(Ctx.Item.Name))
                .AddArgument("SectionVisibility.{0}", section.Visibility.ToString())
                .Arguments.Add(new CodeAttributeArgument("OrderIndex", new CodePrimitiveExpression(item.Order)))
                ;

            Ctx._("return ChildItems.OfType<{0}>()", section.ReferenceType.ClassName);
            return null;
        }
    }
    [TemplateProperty("{0}", AutoFillType.NameOnly)]
    public virtual IEnumerable<GenericReferenceItem> ReferenceSectionItems
    {
        get
        {
            var slot = Ctx.ItemAs<ShellNodeSectionsSlot>();
            var referenceSection = slot.SourceItem as ShellNodeTypeReferenceSection;
            Ctx.SetTypeArgument(referenceSection.ClassName);
            Ctx.AddAttribute(typeof(ReferenceSection))
                .AddArgument(new CodePrimitiveExpression(Ctx.Item.Name))
                .AddArgument("SectionVisibility.{0}", referenceSection.Visibility.ToString())
                .AddArgument(new CodePrimitiveExpression(referenceSection.AllowDuplicates))
                .AddArgument(new CodePrimitiveExpression(referenceSection.IsAutomatic))
                .AddArgument(string.Format("typeof({0})", referenceSection.ReferenceClassName))
                .AddArgument(new CodePrimitiveExpression(referenceSection.IsEditable))
                .Arguments.Add(new CodeAttributeArgument("OrderIndex",new CodePrimitiveExpression(slot.Order)))
                ;
                
            Ctx._("return ChildItems.OfType<{0}>()", referenceSection.ClassName);
            return null;
        }
    }


    public virtual IEnumerable<GenericReferenceItem> InputItems
    {
        get { return null; }
    }

    public virtual IEnumerable<GenericReferenceItem> OutputItems
    {
        get { return null; }
    }

    [TemplateProperty("{0}", AutoFillType.NameOnly, Location = MemberGeneratorLocation.Both)]
    public virtual IEnumerable<GenericReferenceItem> CustomSelectorItems
    {
        get
        {
            Ctx.SetTypeArgument(Ctx.ItemAs<ShellPropertySelectorItem>().ReferenceClassName);
            Ctx._("yield break");
            return null;
        }
    }

    [TemplateProperty("{0}InputSlot", AutoFillType.NameOnlyWithBackingField)]
    public virtual GenericReferenceItem InputSlot
    {
        get
        {
            var item = Ctx.ItemAs<ShellNodeInputsSlot>();
            Ctx.SetType(item.SourceItem.ClassName);
            Ctx.AddAttribute(typeof (InputSlot))
                .AddArgument(new CodePrimitiveExpression(item.Name))
                .AddArgument(new CodePrimitiveExpression(item.SourceItem.AllowMultiple))
                .AddArgument("SectionVisibility.{0}", item.SourceItem.Visibility.ToString())
                .Arguments.Add(new CodeAttributeArgument("OrderIndex", new CodePrimitiveExpression(item.Order)))
                ;

            return null;
        }
    }

    [TemplateProperty("{0}OutputSlot", AutoFillType.NameOnlyWithBackingField)]
    public virtual GenericReferenceItem OutputSlot
    {
        get
        {
            var item = Ctx.ItemAs<ShellNodeOutputsSlot>();
            Ctx.SetType(item.SourceItem.ClassName);
            Ctx.AddAttribute(typeof(OutputSlot))
                .AddArgument(new CodePrimitiveExpression(item.Name))
                .AddArgument(new CodePrimitiveExpression(item.SourceItem.AllowMultiple))
                .AddArgument("SectionVisibility.{0}", item.SourceItem.Visibility.ToString())
                .Arguments.Add(new CodeAttributeArgument("OrderIndex", new CodePrimitiveExpression(item.Order)))
                ;

            return null;
        }
    }




}

[TemplateClass("NodesViewModels", MemberGeneratorLocation.Both, ClassNameFormat = "{0}NodeViewModel", IsEditorExtension = true)]
public class ShellNodeViewModelTemplate : GenericNodeViewModel<GenericNode>, IClassTemplate<ShellNodeTypeNode>
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
    }

    public TemplateContext<ShellNodeTypeNode> Ctx { get; set; }

    
    public ShellNodeViewModelTemplate(GenericNode graphItemObject, DiagramViewModel diagramViewModel) : 
        base(graphItemObject, diagramViewModel)
    {

    }


}

[TemplateClass("Sections", MemberGeneratorLocation.Both, ClassNameFormat = "{0}Reference", IsEditorExtension = true)]
public class ShellReferenceSectionTemplate : GenericReferenceItem<IDiagramNodeItem>,
    IClassTemplate<ShellNodeTypeReferenceSection>
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.IsDesignerFile)
        {
            Ctx.SetBaseTypeArgument(Ctx.Data.ReferenceClassName);

            foreach (var item in Ctx.Data.IncludedInSections)
            {
                Ctx.AddInterface(item.ReferenceClassName);
            }
        }
    }

    public TemplateContext<ShellNodeTypeReferenceSection> Ctx { get; set; }
}

[TemplateClass("Sections", MemberGeneratorLocation.Both, ClassNameFormat = "{0}ChildItem", IsEditorExtension = true)]
public class ShellChildTemplate : GenericNodeChildItem,
    IClassTemplate<ShellChildItemTypeNode>
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.IsDesignerFile)
        {
            if (Ctx.Data["Typed"] && Ctx.Data.BaseNode == null)
                Ctx.SetBaseType(typeof(GenericTypedChildItem));

            foreach (var item in Ctx.Data.IncludedInSections)
            {
                Ctx.AddInterface(item.ReferenceClassName);
            }
        }
        
    }

    public TemplateContext<ShellChildItemTypeNode> Ctx { get; set; }
}

[TemplateClass("ChildItems", MemberGeneratorLocation.Both, ClassNameFormat = "{0}ChildItem", IsEditorExtension = true)]
public class ShellChildItemTemplate : GenericNodeChildItem,
    IClassTemplate<ShellNodeChildTypeNode>
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.IsDesignerFile)
        {
            if (Ctx.Data["Typed"])
                Ctx.SetBaseType(typeof(GenericTypedChildItem));

            foreach (var item in Ctx.Data.IncludedInSections)
            {
                Ctx.AddInterface(item.ReferenceClassName);
            }
        }

    }

    public TemplateContext<ShellNodeChildTypeNode> Ctx { get; set; }
}

[TemplateClass("Slots", MemberGeneratorLocation.Both, ClassNameFormat = "{0}", IsEditorExtension = true)]
public class ShellSlotItemTemplate : GenericSlot, IClassTemplate<ShellSlotTypeNode>
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        var i = new CodeTypeDeclaration("I" + Ctx.Data.Name)
        {
            IsInterface = true,
            Attributes = MemberAttributes.Public,
            IsPartial = true,
        };
        i.BaseTypes.Add(new CodeTypeReference(typeof(IDiagramNodeItem)));
        i.BaseTypes.Add(new CodeTypeReference(typeof(IConnectable)));
        Ctx.Namespace.Types.Add(i);

        if (Ctx.IsDesignerFile)
        {
            if (Ctx.Data.IsOutput)
            {
                if (Ctx.Data.AllowMultiple)
                {
                    Ctx.SetBaseType("MultiOutputSlot<{0}>", Ctx.Data.ReferenceClassName);
                }
                else
                {
                    Ctx.SetBaseType("SingleOutputSlot<{0}>", Ctx.Data.ReferenceClassName);
                }
            }
            else
            {
                if (Ctx.Data.AllowMultiple)
                {
                    Ctx.SetBaseType("MultiInputSlot<{0}>", Ctx.Data.ReferenceClassName);
                }
                else
                {
                    Ctx.SetBaseType("SingleInputSlot<{0}>", Ctx.Data.ReferenceClassName);
                }
            }

            foreach (var item in Ctx.Data.IncludedInSections)
            {
                Ctx.AddInterface(item.ReferenceClassName);
            }
        }
      

    }

    public TemplateContext<ShellSlotTypeNode> Ctx { get; set; }
}

[TemplateClass("Graphs", MemberGeneratorLocation.Both, ClassNameFormat = "{0}Graph", IsEditorExtension = true)]
public class ShellGraphTemplate : UnityGraphData<GenericGraphData<GenericNode>>,IClassTemplate<ShellGraphTypeNode>
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.IsDesignerFile)
        {
            Ctx.SetBaseTypeArgument("GenericGraphData<{0}>",Ctx.Data.RootNode.ClassName);
        }
    }

    public TemplateContext<ShellGraphTypeNode> Ctx { get; set; }
}


public class ShellContextCommandTemplate : EditorCommand<GenericNode>
{
    [TemplateMethod(MemberGeneratorLocation.Both,true)]
    public override void Perform(GenericNode node)
    {
        
    }

    [TemplateMethod(MemberGeneratorLocation.Both, true)]
    public override string CanPerform(GenericNode node)
    {
        return null;
    }
}

