using System;
using System.CodeDom;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame;
using Invert.uFrame.Editor;
using UnityEngine;

public class ShellPropertySelectorItem : GenericTypedChildItem
{

}
public class PluginDesigner : DiagramPlugin
{
    public override void Initialize(uFrameContainer container)
    {
        container.RegisterInstance<IDiagramNodeCommand>(new SelectColorCommand(), "SelectColor");
        var pluginConfig = container
            .AddItem<ShellMemberGeneratorSectionReference>()
            .AddItem<ShellNodeSectionsSlot>()
            .AddItem<ShellNodeInputsSlot>()
            .AddItem<ShellNodeOutputsSlot>()
            .AddItem<ShellMemberGeneratorInputSlot>()
            .AddItem<OverrideMethodReference>()
            .AddTypeItem<ShellPropertySelectorItem>()
            .AddGraph<PluginGraph, ShellPluginNode>("Shell Plugin")
            .Color(NodeColor.Green)
            .HasSubNode<IShellReferenceType>()
            .HasSubNode<ShellNodeTypeNode>()
            .HasSubNode<ShellGraphTypeNode>()
            ;

        var graphConfig = container.AddNode<ShellGraphTypeNode>("Graph Type")
           .Output<ShellNodeTypeNode, ShellNodeSubNodesSlot>("Sub Nodes",true)
           .Output<ShellGeneratorTypeNode, ShellNodeGeneratorsSlot>("Generators", true)
           .HasSubNode<ShellGeneratorTypeNode>()
           .Color(NodeColor.DarkDarkGray)
           ;

        ConfigureGraphTypeGenerators(graphConfig);

        var shellNodeConfig = container.AddNode<ShellNodeTypeNode,ShellNodeTypeViewModel,NodeDesignerDrawer>("Node Type")
            .Output<ShellNodeTypeNode, ShellNodeSubNodesSlot>("Sub Nodes", true)
            .Output<ShellGeneratorTypeNode, ShellNodeGeneratorsSlot>("Generators", true)
            .AddFlag("Custom")
            .AddFlag("Inheritable")
            .HasSubNode<ShellSlotTypeNode>()
            .HasSubNode<ShellNodeTypeSection>()
            .HasSubNode<ShellChildItemTypeNode>()
            .HasSubNode<ShellGeneratorTypeNode>()
            .HasSubNode<ShellNodeTypeReferenceSection>()
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

        shellNodeConfig.ReferenceSection<ShellNodeTypeSection, ShellNodeSectionsSlot>("Sections",
            _ => _.Project.NodeItems.OfType<ShellNodeTypeSection>(), true);

        shellNodeConfig.ReferenceSection<ShellSlotTypeNode, ShellNodeInputsSlot>("Inputs",
            _ => _.Project.NodeItems.OfType<ShellSlotTypeNode>(), true, true);

        shellNodeConfig.ReferenceSection<ShellSlotTypeNode, ShellNodeOutputsSlot>("Outputs",
            _ => _.Project.NodeItems.OfType<ShellSlotTypeNode>(), true, true);

        shellNodeConfig.Proxy("Project Nodes", _ => _.Project.NodeItems.OfType<ShellNodeTypeNode>());

        ConfigureShellNodeGenerators(shellNodeConfig);

        var shellInputReferenceConfig = container.AddNode<ShellSlotTypeNode>("Slot")
            .Color(NodeColor.Black)
            .Input<IShellReferenceType, ReferenceItemType>("Reference Type", false)
            .AddFlag("Multiple")
            .AddFlag("Custom")
            .Validator(_ => _.ReferenceType == null, "This must be connected to a reference.")
            //.Inheritable()
            ;

        ConfigureShellSlotTypeGenerators(shellInputReferenceConfig);
 
        var shellCodeGeneratorConfig = container.AddNode<ShellGeneratorTypeNode>("Code Generator")
            .Color(NodeColor.Purple)

            .Validator(node=>!node.InputsFrom<ShellNodeGeneratorsSlot>().Any(),"Generator is not connected.")
            //.Proxy("Items",node=>node.InputFrom<ShellNodeTypeNode>().ChildItems.OfType<Shell>())
            .AddFlag("Designer Only");
        shellCodeGeneratorConfig.ReferenceSection<GenericReferenceItem, ShellMemberGeneratorSectionReference>("Members",
            node => node.InputFrom<ShellNodeGeneratorsSlot>().Node.ContainedItems.OfType<GenericReferenceItem>(), true,
            false
            );
       
        var sec = shellCodeGeneratorConfig.Section("Overrides",
            node =>
                node.BaseType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.IsVirtual || p.IsAbstract)
                    .Select(p => new OverrideMethodReference()
                    {
                        Name = p.Name
                    }), true, p => { }
            );
        sec.HasPredefinedOptions = true;
        //container.Connectable<ShellNodeSectionsSlot, ShellGeneratorTypeNode>(Color.blue, true);

