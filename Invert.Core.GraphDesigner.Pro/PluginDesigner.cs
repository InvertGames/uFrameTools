using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Invert.Core;
using Invert.Core.GraphDesigner;
using UnityEditor;

public class ShellPropertySelectorItem : GenericTypedChildItem, IShellNodeItem
{
    public IShellNode SelectorFor
    {
        get { return this.RelatedNode() as IShellNode; }
    }

    public string ReferenceClassName
    {
        get
        {
            if (SelectorFor == null) return null;
            return SelectorFor.ClassName;
        }
    }
}

public class PluginDesigner : DiagramPlugin
{
    public override void Initialize(uFrameContainer container)
    {
#if DEBUG && UNITY_DLL
        //container.RegisterInstance<IToolbarCommand>(new PrintPlugins(), "Json");
      
#endif
        container.RegisterInstance<IDiagramNodeCommand>(new SelectColorCommand(), "SelectColor");
        var pluginConfig = container
            .AddItem<ShellNodeSectionsSlot>()
            .AddItem<ShellNodeInputsSlot>()
            .AddItem<ShellNodeOutputsSlot>()
            .AddItem<TemplatePropertyReference>()
            .AddItem<TemplateMethodReference>()
            .AddItem<TemplateFieldReference>()
            .AddItem<TemplateEventReference>()
            .AddItem<ShellAcceptableReferenceType>()
            .AddItem<ShellConnectableReferenceType>()
            .AddTypeItem<ShellPropertySelectorItem>()
            .AddGraph<PluginGraphData,ShellPluginNode>("Shell Plugin")
            .Color(NodeColor.Green)
           
            .HasSubNode<IShellNode>()
            .HasSubNode<TypeReferenceNode>()
            .HasSubNode<ShellNodeConfig>()
            .HasSubNode<ScreenshotNode>()
            .AddCodeTemplate<DocumentationTemplate>()
            ;
        container.AddNode<ScreenshotNode, ScreenshotNodeViewModel, ScreenshotNodeDrawer>("Screenshot");

        var shellConfigurationNode =
            container.AddNode<ShellNodeConfig, ShellNodeConfigViewModel, ShellNodeConfigDrawer>("Node Config")
                .HasSubNode<ShellNodeConfig>()
                .HasSubNode<ScreenshotNode>()
                .HasSubNode<ShellTemplateConfigNode>()
            ;
        shellConfigurationNode.AddFlag("Graph Type");

        container.AddNode<ShellTemplateConfigNode>("Code Template")
            .Color(NodeColor.Purple);
        
        
        RegisteredTemplateGeneratorsFactory.RegisterTemplate<ShellNodeConfig, ShellNodeConfigTemplate>();
        RegisteredTemplateGeneratorsFactory.RegisterTemplate<ShellNodeConfigSection, ShellNodeConfigReferenceSectionTemplate>();
        RegisteredTemplateGeneratorsFactory.RegisterTemplate<ShellNodeConfigSection, ShellNodeConfigChildItemTemplate>();
        RegisteredTemplateGeneratorsFactory.RegisterTemplate<ShellNodeConfig, ShellNodeAsGraphTemplate>();
        RegisteredTemplateGeneratorsFactory.RegisterTemplate<ShellPluginNode, ShellConfigPluginTemplate>();
        RegisteredTemplateGeneratorsFactory.RegisterTemplate<ShellNodeConfig, ShellNodeConfigViewModelTemplate>();
        RegisteredTemplateGeneratorsFactory.RegisterTemplate<ShellNodeConfig, ShellNodeConfigDrawerTemplate>();
        RegisteredTemplateGeneratorsFactory.RegisterTemplate<ShellTemplateConfigNode, ShellNodeConfigTemplateTemplate>();
        RegisteredTemplateGeneratorsFactory.RegisterTemplate<IShellSlotType, ShellSlotItemTemplate>();
        if (GenerateDocumentation)
        {
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<ShellPluginNode, DocumentationTemplate>();
            RegisteredTemplateGeneratorsFactory.RegisterTemplate<IDocumentable, DocumentationPageTemplate>();
        }
        

        container.Connectable<ShellNodeConfigSection, ShellNodeConfig>();
        container.Connectable<ShellNodeConfigSection, ShellNodeConfigSection>();
        container.Connectable<IShellNodeConfigItem, IShellNodeConfigItem>();
        container.Connectable<ShellNodeConfigOutput, ShellNodeConfigInput>();
        container.Connectable<ShellNodeConfigOutput, ShellNodeConfig>();
        container.Connectable<ShellNodeConfigOutput, ShellNodeConfigSection>();
        container.Connectable<ShellNodeConfig, ShellNodeConfigInput>();
        container.Connectable<ShellNodeConfig, ShellNodeConfigSection>();
        container.Connectable<IShellNodeConfigItem, ShellTemplateConfigNode>();

        container.Connectable<ShellNodeConfigSection, ShellNodeConfigInput>();
       
        container.Connectable<ShellNodeConfigSection, ShellNodeConfigSection>();
    }

