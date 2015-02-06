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
        Ctx.AddCondition("AllowMultipleOutputs", _=> !_.Inheritable);
        //Ctx.AddIterator("CustomSelectorItems", _ => _.CustomSelectors);
        foreach (var item in Ctx.Data.IncludedInSections)
        {
            Ctx.CurrentDecleration.BaseTypes.Add(item.ReferenceClassName);
        }

    }
    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public override bool AllowMultipleInputs
    {
        get
        {
            Ctx._("return {0}", Ctx.Data.AllowMultipleInputs ? "true" : "false");
            return base.AllowMultipleInputs;
        }
    }
    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public override bool AllowMultipleOutputs
    {
        get
        {
            Ctx._("return {0}", Ctx.Data.AllowMultipleOutputs ? "true" : "false");
            return base.AllowMultipleOutputs;
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
            var attributes = Ctx.AddAttribute(typeof(ReferenceSection))
                .AddArgument(new CodePrimitiveExpression(Ctx.Item.Name))
                .AddArgument("SectionVisibility.{0}", referenceSection.Visibility.ToString())
                .AddArgument(new CodePrimitiveExpression(referenceSection.AllowDuplicates))
                .AddArgument(new CodePrimitiveExpression(referenceSection.IsAutomatic))
                .AddArgument(string.Format("typeof({0})", referenceSection.ReferenceClassName))
                .AddArgument(new CodePrimitiveExpression(referenceSection.IsEditable))
                .AddArgument("OrderIndex", new CodePrimitiveExpression(slot.Order))
                .AddArgument("HasPredefinedOptions", new CodePrimitiveExpression(referenceSection.HasPredefinedOptions))
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

    //[TemplateProperty("{0}", AutoFillType.NameOnly, Location = MemberGeneratorLocation.Both)]
    //public virtual IEnumerable<GenericReferenceItem> CustomSelectorItems
    //{
    //    get
    //    {
    //        Ctx.SetTypeArgument(Ctx.ItemAs<ShellPropertySelectorItem>().ReferenceClassName);
    //        Ctx._("yield break");
    //        return null;
    //    }
    //}

    [TemplateProperty("{0}InputSlot", AutoFillType.NameOnly)]
    public virtual GenericReferenceItem InputSlot
    {
        get
        {
            
            var item = Ctx.ItemAs<ShellNodeInputsSlot>();
            var field = Ctx.CurrentDecleration._private_(item.SourceItem.ClassName, "_" + item.Name);

            Ctx.SetType(item.SourceItem.ClassName);
            Ctx.AddAttribute(typeof (InputSlot))
                .AddArgument(new CodePrimitiveExpression(item.Name))
                .AddArgument(new CodePrimitiveExpression(item.SourceItem.AllowMultiple))
                .AddArgument("SectionVisibility.{0}", item.SourceItem.Visibility.ToString())
                .Arguments.Add(new CodeAttributeArgument("OrderIndex", new CodePrimitiveExpression(item.Order)))
                ;
            Ctx._if("{0} == null", field.Name)
                .TrueStatements._("{0} = new {1}() {{ Node = this }}",field.Name);
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

[TemplateClass("ViewModels", MemberGeneratorLocation.Both, ClassNameFormat = "{0}NodeViewModel", IsEditorExtension = true, AutoInherit = false)]
public class ShellNodeTypeViewModelTemplate : GenericNodeViewModel<GenericNode>, IClassTemplate<ShellNodeTypeNode>
{

    public void TemplateSetup()
    {
        if (Ctx.IsDesignerFile)
        {
            if (Ctx.Data.BaseNode != null)
            {
                Ctx.SetBaseType(Ctx.Data.BaseNode.Name + "NodeViewModel");
            }
            else
            {
                Ctx.SetBaseTypeArgument(Ctx.Data.ClassName);    
            }
            
        }
            
    }

    public TemplateContext<ShellNodeTypeNode> Ctx { get; set; }

    // For templating
    public ShellNodeTypeViewModelTemplate() : base()
    {
    }

    public ShellNodeTypeViewModelTemplate(GenericNode graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
    {
    }

    [TemplateConstructor(MemberGeneratorLocation.Both,"graphItemObject","diagramViewModel")]
    public void ViewModelConstructor(GenericNode graphItemObject, DiagramViewModel diagramViewModel)
    {
        Ctx.CurrentConstructor.Parameters[0].Type = Ctx.Data.ClassName.ToCodeReference();

    }

}
[TemplateClass("Drawers", MemberGeneratorLocation.Both, ClassNameFormat = "{0}NodeDrawer", IsEditorExtension = true)]
public class ShellNodeTypeDrawerTemplate : GenericNodeDrawer<GenericNode,GenericNodeViewModel<GenericNode>>, IClassTemplate<ShellNodeTypeNode>
{

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        //Ctx.SetBaseTypeArgument(Ctx.Data.ClassName);
        Ctx.SetBaseType("GenericNodeDrawer<{0},{1}>",Ctx.Data.ClassName, Ctx.Data.Name + "NodeViewModel");
    }

    public TemplateContext<ShellNodeTypeNode> Ctx { get; set; }

    // For templating
    public ShellNodeTypeDrawerTemplate()
        : base()
    {
    }

    public ShellNodeTypeDrawerTemplate(GenericNodeViewModel<GenericNode> viewModel) : base(viewModel)
    {
    }

    [TemplateConstructor(MemberGeneratorLocation.Both, "viewModel")]
    public void DrawerConstructor(GenericNodeViewModel<GenericNode> viewModel)
    {
        Ctx.CurrentConstructor.Parameters[0].Type = (Ctx.Data.Name + "NodeViewModel").ToCodeReference();

    }

}