        container.RegisterDrawer<ShellMemberNodeViewModel, ShellMemberNodeDrawer>();
        container.RegisterDrawer<PropertyFieldViewModel, PropertyFieldDrawer>();

        foreach (var item in InvertApplication.GetDerivedTypes<ShellMemberGeneratorNode>(false, false))
        {
            container.RegisterRelation(item, typeof(ViewModel), typeof(ShellMemberNodeViewModel));

            var config = new NodeConfig<ShellMemberGeneratorNode>(container);
            container.RegisterInstance<NodeConfigBase>(config, item.Name);
            config.Name = item.Name.Replace("Shell", "").Replace("Node", "");
            config.Tags.Add(config.Name);
            if (item.GetCustomAttributes(typeof (SingleMemberGenerator), true).Length > 0)
            {
                config.Input<ShellGeneratorTypeNode, ShellMemberGeneratorInputSlot>("For", false);
            }
            else
            {
                config.Input<ShellMemberGeneratorSectionReference, ShellMemberGeneratorInputSlot>("For", false);
            }
            

            shellCodeGeneratorConfig.HasSubNode(item);
        }

        ConfigureCodeGeneratorGenerators(shellCodeGeneratorConfig);

        var shellReferenceConfig = container.AddNode<ShellNodeTypeReferenceSection>("Reference Section")
            .Color(NodeColor.Blue)
            .AddFlag("Automatic")
            .AddFlag("Custom")
            .Input<IShellReferenceType, ReferenceItemType>("Reference Type", false)
            .Validator(_ => _.ReferenceType == null, "This must be connected to a reference.");
            ConfigureReferenceSectionGenerators(shellReferenceConfig);

        var shellSectionConfig = container.AddNode<ShellNodeTypeSection>("Node Section")
            .Color(NodeColor.Red)
            .AddFlag("Read Only")
            .Input<IShellReferenceType, ReferenceItemType>("Reference Type", false)
            .Validator(_ => _.ReferenceType == null, "This must be connected to a reference.")
            ;

        var shellChildItemConfig = container.AddNode<ShellChildItemTypeNode>("Child Item")
            .Color(NodeColor.Blue)
            .AddFlag("Custom")
            .AddFlag("Typed");

        ConfigureChildItemGenerators(shellChildItemConfig);