    [InspectorProperty]
    public static bool GenerateDocumentation
    {
        get { return InvertGraphEditor.Prefs.GetBool("PLUGINDESIGNER_GENDOCS", true); }
        set
        {
            InvertGraphEditor.Prefs.SetBool("PLUGINDESIGNER_GENDOCS", value);
        }
    }
     


}

[TemplateClass(MemberGeneratorLocation.Both, "{0}DocumentationProvider")]
public class DocumentationTemplate : DocumentationDefaultProvider, IClassTemplate<ShellPluginNode>
{
    [TemplateMethod(MemberGeneratorLocation.Both)]
    public override void GetPages(List<DocumentationPage> rootPages)
    {
        //if (Ctx.IsDesignerFile)
        //foreach (var item in Ctx.Data.Graph.Project.NodeItems.OfType<ShellNodeConfig>())
        //{
        //    //Ctx._("DocumentationPage {0}Page = null;",item.Name.Clean());
        //    Ctx._("rootPages.Add({0}Page)",item.Name.Clean());

        

        //}
  
    }
    //[TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    //public object DocumentNodeProperty
    //{
    //    get
    //    {
    //        this.Ctx.CurrentProperty.Name = Ctx.Item.Name.Clean() + "Page";
    //        var field = Ctx.CurrentDecleration._private_(typeof(DocumentationPage),
    //            "_" + this.Ctx.CurrentProperty.Name);

    //        var item = Ctx.ItemAs<ShellNodeConfig>();

    //        Ctx.PushStatements(Ctx._if("{0} == null", field.Name)
    //            .TrueStatements);

    //        Ctx._("{0} = new DocumentationPage(\"{1}\", Document{1}Node, typeof({2})))", field.Name, item.Name, item.ClassName);
    //        foreach (var nodeItem in item.ChildItems.OfType<ShellNodeConfigItem>())
    //        {
    //            Ctx._("{0}.ChildItems.Add({1})", field.Name, nodeItem.Node.Name + nodeItem.Name.Clean() + "Page");
    //        }

    //        Ctx.PopStatements();
    //        Ctx._("return {0}", field.Name);
    //        return null;
    //    }
    //}

    //[TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    //public object DocumentItemProperty {
    //    get
    //    {
    //        this.Ctx.CurrentProperty.Name = Ctx.Item.Node.Name + Ctx.Item.Name.Clean() + "Page";
    //        var field = Ctx.CurrentDecleration._private_(typeof (DocumentationPage),
    //            "_" + this.Ctx.CurrentProperty.Name);

    //        var item = Ctx.ItemAs<IShellNodeConfigItem>();

    //        Ctx.PushStatements(Ctx._if("{0} == null",field.Name)
    //            .TrueStatements);
    //        Ctx._("{0} = new DocumentationPage(\"{3}\", Document{0}Node_{2}, typeof({1})))", field.Name, item.ClassName, item.TypeName.Clean(), item.Name);
    //        Ctx.PopStatements();
    //        Ctx._("return {0}", field.Name);
    //        return null;
    //    } }


   
    //[TemplateMethod(MemberGeneratorLocation.Both, 
    //    AutoFill = AutoFillType.NameOnly, 
    //    NameFormat = "Document{0}Node")]
    //public virtual void DocumentNode(IDocumentationBuilder builder)
    //{
        
    //    if (Ctx.IsDesignerFile)
    //    {
    //        Ctx._("builder.Title(\"{0}\")", Ctx.Item.Name);
    //        //Ctx._("builder.Paragraph(\"{0}\")", Ctx.ItemAs<DiagramNode>().Comments);
    //    }
    //}

