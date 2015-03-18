using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Core;
using Invert.Core.GraphDesigner;

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
        container.RegisterInstance<IToolbarCommand>(new PrintPlugins(), "Json");
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
            .HasSubNode<ShellNodeConfig>().HasSubNode<ScreenshotNode>()
            ;

        container.AddNode<ScreenshotNode, ScreenshotNodeViewModel, ScreenshotNodeDrawer>("Screenshot");
        //var graphConfig = container.AddNode<ShellGraphTypeNode>("Graph Type")
        //   .HasSubNode<ShellGeneratorTypeNode>()
        //   .HasSubNode<ScreenshotNode>()
        //   .Color(NodeColor.DarkDarkGray)
        //   .Validator(_=>_.RootNode == null,"A graph must have a root node.")
        //   ;

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
  


        //var shellNodeConfig = container.AddNode<ShellNodeTypeNode,ShellNodeTypeNodeViewModel,ShellNodeTypeNodeDrawer>("Node Type")
        //    .AddFlag("Custom")
        //    .AddFlag("Inheritable")
        //    .HasSubNode<ShellSlotTypeNode>()
        //    .HasSubNode<ShellSectionNode>()
        //    .HasSubNode<ShellChildItemTypeNode>()
        //    .HasSubNode<ShellGeneratorTypeNode>()
        //    .HasSubNode<ShellNodeTypeReferenceSection>()
        //    .HasSubNode<ShellNodeTypeNode>()
        //    .HasSubNode<TypeReferenceNode>()
        //    .Inheritable()
        //    .ColorConfig(_ =>
        //    {
        //        if (!string.IsNullOrEmpty(_.DataBag["Color"]))
        //        {
        //            var parsed = (NodeColor)Enum.Parse(typeof(NodeColor), _.DataBag["Color"], true);
        //            return parsed;
        //        }
        //        return NodeColor.Gray;
        //    })
        //    ;

        
        //var shellSlotConfig = container.AddNode<ShellSlotTypeNode>("Slot")
        //    .Color(NodeColor.Black)
        //    .AddFlag("Multiple")
        //    .AddFlag("Custom")
        //    .Validator(_ => _.ReferenceType == null, "This must be connected to a reference.")
        //    ;

       
        //var shellReferenceConfig = container.AddNode<ShellNodeTypeReferenceSection>("Reference Section")
        //    .Color(NodeColor.Blue)
        //    .AddFlag("Automatic")
        //    .AddFlag("Custom")
        //    .Validator(_ => _.ReferenceClassName == null, "This must be connected to a reference.");

        //var shellSectionConfig = container.AddNode<ShellSectionNode>("Node Section")
        //    .Color(NodeColor.Red)
        //    .AddFlag("Read Only")
        //    .Validator(_ => _.ReferenceType == null, "This must be connected to a reference.")
        //    ;

        //var shellChildItemConfig = container.AddNode<ShellChildItemTypeNode>("Child Item")
        //    .Color(NodeColor.Blue)
        //    .AddFlag("Custom")
        //    .AddFlag("Typed");

        //pluginConfig.AddCodeTemplate<ShellPluginTemplate>();
        //graphConfig.AddCodeTemplate<ShellGraphTemplate>();
        //shellNodeConfig.AddCodeTemplate<ShellNodeTypeTemplate>();
        //shellNodeConfig.AddCodeTemplate<ShellNodeTypeViewModelTemplate>();
        //shellNodeConfig.AddCodeTemplate<ShellNodeTypeDrawerTemplate>();
        ////shellSlotConfig.AddCodeTemplate<ShellSlotItemTemplate>();
        //RegisteredTemplateGeneratorsFactory.RegisterTemplate<IShellSlotType,ShellSlotItemTemplate>();
        //shellChildItemConfig.AddCodeTemplate<ShellChildTemplate>();
        //shellReferenceConfig.AddCodeTemplate<ShellReferenceSectionTemplate>();

    }

    public string PluginClassFilename
    {
        get { return Path.Combine("Editor", "Plugin.cs"); }
    }
}