        pluginConfig
            .AddEditableClass<ShellPluginClassGenerator>("Plugin")
            .NamespacesConfig(new[] { "Invert.Core.GraphDesigner", "Invert.uFrame", "Invert.uFrame.Editor"})
            .BaseTypeConfig(new CodeTypeReference(typeof(DiagramPlugin)))
            .ClassNameConfig(node => string.Format("{0}Plugin", node.Name))
            .FilenameConfig(node => Path.Combine("Editor", string.Format("{0}Plugin.cs", node.Name)))
            .DesignerFilenameConfig(node => Path.Combine("_DesignerFiles", Path.Combine("Editor", "Plugin.cs")))
            .OverrideTypedMethod(typeof(DiagramPlugin), "Initialize", CreatePluginInitializeMethod)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellNodeTypeNode>(), CreateGraphConfigProperty)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellGraphTypeNode>(), CreateConfigProperty)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellGeneratorTypeNode>(), CreateGeneratorConfigProperty)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellChildItemTypeNode>().Where(x => x["Typed"]),
                    CreateGetSelectionCommandMethod
                )
            ;

      

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
                p => new CodeTypeReference(string.Format("GenericGraphData<{0}Node>", p.Name)), "Graphs")
            ;

        graphConfig.AddEditableClass<ShellGraphTypeNodeClassGenerator>("GraphNode")
            .BaseTypeConfig(new CodeTypeReference(typeof(GenericNode)))
            .ClassNameConfig(node => string.Format("{0}Node", node.Name))
            .FilenameConfig(node => Path.Combine("Editor", string.Format("{0}Node.cs", node.Name)))
            .DesignerFilenameConfig(node => Path.Combine("_DesignerFiles", Path.Combine("Editor", "Plugin.cs")));
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

    private static CodeTypeMember CreateViewModelConstructor(CodeTypeDeclaration decleration, IShellReferenceType data)
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
                _ => new CodeTypeReference(string.Format("GenericReferenceItem<{0}>", _.ReferenceType.ClassName)),
                "ReferenceItems")
            .OnlyIf(_ => _.ReferenceType != null)
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
                _ => new CodeTypeReference(string.Format("GenericNodeGenerator<{0}>", _.GeneratorFor.ClassName)),
                "Generators")
            .OnlyIf(_ => _.GeneratorFor != null)
            
            ;
    }

    private static void ConfigureShellSlotTypeGenerators(NodeConfig<ShellSlotTypeNode> shellInputReferenceConfig)
    {
        shellInputReferenceConfig.AddEditableClass<ShellSlotTypeNodeClassGenerator>("Plugin")
            .AsDerivedEditorEditableClass("{0}", "Plugin", _ => new CodeTypeReference(typeof(GenericSlot)), "Slots")
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

    private void ConfigureShellNodeGenerators(NodeConfig<ShellNodeTypeNode> shellNodeConfig)
    {
        shellNodeConfig.AddEditableClass<ShellNodeTypeClassGenerator>("Node")
          .AsDerivedEditorEditableClass("{0}Node", "Plugin", node => node["Inheritable"]
                       ? new CodeTypeReference(typeof(GenericInheritableNode))
                       : new CodeTypeReference(typeof(GenericNode)), "Nodes")
          .MembersPerChild<ShellNodeSectionsSlot>(GetNodeSectionProperty)
          .MembersPerChild<ShellNodeInputsSlot>(CreateInputSlotProperty)
          .MembersPerChild<ShellNodeOutputsSlot>(CreateOutputSlotProperty)
          .MembersFor<ShellNodeSectionsSlot>(p => p.Sections.Where(x => x.SourceItem is ShellNodeTypeReferenceSection), CreateReferenceSectionProperty)
          ;

        shellNodeConfig.AddEditableClass<ShellNodeTypeNodeViewModelGenerator>("NodeViewModelGenerator")
            .AsDerivedEditorEditableClass("{0}NodeViewModel", "Plugin",
                _ => new CodeTypeReference(string.Format("GenericNodeViewModel<{0}Node>", _.Name)), "Nodes")
            .Member(_ => CreateViewModelConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
            .OnlyIf(_ => _["Custom"])
            ;

        shellNodeConfig.AddEditableClass<ShellNodeTypeNodeDrawerGenerator>("NodeDrawerGenerator")
            .AsDerivedEditorEditableClass("{0}Drawer", "Plugin",
                _ => new CodeTypeReference(string.Format("GenericNodeDrawer<{0}Node, {0}NodeViewModel>", _.Name)), "Nodes")
            .Member(_ => CreateDrawerConstructor(_.Decleration, _.Data), MemberGeneratorLocation.Both)
            //.OverrideTypedMethod(typeof (DiagramNodeDrawer), "GetContentDrawers", null, true,
            //    MemberGeneratorLocation.EditableFile)
            .OnlyIf(_ => _["Custom"])
            ;
    }

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

    private static CodeTypeMember CreateDrawerConstructor(CodeTypeDeclaration decleration, IShellReferenceType data)
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

    private CodeTypeMember CreateViewModelConstructor(LambdaMemberGenerator<IShellReferenceType> arg1)
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
            Type = new CodeTypeReference(string.Format("IEnumerable<{0}>", referenceItem.ReferenceType.ClassName))
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
                    referenceItem.ReferenceType.ClassName)));
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
        var typeName = isReferenceItem ? arg1.Data.SourceItem.ClassName : arg1.Data.SourceItem.ReferenceType.ClassName;
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
        return new CodeMemberField(string.Format("NodeConfig<{0}Node>", arg1.Data.Name), arg1.Data.Name)
        {
            Attributes = MemberAttributes.Public
        };
    }

    private CodeTypeMember CreateConfigProperty(LambdaMemberGenerator<ShellGraphTypeNode> arg1)
    {
        return new CodeMemberField(string.Format("NodeConfig<{0}Node>", arg1.Data.Name), arg1.Data.Name)
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
        foreach (var itemType in shellPluginNode.Project.NodeItems.OfType<IShellReferenceType>().Where(p => p.IsValid))
        {
            if (itemType is ShellNodeTypeNode) continue;
            if (itemType is ShellNodeTypeSection && !(itemType is ShellNodeTypeReferenceSection)) continue;
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
        foreach (var graphType in shellPluginNode.Project.NodeItems.OfType<ShellGraphTypeNode>().Where(p => p.IsValid))
        {
            var varName = graphType.Name;
            method.Statements.Add(new CodeSnippetExpression(string.Format("{1} = container.AddGraph<{0}Graph, {0}Node>(\"{0}\")", graphType.Name, varName)));
            foreach (var item in graphType.SubNodes)
            {
                method.Statements.Add(
                    new CodeSnippetExpression(string.Format("{0}.HasSubNode<{1}Node>()", varName, item.Name)));
            }

        }
        foreach (var nodeType in shellPluginNode.Project.NodeItems.OfType<ShellNodeTypeNode>().Where(p => p.IsValid))
        {
            InitializeNodeType(method, nodeType);
        }

    }

    private static void InitializeNodeType(CodeMemberMethod method, ShellNodeTypeNode nodeType)
    {
        var varName = nodeType.Name;
        if (nodeType["Custom"])
        {
            method.Statements.Add(
           new CodeSnippetExpression(string.Format("{1} = container.AddNode<{0}Node,{0}NodeViewModel,{0}Drawer>(\"{0}\")", nodeType.Name, varName)));
        }
        else
        {
            method.Statements.Add(
              new CodeSnippetExpression(string.Format("{1} = container.AddNode<{0}Node>(\"{0}\")", nodeType.Name, varName)));
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
        foreach (var item in nodeType.InputSlots)
        {
            var source = item.SourceItem;
            method.Statements.Add(new CodeSnippetExpression(string.Format("{0}.Input<{1},{2}>(\"{3}\", {4})",
                varName, source.ReferenceType.ClassName,
                source.ClassName,
                item.Name,
                source.Flags.ContainsKey("Multiple") && source.Flags["Multiple"] ? "true" : "false"
                )));

        }
        foreach (var item in nodeType.OutputSlots)
        {
            var source = item.SourceItem;
            method.Statements.Add(new CodeSnippetExpression(string.Format("{0}.Output<{1},{2}>(\"{3}\", {4})",
                varName, source.ReferenceType.ClassName,
                source.ClassName,
                item.Name,
                source.Flags.ContainsKey("Multiple") && source.Flags["Multiple"] ? "true" : "false"
                )));
        }
        foreach (var item in nodeType.Sections)
        {
            var sectionItem = item.SourceItem;
            var referenceTypeSection = sectionItem as ShellNodeTypeReferenceSection;
            if (referenceTypeSection != null)
            {
                method.Statements.Add(
                    new CodeSnippetExpression(string.Format("{0}.ReferenceSection<{1},{3}>(\"{2}\",node=>node.Possible{2}, true)", varName, sectionItem.ReferenceType.ClassName, item.Name, sectionItem.ClassName))
                    );
            }
            else
            {
                if (sectionItem.ReferenceType.Flags.ContainsKey("Typed") && sectionItem.ReferenceType.Flags["Typed"])
                {
                    method.Statements.Add(
                           new CodeSnippetExpression(string.Format("{0}.TypedSection<{1}>(\"{2}\", Get{3}SelectionCommand())", varName, sectionItem.ReferenceType.ClassName, item.Name, sectionItem.ReferenceType.Name))
                       );

                }
                else
                {
                    method.Statements.Add(
                        new CodeSnippetExpression(string.Format("{0}.Section<{1}>(\"{2}\",node=>node.{2}, true)", varName, sectionItem.ReferenceType.ClassName, item.Name))
                    );
                }

            }

        }

        foreach (var item in nodeType.Generators)
        {
            if (item.IsDesignerOnly)
            {
                method.Statements.Add(
                    new CodeSnippetExpression(string.Format("{0}Config = {1}.AddDesignerOnlyClass<{0}Generator>(\"{0}\")", item.Name,
                        varName)));

                //AsDerivedEditorEditableClass("{0}Graph", "Plugin",
//                p => new CodeTypeReference(string.Format("GenericGraphData<{0}Node>", p.Name)), "Graphs")
  //          ;
               // .method.Add("{0}Config.AsDerivedEditableClass()")
            }
            else
            {
                method.Statements.Add(
                    new CodeSnippetExpression(string.Format("{0}Config = {1}.AddEditableClass<{0}Generator>(\"{0}\")", item.Name,
                        varName)));
            }

        }
    }
}