    //[TemplateMethod(MemberGeneratorLocation.Both,
    //    AutoFill = AutoFillType.NameOnly,
    //    NameFormat = "Document{0}")]
    //public virtual void DocumentItem(IDocumentationBuilder builder)
    //{
    //    this.Ctx.CurrentMethod.Name = "Document" + Ctx.Item.Node.Name + "Node_" + Ctx.ItemAs<IShellNodeConfigItem>().TypeName.Clean();
    //    if (Ctx.IsDesignerFile)
    //    {
    //        Ctx._("builder.Title(\"{0}\")", Ctx.Item.Name);
    //    }
    //}

    public string OutputPath
    {
        get { return Path2.Combine("Editor", "Documentation"); }
    }
    public bool CanGenerate { get { return true; } }
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        //Ctx.AddIterator("DocumentNodeProperty", _ => _.Graph.Project.NodeItems.OfType<ShellNodeConfig>().Distinct());
        //Ctx.AddIterator("DocumentItemProperty", _ => _.Graph.Project.AllGraphItems.OfType<IShellNodeConfigItem>().Distinct());

        //Ctx.AddIterator("DocumentNode", _ => _.Graph.Project.NodeItems.OfType<ShellNodeConfig>().Distinct());
        //Ctx.AddIterator("DocumentItem", _ => _.Graph.Project.AllGraphItems.OfType<IShellNodeConfigItem>().Distinct());
    }

    public TemplateContext<ShellPluginNode> Ctx { get; set; }

}

[TemplateClass(MemberGeneratorLocation.Both,"{0}Page")]
public class DocumentationPageTemplate : DocumentationPage, IClassTemplate<IDocumentable>
{
    public string OutputPath
    {
        get { return Path2.Combine("Documentation","Editor", "Pages"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }
    
    public void TemplateSetup()
    {

        var className = Ctx.Data.Node.Name + "Page";
        if (Ctx.Data.Node != Ctx.Data)
        {
            className = Ctx.Data.Node.Name.Clean() + Ctx.Data.Name.Clean() + "Page";
        }
        if (Ctx.IsDesignerFile)
        {
            className += "Base";
        }
        Ctx.CurrentDecleration.Name = className;


        if (Ctx.IsDesignerFile)
        {
            if (this.Ctx.Data.Node.Graph.RootFilter != Ctx.Data)
            {
                Ctx.SetBaseType(this.Ctx.Data.Node.Graph.RootFilter.Name.Clean() + "Page");
            }
         
        }
        else
        {
            Ctx.SetBaseType(className + "Base");
        }

        if (Ctx.IsDesignerFile || this.Ctx.Data.Node.Graph.RootFilter == Ctx.Data)
        {
            Ctx.CurrentDecleration.TypeAttributes |= TypeAttributes.Abstract;
            //Ctx.CurrentDecleration.Attributes = MemberAttributes.Abstract | MemberAttributes.Public;
        }
        
    }

    public TemplateContext<IDocumentable> Ctx { get; set; }

    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public override Type ParentPage
    {
        get
        {
            if (Ctx.Data != Ctx.Data.Node.Graph.RootFilter)
            {
                if (Ctx.Data.Node != Ctx.Data)
                {
                    Ctx._("return typeof({0}PageBase)", Ctx.Data.Node.Name);
                }
                else
                {
                    Ctx._("return null");
                }
                    
            }
            else
            {
                Ctx._("return null");
            }
            return null;
        }
    }

    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public override Type RelatedNodeType
    {
        get
        {
            var classType = Ctx.Data as IClassTypeNode;
            if (classType != null)
            {
                Ctx._("return typeof({0})", classType.ClassName);
            }
            else
            {
                Ctx._("return typeof({0})", Ctx.Data.Name);
            }
            
            return null;
        }
        set { base.RelatedNodeType = value; }
    }

    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public override string Name
    {
        get
        {
            if (Ctx.CurrentDecleration.TypeAttributes == TypeAttributes.Abstract)
            {
                Ctx._("return base.Name");
            }
            else
            {
                Ctx._("return \"{0}\"", Ctx.Data.Name);
            }
           
            return null;
        }
    }
    [TemplateMethod(MemberGeneratorLocation.Both)]
    public override void GetContent(IDocumentationBuilder _)
    {
        
    }
}
