using System;
using System.CodeDom;
using System.IO;
using System.Linq;
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
            .HasSubNode<ShellNodeTypeNode>()
            .HasSubNode<ShellGraphTypeNode>()
            .HasSubNode<ShellSlotTypeNode>()
            .HasSubNode<TypeReferenceNode>()
            ;

        var graphConfig = container.AddNode<ShellGraphTypeNode>("Graph Type")
           .HasSubNode<ShellGeneratorTypeNode>()
           .Color(NodeColor.DarkDarkGray)
           .Validator(_=>_.RootNode == null,"A graph must have a root node.")
           ;

        var shellNodeConfig = container.AddNode<ShellNodeTypeNode>("Node Type")
            .AddFlag("Custom")
            .AddFlag("Inheritable")
            .HasSubNode<ShellSlotTypeNode>()
            .HasSubNode<ShellSectionNode>()
            .HasSubNode<ShellChildItemTypeNode>()
            .HasSubNode<ShellGeneratorTypeNode>()
            .HasSubNode<ShellNodeTypeReferenceSection>()
            .HasSubNode<ShellNodeTypeNode>()
            .HasSubNode<TypeReferenceNode>()
            .Inheritable()
            .ColorConfig(_ =>
            {
                if (!string.IsNullOrEmpty(_.DataBag["Color"]))
                {
                    var parsed = (NodeColor)Enum.Parse(typeof(NodeColor), _.DataBag["Color"], true);
                    return parsed;
                }
                return NodeColor.Gray;
            })
            ;

        
        var shellSlotConfig = container.AddNode<ShellSlotTypeNode>("Slot")
            .Color(NodeColor.Black)
            .AddFlag("Multiple")
            .AddFlag("Custom")
            .Validator(_ => _.ReferenceType == null, "This must be connected to a reference.")
            ;

       
        var shellReferenceConfig = container.AddNode<ShellNodeTypeReferenceSection>("Reference Section")
            .Color(NodeColor.Blue)
            .AddFlag("Automatic")
            .AddFlag("Custom")
            .Validator(_ => _.ReferenceClassName == null, "This must be connected to a reference.");

        var shellSectionConfig = container.AddNode<ShellSectionNode>("Node Section")
            .Color(NodeColor.Red)
            .AddFlag("Read Only")
            .Validator(_ => _.ReferenceType == null, "This must be connected to a reference.")
            ;

        var shellChildItemConfig = container.AddNode<ShellChildItemTypeNode>("Child Item")
            .Color(NodeColor.Blue)
            .AddFlag("Custom")
            .AddFlag("Typed");

        pluginConfig.AddCodeTemplate<ShellPluginTemplate>();
        graphConfig.AddCodeTemplate<ShellGraphTemplate>();
        shellNodeConfig.AddCodeTemplate<ShellNodeTypeTemplate>();
        shellNodeConfig.AddCodeTemplate<ShellNodeTypeViewModelTemplate>();
        shellNodeConfig.AddCodeTemplate<ShellNodeTypeDrawerTemplate>();
        shellSlotConfig.AddCodeTemplate<ShellSlotItemTemplate>();
        shellChildItemConfig.AddCodeTemplate<ShellChildTemplate>();
        shellReferenceConfig.AddCodeTemplate<ShellReferenceSectionTemplate>();

    }

    public string PluginClassFilename
    {
        get { return Path.Combine("Editor", "Plugin.cs"); }
    }
}