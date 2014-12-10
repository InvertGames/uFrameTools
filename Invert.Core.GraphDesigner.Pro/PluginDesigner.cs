using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;

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
#if DEBUG
        container.RegisterInstance<IToolbarCommand>(new PrintPlugins(), "Json");
#endif
        container.RegisterInstance<IDiagramNodeCommand>(new SelectColorCommand(), "SelectColor");
        var pluginConfig = container
            .AddItem<ShellMemberGeneratorSectionReference>()
            .AddItem<ShellNodeSectionsSlot>()
            .AddItem<ShellNodeInputsSlot>()
            .AddItem<ShellNodeOutputsSlot>()
            .AddItem<ShellMemberGeneratorInputSlot>()
            .AddItem<TemplatePropertyReference>()
            .AddItem<TemplateMethodReference>()
            .AddItem<TemplateFieldReference>()
            .AddItem<TemplateEventReference>()
            .AddItem<ShellAcceptableReferenceType>()
            .AddItem<ShellConnectableReferenceType>()
            .AddTypeItem<ShellPropertySelectorItem>()
            .AddGraph<PluginGraph, ShellPluginNode>("Shell Plugin")
            .Color(NodeColor.Green)
            .HasSubNode<IShellNode>()
            .HasSubNode<ShellNodeTypeNode>()
            .HasSubNode<ShellGraphTypeNode>()
            .HasSubNode<ShellSlotTypeNode>()
            ;

        var graphConfig = container.AddNode<ShellGraphTypeNode>("Graph Type")
           .HasSubNode<ShellGeneratorTypeNode>()
           .Color(NodeColor.DarkDarkGray)
           .Validator(_=>_.RootNode == null,"A graph must have a root node.")
           ;

        var shellNodeConfig = container.AddNode<ShellNodeTypeNode, ShellNodeTypeViewModel, NodeDesignerDrawer>("Node Type")
            .AddFlag("Custom")
            .AddFlag("Inheritable")
            .HasSubNode<ShellSlotTypeNode>()
            .HasSubNode<ShellSectionNode>()
            .HasSubNode<ShellChildItemTypeNode>()
            .HasSubNode<ShellGeneratorTypeNode>()
            .HasSubNode<ShellNodeTypeReferenceSection>()
            .HasSubNode<ShellNodeTypeNode>()
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

        pluginConfig
            .AddEditableClass<ShellPluginClassGenerator>("Plugin")
            .NamespacesConfig(new[] { "Invert.Core.GraphDesigner", "Invert.Core.GraphDesigner.Unity", "Invert.uFrame", "Invert.uFrame.Editor" })
            .BaseTypeConfig(new CodeTypeReference(typeof(DiagramPlugin)))
            .ClassNameConfig(node => string.Format("{0}Plugin", node.Name))
            .FilenameConfig(node => Path.Combine("Editor", string.Format("{0}Plugin.cs", node.Name)))
            .DesignerFilenameConfig(node => Path.Combine("Editor", "Plugin.cs"))
            .OverrideTypedMethod(typeof(DiagramPlugin), "Initialize", CreatePluginInitializeMethod)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellNodeTypeNode>(), CreateGraphConfigProperty)
            //.MembersFor(p => p.Project.NodeItems.OfType<ShellGraphTypeNode>(), CreateConfigProperty)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellGeneratorTypeNode>(), CreateGeneratorConfigProperty)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellChildItemTypeNode>().Where(x => x["Typed"]),
                    CreateGetSelectionCommandMethod
                )
            ;

        // Special connections that aren't slots
       // container.RegisterConnectable<ShellNodeTypeNode, ShellNodeTypeNode>();
       // container.RegisterConnectable<ShellChildItemTypeNode, SingleInputSlot<ShellChildItemTypeNode>>();

        graphConfig.AddCodeTemplate<ShellGraphTemplate>();
        shellNodeConfig.AddCodeTemplate<ShellNodeTypeTemplate>();
        shellSlotConfig.AddCodeTemplate<ShellSlotItemTemplate>();
        shellChildItemConfig.AddCodeTemplate<ShellChildTemplate>();
        shellReferenceConfig.AddCodeTemplate<ShellReferenceSectionTemplate>();

        //container.RegisterDrawer<ShellMemberNodeViewModel, ShellMemberNodeDrawer>();
        //container.RegisterDrawer<PropertyFieldViewModel, PropertyFieldDrawer>();

        // TODO put this in some kind of automatic node adder extension method
        //foreach (var item in InvertApplication.GetDerivedTypes<ShellMemberGeneratorNode>(false, false))
        //{
            
          //  container.RegisterRelation(item, typeof(ViewModel), typeof(ShellMemberNodeViewModel));

           // var config = new NodeConfig<ShellMemberGeneratorNode>(container);
        //    container.RegisterInstance<NodeConfigBase>(config, item.Name);
        //    config.Name = item.Name.Replace("Shell", "").Replace("Node", "");
        //    config.Tags.Add(config.Name);

        //    shellCodeGeneratorConfig.HasSubNode(item);
        //}


    }
    
    private GraphTypeInfo[] GetPropertySelectorTypes(IDiagramNodeItem arg1)
    {
        return arg1.Node.Project.NodeItems.OfType<GenericNode>().Select(p => new GraphTypeInfo()
        {
            Name = p.Identifier,
            Label = p.Name
        }).ToArray();
    }

    private static void ConfigureGraphTypeGenerators(NodeConfig<ShellGraphTypeNode> graphConfig)
    {
        graphConfig
            .AddEditableClass<ShellGraphClassGenerator>("Graph")
            .AsDerivedEditorEditableClass("{0}Graph", "Plugin",
                p => new CodeTypeReference(string.Format("UnityGraphData<GenericGraphData<{0}>>", p.RootNode.ClassName)), "Graphs")
            ;

        //graphConfig.AddEditableClass<ShellGraphTypeNodeClassGenerator>("GraphNode")
        //    .BaseTypeConfig(new CodeTypeReference(typeof(GenericNode)))
        //    .ClassNameConfig(node => string.Format("{0}Node", node.Name))
        //    .FilenameConfig(node => Path.Combine("Editor", string.Format("{0}Node.cs", node.Name)))
        //    .DesignerFilenameConfig(node => Path.Combine("Editor", "Plugin.cs"));
    }

    private static void ConfigureChildItemGenerators(NodeConfig<ShellChildItemTypeNode> shellChildItemConfig)
    {
        shellChildItemConfig
            .AddEditableClass<ShellChildItemTypeNodeClassGenerator>("Plugin")
            .AsDerivedEditorEditableClass("{0}ChildItem", "Plugin", _ => _["Typed"]
                ? new CodeTypeReference(typeof(GenericTypedChildItem))
                : new CodeTypeReference(typeof(GenericNodeChildItem)), "ChildItems")
            ;

        shellChildItemConfig
            .AddEditableClass<ShellChildItemTypeNodeViewModelGenerator>("ChildItemViewModel")
            .AsDerivedEditorEditableClass("{0}ViewModel", "Plugin", _ => _["Typed"]
                ? new CodeTypeReference(typeof(TypedItemViewModel))
                : new CodeTypeReference(string.Format("GenericItemViewModel<{0}>", _.ClassName)), "ChildItems")
            .Member(_ => CreateViewModelConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
            .OnlyIf(_ => _["Custom"])
            ;

        shellChildItemConfig
            .AddEditableClass<ShellChildItemTypeNodeDrawerGenerator>("ChildItemDrawer")
            .AsDerivedEditorEditableClass("{0}Drawer", "Plugin", _ => _["Typed"]
                ? new CodeTypeReference(typeof(TypedItemDrawer))
                : new CodeTypeReference(typeof(ItemDrawer)), "ChildItems")
            .Member(_ => CreateDrawerConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
            .OnlyIf(_ => _["Custom"])
            ;
    }

    private static CodeTypeMember CreateViewModelConstructor(CodeTypeDeclaration decleration, IShellNode data)
    {
        var constructor = new CodeConstructor()
        {
            Attributes = MemberAttributes.Public,
            Name = decleration.Name
        };
        constructor.Parameters.Add(new CodeParameterDeclarationExpression(data.ClassName, "item"));
        if (data is ShellNodeTypeNode)
        {
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DiagramViewModel), "parent"));
        }
        else
        {
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DiagramNodeViewModel), "parent"));
        }

        constructor.BaseConstructorArgs.Add(new CodeSnippetExpression("item"));
        constructor.BaseConstructorArgs.Add(new CodeSnippetExpression("parent"));
        return constructor;
    }

    private static void ConfigureReferenceSectionGenerators(NodeConfig<ShellNodeTypeReferenceSection> shellReferenceConfig)
    {
        shellReferenceConfig.AddEditableClass<ShellNodeTypeSectionClassGenerator>("Plugin")
            .AsDerivedEditorEditableClass("{0}Reference", "Plugin",
                _ => new CodeTypeReference(string.Format("GenericReferenceItem<{0}>", _.ReferenceClassName)),
                "ReferenceItems")
            .OnlyIf(_ => _.ReferenceClassName != null)
            ;

        shellReferenceConfig
            .AddEditableClass<ShellNodeTypeReferenceSectionViewModelGenerator>("ReferenceSectionViewModel")
            .AsDerivedEditorEditableClass("{0}ViewModel", "Plugin", _ => new CodeTypeReference(string.Format("GenericItemViewModel<{0}>", _.ClassName)), "ReferenceItems")
            .Member(_ => CreateViewModelConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
            .OnlyIf(_ => _.IsCustom)
            ;

        shellReferenceConfig
            .AddEditableClass<ShellNodeTypeReferenceSectionDrawerGenerator>("ReferenceSectionDrawer")
            .AsDerivedEditorEditableClass("{0}Drawer", "Plugin", _ => new CodeTypeReference(string.Format("ItemDrawer<{0}ViewModel>", _.Name)), "ReferenceItems")
            .Member(_ => CreateDrawerConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
            .OnlyIf(_ => _.IsCustom)
            ;

    }

    private static void ConfigureCodeGeneratorGenerators(NodeConfig<ShellGeneratorTypeNode> shellCodeGeneratorConfig)
    {

        shellCodeGeneratorConfig.AddEditableClass<ShellGeneratorTypeNodeGenerator>("Plugin")
            .AsDerivedEditorEditableClass("{0}Generator", "Plugin",
                _ => new CodeTypeReference(string.Format("TypeClassGenerator<{0},{1}>", _.GeneratorFor.ClassName, _.BaseType.Name)),
                "Generators")
            .OnlyIf(_ => _.GeneratorFor != null)

            ;
    }

    private static void ConfigureShellSlotTypeGenerators(NodeConfig<ShellSlotTypeNode> shellInputReferenceConfig)
    {
        shellInputReferenceConfig.AddEditableClass<ShellSlotTypeNodeClassGenerator>("Plugin")
            .AsDerivedEditorEditableClass("{0}", "Plugin",
            _ => _.AllowMultiple ?
                new CodeTypeReference(string.Format("Multi{0}Slot<I{1}>", _.IsOutput ? "Output" : "Input", _.ClassName)) :
                new CodeTypeReference(string.Format("Single{0}Slot<I{1}>", _.IsOutput ? "Output" : "Input", _.ClassName)), "Slots")
            ;

        shellInputReferenceConfig
          .AddEditableClass<ShellSlotTypeNodeViewModelGenerator>("SlotViewModel")
          .AsDerivedEditorEditableClass("{0}ViewModel", "Plugin", _ => new CodeTypeReference(string.Format("GenericItemViewModel<{0}>", _.ClassName)), "Slots")
          .Member(_ => CreateViewModelConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
          .OnlyIf(_ => _.IsCustom)
          ;

        shellInputReferenceConfig
            .AddEditableClass<ShellSlotTypeNodeDrawerGenerator>("SlotDrawer")
            .AsDerivedEditorEditableClass("{0}Drawer", "Plugin", _ => new CodeTypeReference(string.Format("SlotDrawer<{0}ViewModel>", _.Name)), "Slots")
            .Member(_ => CreateDrawerConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
            .OnlyIf(_ => _.IsCustom)
            ;
    }

    public string PluginClassFilename
    {
        get { return Path.Combine("Editor", "Plugin.cs"); }
    }
    //private void ConfigureShellNodeGenerators(NodeConfig<ShellNodeTypeNode> shellNodeConfig)
    //{
    //    shellNodeConfig.AddEditableClass<ShellNodeTypeGenerator>("Node")
    //        .FilenameConfig(node => Path.Combine("Editor", node.ClassName + ".cs"))
    //        .DesignerFilenameConfig(PluginClassFilename);
    //        ;

    //    shellNodeConfig.AddEditableClass<ShellNodeTypeNodeViewModelGenerator>("NodeViewModelGenerator")
    //      .AsDerivedEditorEditableClass("{0}NodeViewModel", "Plugin",
    //          _ => new CodeTypeReference(string.Format("GenericNodeViewModel<{0}Node>", _.Name)), "Nodes")
    //      .Member(_ => CreateViewModelConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
    //      .OnlyIf(_ => _["Custom"])
    //      ;

    //    shellNodeConfig.AddEditableClass<ShellNodeTypeNodeDrawerGenerator>("NodeDrawerGenerator")
    //      .AsDerivedEditorEditableClass("{0}Drawer", "Plugin",
    //          _ => new CodeTypeReference(string.Format("GenericNodeDrawer<{0}Node, {0}NodeViewModel>", _.Name)), "Nodes")
    //      .Member(_ => CreateDrawerConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
    //            //.OverrideTypedMethod(typeof (DiagramNodeDrawer), "GetContentDrawers", null, true,
    //            //    MemberGeneratorLocation.EditableFile)
    //      .OnlyIf(_ => _["Custom"])
    //      ;
    //    return;
    //    shellNodeConfig.AddEditableClass<ShellNodeTypeClassGenerator>("Node")
    //      .AsDerivedEditorEditableClass("{0}Node", "Plugin", node => node["Inheritable"]
    //                   ? new CodeTypeReference(typeof(GenericInheritableNode))
    //                   : new CodeTypeReference(typeof(GenericNode)), "Nodes")
    //      .MembersPerChild<ShellNodeSectionsSlot>(GetNodeSectionProperty)
    //      .MembersPerChild<ShellNodeInputsSlot>(CreateInputSlotProperty)
    //      .MembersPerChild<ShellNodeOutputsSlot>(CreateOutputSlotProperty)
    //      .MembersFor<ShellNodeSectionsSlot>(p => p.Sections.Where(x => x.SourceItem is ShellNodeTypeReferenceSection), CreateReferenceSectionProperty)
    //      ;




    //}

    private CodeTypeMember CreateOutputSlotProperty(LambdaMemberGenerator<ShellNodeOutputsSlot> arg1)
    {
        var property = new CodeMemberProperty()
        {
            Name = arg1.Data.Name + "OutputSlot",
            Type = new CodeTypeReference(arg1.Data.SourceItem.Name),
            Attributes = MemberAttributes.Public,
            HasGet = true,
            HasSet = false
        };
        property.GetStatements.Add(
            new CodeSnippetExpression(string.Format("return GetConnectionReference<{0}>()", arg1.Data.SourceItem.Name)));

        return property;
    }

    private CodeTypeMember CreateInputSlotProperty(LambdaMemberGenerator<ShellNodeInputsSlot> arg1)
    {
        var property = new CodeMemberProperty()
        {
            Name = arg1.Data.Name + "InputSlot",
            Type = new CodeTypeReference(arg1.Data.SourceItem.Name),
            Attributes = MemberAttributes.Public,
            HasGet = true,
            HasSet = false
        };
        property.GetStatements.Add(
            new CodeSnippetExpression(string.Format("return GetConnectionReference<{0}>()", arg1.Data.SourceItem.Name)));
        return property;
    }

    private static CodeTypeMember CreateDrawerConstructor(CodeTypeDeclaration decleration, IShellNode data)
    {
        var constructor = new CodeConstructor()
        {
            Attributes = MemberAttributes.Public,
            Name = decleration.Name
        };
        if (data is ShellNodeTypeNode)
        {
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(data.Name + "NodeViewModel", "viewModel"));
        }
        else
        {
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(data.Name + "ViewModel", "viewModel"));
        }

        constructor.BaseConstructorArgs.Add(new CodeSnippetExpression("viewModel"));
        return constructor;
    }

    private void CreateCustomDrawMethod(ShellNodeTypeNode arg1, CodeMemberMethod arg2)
    {
        arg2.Comments.Add(new CodeCommentStatement("Use GUI methods here"));
    }

    private CodeTypeMember CreateViewModelConstructor(LambdaMemberGenerator<IShellNode> arg1)
    {
        var constructor = new CodeConstructor()
        {
            Attributes = MemberAttributes.Public,
            Name = arg1.Decleration.Name
        };
        constructor.Parameters.Add(new CodeParameterDeclarationExpression(arg1.Data.ClassName, "item"));
        constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(DiagramViewModel), "diagram"));
        constructor.BaseConstructorArgs.Add(new CodeSnippetExpression("item"));
        constructor.BaseConstructorArgs.Add(new CodeSnippetExpression("diagram"));
        return constructor;
    }

    private CodeTypeMember CreateReferenceSectionProperty(LambdaMemberGenerator<ShellNodeSectionsSlot> arg1)
    {
        var referenceItem = arg1.Data.SourceItem as ShellNodeTypeReferenceSection;
        var property = new CodeMemberProperty()
        {
            Name = "Possible" + arg1.Data.Name,
            Attributes = MemberAttributes.Public,
            HasGet = true,
            HasSet = false,
            Type = new CodeTypeReference(string.Format("IEnumerable<{0}>", referenceItem.ReferenceClassName))
        };
        if (referenceItem["Automatic"])
        {
            property.GetStatements.Add(
                new CodeSnippetExpression("yield break"));
        }
        else
        {
            property.GetStatements.Add(
                new CodeSnippetExpression(string.Format("return Project.AllGraphItems.OfType<{0}>()",
                    referenceItem.ReferenceClassName)));
        }


        return property;
    }

    private CodeTypeMember CreateGetSelectionCommandMethod(LambdaMemberGenerator<ShellChildItemTypeNode> arg1)
    {
        var method = new CodeMemberMethod()
        {
            Name = string.Format("Get{0}SelectionCommand", arg1.Data.Name),
            Attributes = MemberAttributes.Public,
            ReturnType = new CodeTypeReference(typeof(SelectItemTypeCommand))
        };
        method.Statements.Add(new CodeSnippetExpression("return new SelectItemTypeCommand() { IncludePrimitives = true, AllowNone = false }"));
        return method;
    }

    private CodeTypeMember GetNodeSectionProperty(LambdaMemberGenerator<ShellNodeSectionsSlot> arg1)
    {
        var isReferenceItem = arg1.Data.SourceItem is ShellNodeTypeReferenceSection;

        //var typeName = isReferenceItem ? arg1.Data.SourceItem.ClassName : arg1.Data.SourceItem.ReferenceType.ClassName;
        var typeName = isReferenceItem ? arg1.Data.SourceItem.ClassName : arg1.Data.SourceItem.ReferenceClassName;
        var property = new CodeMemberProperty()
        {
            Name = arg1.Data.Name,
            Type = new CodeTypeReference(string.Format("IEnumerable<{0}>", typeName)),
            Attributes = MemberAttributes.Public,
            HasGet = true,
            HasSet = false
        };

        //if (isReferenceItem)
        //{
        //    property.GetStatements.Add(
        //        new CodeSnippetExpression(string.Format("return ChildItems.OfType<{0}>()",
        //            typeName)));

        //    return property;
        //}

        property.GetStatements.Add(
            new CodeSnippetExpression(string.Format("return ChildItems.OfType<{0}>()",
                typeName)));

        return property;
    }

    private CodeTypeMember CreateGraphConfigProperty(LambdaMemberGenerator<ShellNodeTypeNode> arg1)
    {
        return new CodeMemberField(string.Format("NodeConfig<{0}>", arg1.Data.ClassName), arg1.Data.Name)
        {
            Attributes = MemberAttributes.Public
        };
    }

    private CodeTypeMember CreateConfigProperty(LambdaMemberGenerator<ShellGraphTypeNode> arg1)
    {
        return new CodeMemberField(string.Format("NodeConfig<{0}>", arg1.Data.Name), arg1.Data.RootNode.Name)
        {
            Attributes = MemberAttributes.Public
        };
    }
    private CodeTypeMember CreateGeneratorConfigProperty(LambdaMemberGenerator<ShellGeneratorTypeNode> arg1)
    {
        return new CodeMemberField(string.Format("NodeGeneratorConfig<{0}>", arg1.Data.GeneratorFor.ClassName), arg1.Data.Name + "Config")
        {
            Attributes = MemberAttributes.Public
        };
    }

    private void CreatePluginInitializeMethod(ShellPluginNode shellPluginNode, CodeMemberMethod method)
    {
        foreach (var item in shellPluginNode.Project.NodeItems.OfType<ShellChildItemTypeNode>())
        {
            if (!item["Typed"]) continue;
            method._(
                "container.RegisterInstance<IEditorCommand>(Get{0}SelectionCommand(), typeof({1}).Name + \"TypeSelection\");", item.Name, item.ClassName);

        }
        foreach (var itemType in shellPluginNode.Project.NodeItems.OfType<IShellNode>().Where(p => p.IsValid))
        {
            if (itemType is ShellNodeTypeNode) continue;
            if (itemType is ShellSectionNode) continue;
            if (itemType is ShellGraphTypeNode) continue;
            if (itemType.Flags.ContainsKey("Typed") && itemType.Flags["Typed"])
            {
                if (itemType.IsCustom)
                {
                    method.Statements.Add(
                        new CodeSnippetExpression(string.Format("container.AddTypeItem<{0},{1}ViewModel,{1}Drawer>()", itemType.ClassName, itemType.Name)));
                }
                else
                {
                    method.Statements.Add(
                        new CodeSnippetExpression(string.Format("container.AddTypeItem<{0}>()", itemType.ClassName)));
                }

            }
            else
            {
                if (itemType.Flags.ContainsKey("Custom") && itemType.Flags["Custom"])
                {
                    method.Statements.Add(
                    new CodeSnippetExpression(string.Format("container.AddItem<{0}, {1}ViewModel, {1}Drawer>()", itemType.ClassName, itemType.Name)));
                }
                else
                {
                    method.Statements.Add(
                    new CodeSnippetExpression(string.Format("container.AddItem<{0}>()", itemType.ClassName)));
                }


            }

        }
        var graphTypes = shellPluginNode.Project.NodeItems.OfType<ShellGraphTypeNode>().Where(p => p.IsValid).ToArray();
        //foreach (var graphType in graphTypes)
        //{
        //    var varName = graphType.Name;
        //    method.Statements.Add(new CodeSnippetExpression(string.Format("{1} = container.AddGraph<{0}Graph, {2}>(\"{0}\")", graphType.RootNode.Name, varName,graphType.RootNode.ClassName)));
        //    //foreach (var item in graphType.SubNodes)
        //    //{
        //    //    method.Statements.Add(
        //    //        new CodeSnippetExpression(string.Format("{0}.HasSubNode<{1}Node>()", varName, item.Name)));
        //    //}

        //}
        foreach (var nodeType in shellPluginNode.Project.NodeItems.OfType<ShellNodeTypeNode>().Where(p => p.IsValid))
        {
            InitializeNodeType(method, nodeType, graphTypes.FirstOrDefault(p => p.RootNode == nodeType));
        }

        foreach (var nodeType in shellPluginNode.Project.NodeItems.OfType<IShellConnectable>().Where(p => p.IsValid))
        {
            foreach (var item in nodeType.ConnectableTo)
            {
                method._("container.Connectable<{0},{1}>()", nodeType.ClassName, item.SourceItem.ClassName);
            }

        }
        foreach (var nodeType in shellPluginNode.Project.NodeItems.OfType<IReferenceNode>().Where(p => p.IsValid))
        {
            if (nodeType is ShellSlotTypeNode) continue;

            if (nodeType.Flags.ContainsKey("Output") && nodeType.Flags["Output"])
            {
                method._("container.Connectable<{0},{1}>()", nodeType.ClassName, nodeType.ReferenceClassName);
            }
            else
            {
                method._("container.Connectable<{0},{1}>()", nodeType.ReferenceClassName, nodeType.ClassName);
            }
                
            //foreach (var item in nodeType.AcceptableTypes)
            //{
               
            //}

        }

    }

    private static void InitializeNodeType(CodeMemberMethod method, ShellNodeTypeNode nodeType, ShellGraphTypeNode graphType)
    {
        var varName = nodeType.Name;
        var type = graphType == null ? "Node" : "Graph";
        if (graphType != null)
        {
            method.Statements.Add(
                new CodeSnippetExpression(string.Format("{1} = container.AddGraph<{0}, {2}>(\"{0}\")",
                    graphType.ClassName, varName, graphType.RootNode.ClassName)));
        }
        else
        {
            if (nodeType["Custom"])
            {
                method.Statements.Add(
               new CodeSnippetExpression(string.Format("{1} = container.Add{2}<{0}Node,{0}NodeViewModel,{0}Drawer>(\"{0}\")", nodeType.Name, varName, type)));
            }
            else
            {
                method.Statements.Add(
                  new CodeSnippetExpression(string.Format("{1} = container.Add{2}<{0}Node>(\"{0}\")", nodeType.Name, varName, type)));
            }
        }
      

        if (nodeType["Inheritable"])
        {
            method.Statements.Add(new CodeSnippetExpression(string.Format("{0}.Inheritable()", varName)));
        }
        if (!string.IsNullOrEmpty(nodeType.DataBag["Color"]))
        {
            method.Statements.Add(
                new CodeSnippetExpression(string.Format("{0}.Color(NodeColor.{1})", varName, nodeType.DataBag["Color"])));
        }


        foreach (var item in nodeType.SubNodes)
        {
            method.Statements.Add(
                new CodeSnippetExpression(string.Format("{0}.HasSubNode<{1}Node>()", varName, item.Name)));
        }
        //foreach (var item in nodeType.InputSlots)
        //{
        //    var source = item.SourceItem;
        //    method.Statements.Add(new CodeSnippetExpression(string.Format("{0}.Input<{1},{2}>(\"{3}\", {4})",
        //        varName, source.ReferenceType.ClassName,
        //        source.ClassName,
        //        item.Name,
        //        source.Flags.ContainsKey("Multiple") && source.Flags["Multiple"] ? "true" : "false"
        //        )));

        //}
        //foreach (var item in nodeType.OutputSlots)
        //{
        //    var source = item.SourceItem;
        //    method.Statements.Add(new CodeSnippetExpression(string.Format("{0}.Output<{1},{2}>(\"{3}\", {4})",
        //        varName, source.ReferenceType.ClassName,
        //        source.ClassName,
        //        item.Name,
        //        source.Flags.ContainsKey("Multiple") && source.Flags["Multiple"] ? "true" : "false"
        //        )));
        //}
        //foreach (var item in nodeType.Sections)
        //{
        //    var sectionItem = item.SourceItem;
        //    var referenceTypeSection = sectionItem as ShellNodeTypeReferenceSection;
        //    if (referenceTypeSection != null)
        //    {
        //        method.Statements.Add(
        //            new CodeSnippetExpression(string.Format("{0}.ReferenceSection<{1},{3}>(\"{2}\",node=>node.Possible{2}, true)", varName, sectionItem.ReferenceType.ClassName, item.Name, sectionItem.ClassName))
        //            );
        //    }
        //    else
        //    {
        //        if (sectionItem.ReferenceType.Flags.ContainsKey("Typed") && sectionItem.ReferenceType.Flags["Typed"])
        //        {
        //            method.Statements.Add(
        //                   new CodeSnippetExpression(string.Format("{0}.TypedSection<{1}>(\"{2}\", Get{3}SelectionCommand())", varName, sectionItem.ReferenceType.ClassName, item.Name, sectionItem.ReferenceType.Name))
        //               );

        //        }
        //        else
        //        {
        //            method.Statements.Add(
        //                new CodeSnippetExpression(string.Format("{0}.Section<{1}>(\"{2}\",node=>node.{2}, true)", varName, sectionItem.ReferenceType.ClassName, item.Name))
        //            );
        //        }

        //    }

        //}

        //foreach (var item in nodeType.Generators)
        //{
        //    if (item.IsDesignerOnly)
        //    {
        //        method.Statements.Add(
        //            new CodeSnippetExpression(string.Format("{0}Config = {1}.AddDesignerOnlyClass<{0}Generator>(\"{0}\")", item.Name,
        //                varName)));
        //    }
        //    else
        //    {
        //        method.Statements.Add(
        //            new CodeSnippetExpression(string.Format("{0}Config = {1}.AddEditableClass<{0}Generator>(\"{0}\")", item.Name,
        //                varName)));
        //    }
        //    if (item.IsEditorExtension)
        //    {
        //        method._("{0}Config.FilenameConfigAsEditor(node=> Path.Combine(\"{1}\", node.NameAs{0}+ \".cs\"))", item.Name, item.FolderName);
        //        method._("{0}Config.DesignerFilenameConfigAsEditor(\"{0}.cs\")", item.Name);
        //    }
        //    else
        //    {
        //        method._("{0}Config.FilenameConfig(node=> Path.Combine(\"{1}\", node.NameAs{0} + \".cs\"))", item.Name, item.FolderName);
        //        method._("{0}Config.DesignerFilenameConfig(\"{0}.cs\")", item.Name);
        //    }
        //}
    }
}