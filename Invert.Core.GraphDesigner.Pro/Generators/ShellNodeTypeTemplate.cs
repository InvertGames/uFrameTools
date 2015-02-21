using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;

[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}Node")]
public class ShellNodeTypeTemplate : GenericNode, IClassTemplate<ShellNodeTypeNode>
{
    public TemplateContext<ShellNodeTypeNode> Ctx { get; set; }

    public string OutputPath
    {
        get { return Path2.Combine("Editor", "Nodes"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.Data.Inheritable && Ctx.IsDesignerFile)
        {
            Ctx.CurrentDecleration.BaseTypes.Clear();
            Ctx.CurrentDecleration.BaseTypes.Add(typeof(GenericInheritableNode));
        }
        Ctx.AddIterator("PossibleReferenceItems", _ => _.Sections.Where(p => p.SourceItem is ShellNodeTypeReferenceSection));
        Ctx.AddIterator("ReferenceSectionItems", _ => _.Sections.Where(p => p.SourceItem is ShellNodeTypeReferenceSection));
        Ctx.AddIterator("SectionItems", _ => _.Sections.Where(p => p.SourceItem is ShellSectionNode));
        Ctx.AddIterator("InputSlot", _ => _.InputSlots);
        Ctx.AddIterator("OutputSlot", _ => _.OutputSlots);
        Ctx.AddCondition("AllowMultipleOutputs", _ => !_.Inheritable);
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

    [TemplateProperty("Possible{0}", AutoFillType.NameOnly)]
    public virtual IEnumerable<GenericReferenceItem> PossibleReferenceItems
    {
        get
        {
            var referenceName = Ctx.ItemAs<ShellNodeSectionsSlot>().SourceItem.ReferenceClassName;
            Ctx.SetTypeArgument(referenceName);
            Ctx._("return this.Project.AllGraphItems.OfType<{0}>()", referenceName);
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

            Ctx.AddAttribute(typeof(Section))
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
            Ctx.AddAttribute(typeof(InputSlot))
                .AddArgument(new CodePrimitiveExpression(item.Name))
                .AddArgument(new CodePrimitiveExpression(item.SourceItem.AllowMultiple))
                .AddArgument("SectionVisibility.{0}", item.SourceItem.Visibility.ToString())
                .Arguments.Add(new CodeAttributeArgument("OrderIndex", new CodePrimitiveExpression(item.Order)))
                ;
            Ctx._if("{0} == null", field.Name)
                .TrueStatements._("{0} = new {1}() {{ Node = this }}", field.Name);
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

[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}NodeViewModel", AutoInherit = false)]
public class ShellNodeTypeViewModelTemplate : GenericNodeViewModel<GenericNode>, IClassTemplate<ShellNodeTypeNode>
{
    public string OutputPath
    {
        get { return Path2.Combine("Editor", "ViewModels"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

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
    public ShellNodeTypeViewModelTemplate()
        : base()
    {
    }

    public ShellNodeTypeViewModelTemplate(GenericNode graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {
    }

    [TemplateConstructor(MemberGeneratorLocation.Both, "graphItemObject", "diagramViewModel")]
    public void ViewModelConstructor(GenericNode graphItemObject, DiagramViewModel diagramViewModel)
    {
        Ctx.CurrentConstructor.Parameters[0].Type = Ctx.Data.ClassName.ToCodeReference();

    }

}
[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}NodeDrawer")]
public class ShellNodeTypeDrawerTemplate : GenericNodeDrawer<GenericNode, GenericNodeViewModel<GenericNode>>, IClassTemplate<ShellNodeTypeNode>
{
    public string OutputPath
    {
        get { return Path2.Combine("Editor", "Drawers"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        //Ctx.SetBaseTypeArgument(Ctx.Data.ClassName);
        Ctx.SetBaseType("GenericNodeDrawer<{0},{1}>", Ctx.Data.ClassName, Ctx.Data.Name + "NodeViewModel");
    }

    public TemplateContext<ShellNodeTypeNode> Ctx { get; set; }

    // For templating
    public ShellNodeTypeDrawerTemplate()
        : base()
    {
    }

    public ShellNodeTypeDrawerTemplate(GenericNodeViewModel<GenericNode> viewModel)
        : base(viewModel)
    {
    }

    [TemplateConstructor(MemberGeneratorLocation.Both, "viewModel")]
    public void DrawerConstructor(GenericNodeViewModel<GenericNode> viewModel)
    {
        Ctx.CurrentConstructor.Parameters[0].Type = (Ctx.Data.Name + "NodeViewModel").ToCodeReference();

    }

}

[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}NodeViewModel", AutoInherit = false)]
public class ShellNodeConfigViewModelTemplate : GenericNodeViewModel<GenericNode>, IClassTemplate<ShellNodeConfig>
{
    public string OutputPath
    {
        get { return Path2.Combine("Editor", "ViewModels"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

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

    public TemplateContext<ShellNodeConfig> Ctx { get; set; }

    // For templating
    public ShellNodeConfigViewModelTemplate()
        : base()
    {
    }

    public ShellNodeConfigViewModelTemplate(GenericNode graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {
    }

    [TemplateConstructor(MemberGeneratorLocation.Both, "graphItemObject", "diagramViewModel")]
    public void ViewModelConstructor(GenericNode graphItemObject, DiagramViewModel diagramViewModel)
    {
        Ctx.CurrentConstructor.Parameters[0].Type = Ctx.Data.ClassName.ToCodeReference();

    }

}
[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}NodeDrawer")]
public class ShellNodeConfigDrawerTemplate : GenericNodeDrawer<GenericNode, GenericNodeViewModel<GenericNode>>, IClassTemplate<ShellNodeConfig>
{
    public string OutputPath
    {
        get { return Path2.Combine("Editor", "Drawers"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        //Ctx.SetBaseTypeArgument(Ctx.Data.ClassName);
        Ctx.SetBaseType("GenericNodeDrawer<{0},{1}>", Ctx.Data.ClassName, Ctx.Data.Name + "NodeViewModel");
    }

    public TemplateContext<ShellNodeConfig> Ctx { get; set; }

    // For templating
    public ShellNodeConfigDrawerTemplate()
        : base()
    {
    }

    public ShellNodeConfigDrawerTemplate(GenericNodeViewModel<GenericNode> viewModel)
        : base(viewModel)
    {
    }

    [TemplateConstructor(MemberGeneratorLocation.Both, "viewModel")]
    public void DrawerConstructor(GenericNodeViewModel<GenericNode> viewModel)
    {
        Ctx.CurrentConstructor.Parameters[0].Type = (Ctx.Data.Name + "NodeViewModel").ToCodeReference();

    }

}

[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}Node")]
public class ShellNodeConfigTemplate : GenericNode, IClassTemplate<ShellNodeConfig>
{
    public TemplateContext<ShellNodeConfig> Ctx { get; set; }

    public string OutputPath
    {
        get { return Path2.Combine("Editor", "Nodes"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        var i = new CodeTypeDeclaration(Ctx.Data.ReferenceClassName)
        {
            IsInterface = true,
            Attributes = MemberAttributes.Public,
            IsPartial = true,
        };
        i.BaseTypes.Add(new CodeTypeReference(typeof(IDiagramNodeItem)));
        i.BaseTypes.Add(new CodeTypeReference(typeof(IConnectable)));
        Ctx.Namespace.Types.Add(i);
        if (Ctx.Data.Inheritable && Ctx.IsDesignerFile)
        {
            Ctx.CurrentDecleration.BaseTypes.Clear();
            Ctx.CurrentDecleration.BaseTypes.Add(typeof(GenericInheritableNode));
        }
        if (Ctx.IsDesignerFile && Ctx.Data.IsClass)
        {
            Ctx.CurrentDecleration.BaseTypes.Add(typeof(IClassTypeNode));
        }

        Ctx.AddIterator("PossibleReferenceItems", _ => _.Sections.Where(p => p.SectionType == ShellNodeConfigSectionType.ReferenceItems));
        Ctx.AddIterator("ReferenceSectionItems", _ => _.Sections.Where(p => p.SectionType == ShellNodeConfigSectionType.ReferenceItems));
        Ctx.AddIterator("SectionItems", _ => _.Sections.Where(p => p.SectionType == ShellNodeConfigSectionType.ChildItems));
        Ctx.AddIterator("InputSlot", _ => _.InputSlots);
        Ctx.AddIterator("OutputSlot", _ => _.OutputSlots);
        Ctx.AddCondition("AllowMultipleOutputs", _ => !_.Inheritable);
        Ctx.AddCondition("ClassName", _ => Ctx.Data.IsClass);
        //Ctx.AddIterator("CustomSelectorItems", _ => _.CustomSelectors);
        foreach (var item in Ctx.Data.IncludedInSections)
        {
            Ctx.CurrentDecleration.BaseTypes.Add(item.ReferenceClassName);
        }

    }
    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public virtual string ClassName
    {
        get
        {
            Ctx._("return this.Name");
            return null;
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

    [TemplateProperty("Possible{0}", AutoFillType.NameOnly)]
    public virtual IEnumerable<GenericReferenceItem> PossibleReferenceItems
    {
        get
        {
            var referenceName = Ctx.ItemAs<ShellNodeConfigSection>().ReferenceClassName;
            Ctx.SetTypeArgument(referenceName);
            Ctx._("return this.Project.AllGraphItems.OfType<{0}>()", referenceName);
            //Ctx.AddAttribute(typeof (ReferenceSection), Ctx.Item.Name);
            return null;
        }
    }

    [TemplateProperty("{0}", AutoFillType.NameOnly)]
    public virtual IEnumerable<GenericReferenceItem> SectionItems
    {
        get
        {
            var item = Ctx.ItemAs<ShellNodeConfigSection>();

            Ctx.SetTypeArgument(item.ClassName);

            Ctx.AddAttribute(typeof(Section))
                .AddArgument(new CodePrimitiveExpression(Ctx.Item.Name))
                .AddArgument("SectionVisibility.{0}", item.Visibility.ToString())
                .Arguments.Add(new CodeAttributeArgument("OrderIndex", new CodePrimitiveExpression(item.Row)))
                ;

            Ctx._("return ChildItems.OfType<{0}>()", item.ClassName);
            return null;
        }
    }

    [TemplateProperty("{0}", AutoFillType.NameOnly)]
    public virtual IEnumerable<GenericReferenceItem> ReferenceSectionItems
    {
        get
        {
            var referenceSection = Ctx.ItemAs<ShellNodeConfigSection>();

            Ctx.SetTypeArgument(referenceSection.ClassName);
            var attributes = Ctx.AddAttribute(typeof(ReferenceSection))
                .AddArgument(new CodePrimitiveExpression(Ctx.Item.Name))
                .AddArgument("SectionVisibility.{0}", referenceSection.Visibility.ToString())
                .AddArgument(new CodePrimitiveExpression(referenceSection.AllowDuplicates))
                .AddArgument(new CodePrimitiveExpression(referenceSection.IsAutomatic))
                .AddArgument(string.Format("typeof({0})", referenceSection.ReferenceClassName))
                .AddArgument(new CodePrimitiveExpression(referenceSection.IsEditable))
                .AddArgument("OrderIndex", new CodePrimitiveExpression(referenceSection.Row))
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

    [TemplateProperty("{0}InputSlot", AutoFillType.NameOnly)]
    public virtual GenericReferenceItem InputSlot
    {
        get
        {

            var item = Ctx.ItemAs<ShellNodeConfigInput>();
            var field = Ctx.CurrentDecleration._private_(item.ClassName, "_" + item.Name);

            Ctx.SetType(item.ClassName);
            Ctx.AddAttribute(typeof(InputSlot))
                .AddArgument(new CodePrimitiveExpression(item.Name))
                .AddArgument(new CodePrimitiveExpression(item.AllowMultiple))
                .AddArgument("SectionVisibility.{0}", item.Visibility.ToString())
                .Arguments.Add(new CodeAttributeArgument("OrderIndex", new CodePrimitiveExpression(item.Row)))
                ;
            Ctx._if("{0} == null", field.Name)
                .TrueStatements._("{0} = new {1}() {{ Node = this }}", field.Name, item.ClassName);
            Ctx._("return {0}", field.Name);
            return null;
        }
        set { Ctx._("_{0} = value", Ctx.Item.Name); }

    }

    [TemplateProperty("{0}OutputSlot", AutoFillType.NameOnly)]
    public virtual GenericReferenceItem OutputSlot
    {
        get
        {
            var item = Ctx.ItemAs<ShellNodeConfigOutput>();
            var field = Ctx.CurrentDecleration._private_(item.ClassName, "_" + item.Name);
            Ctx.SetType(item.ClassName);
            Ctx.AddAttribute(typeof(OutputSlot))
                .AddArgument(new CodePrimitiveExpression(item.Name))
                .AddArgument(new CodePrimitiveExpression(item.AllowMultiple))
                .AddArgument("SectionVisibility.{0}", item.Visibility.ToString())
                .Arguments.Add(new CodeAttributeArgument("OrderIndex", new CodePrimitiveExpression(item.Row)))
                ;
            Ctx._if("{0} == null", field.Name)
           .TrueStatements._("{0} = new {1}() {{ Node = this }}", field.Name, item.ClassName);

            Ctx._("return {0}", field.Name);
            return null;
        }
        set { Ctx._("_{0} = value", Ctx.Item.Name); }
    }

}

[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}Reference")]
public class ShellNodeConfigReferenceSectionTemplate : GenericReferenceItem<IDiagramNodeItem>,
    IClassTemplate<ShellNodeConfigSection>
{
    public string OutputPath
    {
        get { return Path2.Combine("Editor", "Sections"); }
    }

    public bool CanGenerate
    {
        get { return Ctx.Data.SectionType == ShellNodeConfigSectionType.ReferenceItems; }
    }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        var i = new CodeTypeDeclaration(Ctx.Data.ReferenceClassName)
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
            Ctx.SetBaseTypeArgument(Ctx.Data.ReferenceClassName);

            foreach (var item in Ctx.Data.IncludedInSections)
            {
                Ctx.AddInterface(item.ReferenceClassName);
            }
        }
    }

    public TemplateContext<ShellNodeConfigSection> Ctx { get; set; }

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
}
[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}ChildItem")]
public class ShellNodeConfigChildItemTemplate : GenericNodeChildItem,
    IClassTemplate<ShellNodeConfigSection>
{
    public string OutputPath
    {
        get { return Path2.Combine("Editor", "ChildItems"); }
    }

    public bool CanGenerate
    {
        get { return Ctx.Data.SectionType == ShellNodeConfigSectionType.ChildItems; }
    }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        var i = new CodeTypeDeclaration(Ctx.Data.ReferenceClassName)
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
            if (Ctx.Data.IsTyped)
                Ctx.SetBaseType(typeof(GenericTypedChildItem));

            foreach (var item in Ctx.Data.IncludedInSections)
            {
                Ctx.AddInterface(item.ReferenceClassName);
            }
        }

    }

    public TemplateContext<ShellNodeConfigSection> Ctx { get; set; }

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
}


[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}Graph")]
public class ShellNodeAsGraphTemplate : GenericGraphData<GenericNode>, IClassTemplate<ShellNodeConfig>
{
    public string OutputPath
    {
        get { return Path2.Combine("Editor", "Graphs"); }
    }

    public bool CanGenerate
    {
        get { return Ctx.Data.IsGraphType; }
    }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.IsDesignerFile)
        {
            Ctx.SetBaseType("GenericGraphData<{0}>", Ctx.Data.ClassName);
        }
    }

    public TemplateContext<ShellNodeConfig> Ctx { get; set; }
}

[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}")]
public class ShellConfigPluginTemplate : DiagramPlugin, IClassTemplate<ShellPluginNode>
{
    #region Template Setup

    public string OutputPath
    {
        get { return Path2.Combine("Editor", "Plugins"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

    public void TemplateSetup()
    {
        Ctx.AddIterator("NodeConfigProperty", _ => _.Graph.NodeItems.OfType<ShellNodeConfig>());
        Ctx.AddIterator("GetSelectionCommand", _ => _.Graph.AllGraphItems.OfType<ShellNodeConfigSection>().Where(x => x.IsTyped && x.SectionType == ShellNodeConfigSectionType.ChildItems));
        Ctx.TryAddNamespace("Invert.Core");
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
    }

    [TemplateMethod("Get{0}SelectionCommand", MemberGeneratorLocation.Both, true)]
    public virtual Invert.Core.GraphDesigner.SelectItemTypeCommand GetSelectionCommand()
    {
        Ctx._("return new SelectItemTypeCommand() {{ IncludePrimitives = true, AllowNone = false }}");
        return null;
    }


    public TemplateContext<ShellPluginNode> Ctx { get; set; }
    #endregion

    public override bool Ignore
    {
        get
        {
            return true;
        }
    }

    [TemplateProperty("{0}", AutoFillType.NameAndTypeWithBackingField)]
    public NodeConfig<GenericNode> NodeConfigProperty
    {
        get
        {
            var item = Ctx.ItemAs<IClassTypeNode>().ClassName;
            Ctx.SetTypeArgument(item);
            return null;
        }
        set
        {

        }
    }

    [TemplateMethod(MemberGeneratorLocation.Both, true)]
    public override void Initialize(uFrameContainer container)
    {
        if (!Ctx.IsDesignerFile) return;
        Ctx.CurrentMethodAttribute.CallBase = false;
        var method = Ctx.CurrentMethod;
        //foreach (var item in Ctx.Data.Graph.NodeItems.OfType<ShellChildItemTypeNode>())
        //{
        //    if (!item["Typed"]) continue;


        //}
        foreach (var itemType in Ctx.Data.Project.AllGraphItems.OfType<ShellNodeConfigSection>().Where(p => p.IsValid && p.SectionType == ShellNodeConfigSectionType.ChildItems || p.SectionType == ShellNodeConfigSectionType.ReferenceItems))
        {

            if (itemType.IsTyped && itemType.SectionType == ShellNodeConfigSectionType.ChildItems)
            {
                method._(
               "container.RegisterInstance<IEditorCommand>(Get{0}SelectionCommand(), typeof({1}).Name + \"TypeSelection\");", itemType.Name, itemType.ClassName);

                //if (itemType.IsCustom)
                //{
                //    method.Statements.Add(
                //        new CodeSnippetExpression(string.Format("container.AddTypeItem<{0},{1}ViewModel,{1}Drawer>()", itemType.ClassName, itemType.Name)));
                //}
                //else
                //{
                method.Statements.Add(
                    new CodeSnippetExpression(string.Format("container.AddTypeItem<{0}>()", itemType.ClassName)));
                //}

            }
            else
            {
                //if (itemType.Flags.ContainsKey("Custom") && itemType.Flags["Custom"])
                //{
                //    method.Statements.Add(
                //    new CodeSnippetExpression(string.Format("container.AddItem<{0}, {1}ViewModel, {1}Drawer>()", itemType.ClassName, itemType.Name)));
                //}
                //else
                //{
                method.Statements.Add(
                new CodeSnippetExpression(string.Format("container.AddItem<{0}>()", itemType.ClassName)));
                //}
            }
        }

        //var graphTypes = Ctx.Data.Graph.NodeItems.OfType<ShellNodeConfig>().Where(p => p.IsValid && p.IsGraphType).ToArray();
        foreach (var nodeType in Ctx.Data.Project.NodeItems.OfType<ShellNodeConfig>().Where(p => p.IsValid))
        {
            InitializeNodeType(method, nodeType);

        }

        foreach (var item in Ctx.Data.Project.AllGraphItems.OfType<IShellNodeConfigItem>())
        {
            var connectableTo = item.OutputsTo<IShellNodeConfigItem>();
            foreach (var c in connectableTo)
            {
                method._("container.Connectable<{0},{1}>()", item.ClassName, c.ClassName);
            }
        }


        foreach (var item in Ctx.Data.Project.AllGraphItems.OfType<IShellNodeConfigItem>())
        {
            foreach (var template in item.OutputsTo<ShellTemplateConfigNode>())
            {
                method.Statements.Add(new CodeSnippetExpression(string.Format("RegisteredTemplateGeneratorsFactory.RegisterTemplate<{0},{1}>()", item.ClassName, template.Name)));
            }
        }
        //foreach (var nodeType in Ctx.Data.Graph.NodeItems.OfType<IShellConnectable>().Where(p => p.IsValid))
        //{
        //    foreach (var item in nodeType.ConnectableTo)
        //    {
        //        method._("container.Connectable<{0},{1}>()", nodeType.ClassName, item.SourceItem.ClassName);
        //    }

        //}
        //foreach (var nodeType in Ctx.Data.Graph.NodeItems.OfType<IReferenceNode>().Where(p => p.IsValid))
        //{


        //    if (nodeType.Flags.ContainsKey("Output") && nodeType.Flags["Output"])
        //    {
        //        method._("container.Connectable<{0},{1}>()", nodeType.ClassName, nodeType.ReferenceClassName);
        //    }
        //    else
        //    {
        //        method._("container.Connectable<{0},{1}>()", nodeType.ReferenceClassName, nodeType.ClassName);
        //    }
        //}
    }

    private static void InitializeNodeType(CodeMemberMethod method, ShellNodeConfig nodeType)
    {
        var varName = nodeType.Name;

        if (nodeType.IsGraphType)
        {
            method.Statements.Add(
                new CodeSnippetExpression(string.Format("{1} = container.AddGraph<{0}, {2}>(\"{0}\")",
                    nodeType.Name + "Graph", varName, nodeType.ClassName)));
        }
        else
        {

            method.Statements.Add(
           new CodeSnippetExpression(string.Format("{1} = container.AddNode<{0},{0}ViewModel,{0}Drawer>(\"{0}\")", nodeType.ClassName, varName)));

        }



        if (nodeType.Inheritable)
        {
            method.Statements.Add(new CodeSnippetExpression(string.Format("{0}.Inheritable()", varName)));
        }

        method.Statements.Add(
            new CodeSnippetExpression(string.Format("{0}.Color(NodeColor.{1})", varName, nodeType.Color.ToString())));



        foreach (var item in nodeType.SubNodes)
        {
            method.Statements.Add(
                new CodeSnippetExpression(string.Format("{0}.HasSubNode<{1}Node>()", varName, item.Name)));
        }

    }

}

[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}")]
public class ShellNodeConfigTemplateTemplate : IClassTemplate<ShellTemplateConfigNode>
{

    public string OutputPath
    {
        get { return Path2.Combine("Editor", "Templates"); }
    }

    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public virtual string OutputPathProperty
    {
        get
        {
            Ctx.CurrentProperty.Name = "OutputPath";
            Ctx._("return \"{0}\"", Ctx.Data.OutputPath);
            return null;
        }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public virtual bool CanGenerateProperty
    {
        get
        {
            Ctx.CurrentProperty.Name = "CanGenerate";
            Ctx._("return true");
            return true;
        }
    }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        //Ctx.CurrentDecleration.IsPartial = true;
        //Ctx.CurrentDecleration.Name = Ctx.Data.Name;
        if (Ctx.IsDesignerFile)
        {
            Ctx.CurrentDecleration.BaseTypes.Clear();
            Ctx.CurrentDecleration.BaseTypes.Add(string.Format("IClassTemplate<{0}>", Ctx.Data.NodeConfig.ClassName));

            Ctx.CurrentDecleration.CustomAttributes.Add(new CodeAttributeDeclaration(
                new CodeTypeReference(typeof(TemplateClass)),
                //new CodeAttributeArgument("OutputPath", new CodePrimitiveExpression(Ctx.Data.OutputPath)),
                new CodeAttributeArgument("Location", new CodeSnippetExpression(string.Format("MemberGeneratorLocation.{0}", Ctx.Data.Files))),
                new CodeAttributeArgument("AutoInherit", new CodePrimitiveExpression(Ctx.Data.AutoInherit)),
                new CodeAttributeArgument("ClassNameFormat", new CodePrimitiveExpression(Ctx.Data.ClassNameFormat))
                ));
        }
    }

    [TemplateMethod(MemberGeneratorLocation.Both)]
    public virtual void TemplateSetupMethod()
    {
        Ctx.CurrentMethod.Name = "TemplateSetup";
        if (Ctx.IsDesignerFile)
        {
            Ctx.PushStatements(Ctx._if("Ctx.IsDesignerFile").TrueStatements);
            Ctx._("Ctx.CurrentDecleration.BaseTypes.Clear()");
            if (!string.IsNullOrEmpty(Ctx.Data.TemplateBaseClass))
            {
                Ctx._("Ctx.CurrentDecleration.BaseTypes.Add(new CodeTypeReference(\"{0}\"))", Ctx.Data.TemplateBaseClass);
            }
            Ctx.PopStatements();
        }

    }

    public TemplateContext<ShellTemplateConfigNode> Ctx { get; set; }

    [TemplateProperty(MemberGeneratorLocation.DesignerFile, AutoFillType.NameAndTypeWithBackingField)]
    public TemplateContext<GenericNode> CtxProperty
    {
        get
        {
            Ctx.CurrentProperty.Name = "Ctx";
            Ctx.SetTypeArgument(Ctx.Data.NodeConfig.ClassName);
            return null;
        }
    }
}