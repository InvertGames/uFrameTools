using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Invert.Core.GraphDesigner;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;

public class PluginDesigner : DiagramPlugin
{
    public override void Initialize(uFrameContainer container)
    {
        container.RegisterInstance<IDiagramNodeCommand>(new SelectColorCommand(), "SelectColor");

        var pluginConfig = container
            .AddGraph<PluginGraph, PluginGraphNode>("Shell Plugin")
            .Color(NodeColor.Green)

            .HasSubNode<ShellNodeTypeNode>()
            //.HasSubNode<ShellNodeChildTypeNode>()
            .HasSubNode<ShellGraphTypeNode>()
            .HasSubNode<ShellGenericReferenceItem>()
            .HasSubNode<ShellGenericNodeChildItem>()
            .HasSubNode<ShellGenericInputReference>()
            .HasSubNode<ShellGenericOutputReference>()
          
            
            ;
        
        var graphConfig = container.AddNode<ShellGraphTypeNode>("Graph Type")
            //.OutputAlias("Sub Nodes")
           .Output<ShellNodeTypeNode, SubNodeOutput>("Sub Nodes",true)
           .Output<ShellCodeGeneratorNode, GeneratorChannel>("Generators", true)
           .Color(NodeColor.DarkDarkGray)
           ;

        var shellNodeConfig = container.AddNode<ShellNodeTypeNode>("Node Type")
            //.OutputAlias("Derived Nodes")
            .Output<ShellNodeTypeNode, SubNodeOutput>("Sub Nodes",true)
            .Output<ShellCodeGeneratorNode,GeneratorChannel>("Generators", true)
            //.InputAlias("Base Node")
            .AddFlag("Custom")
            .HasSubNode<ShellGenericInputReference>()
            .HasSubNode<ShellGenericOutputReference>()
            .HasSubNode<ShellGenericReferenceItem>()
            .HasSubNode<ShellGenericNodeChildItem>()
              .HasSubNode<ShellCodeGeneratorNode>()
            .Inheritable()
            .AddFlag("Inheritable")
            //.Input<ShellGraphTypeNode>("Graph")
            .ColorConfig(_ =>
            {
                if (!string.IsNullOrEmpty(_.DataBag["Color"]))
                {
                    var parsed = (NodeColor)Enum.Parse(typeof (NodeColor), _.DataBag["Color"], true);
                    return parsed;
                }
                return NodeColor.Gray;
            })
            ;

        var shellInputReferenceConfig = container.AddNode<ShellGenericInputReference>("IO Connector")
            .Color(NodeColor.Black)
            .Inheritable()
            .AddEditableClass<ShellGenericInputReferenceClassGenerator>("Plugin")
            .AsDerivedEditorEditableClass("{0}","Plugin",_=>new CodeTypeReference(typeof(GenericConnectionReference)),"Connectors")
            ;

        var shellCodeGeneratorConfig = container.AddNode<ShellCodeGeneratorNode>("Code Generator")
            .Color(NodeColor.Purple)
            .AddFlag("Designer Only")
            .AddEditableClass<ShellCodeGeneratorNodeGenerator>("Plugin")
            .AsDerivedEditorEditableClass("{0}", "Plugin", _ => new CodeTypeReference(string.Format("NodeCodeGenerator<{0}>", _.GeneratorFor.ClassName)), "Generators")
            .OnlyIf(_=>_.GeneratorFor != null)
            ;

        var shellReferenceConfig = container.AddNode<ShellGenericReferenceItem>("Reference Section Item")
            .Color(NodeColor.Yellow)
            .AddEditableClass<ShellGenericReferenceItemClassGenerator>("Plugin")
            .AsDerivedEditorEditableClass("{0}Reference", "Plugin", _ => new CodeTypeReference(typeof(GenericReferenceItem)),"ReferenceItems")
            ;

        var shellChildItemConfig = container.AddNode<ShellGenericNodeChildItem>("Child Item")
            .Color(NodeColor.YellowGreen)
            .AddEditableClass<ShellGenericNodeChildItemClassGenerator>("Plugin")
            .AsDerivedEditorEditableClass("{0}ChildItem", "Plugin", _ => new CodeTypeReference(typeof(GenericNodeChildItem)), "ChildItems")
            ;


        shellNodeConfig.AddEditableClass<ShellNodeTypeClassGenerator>("Node")
            .AsDerivedEditorEditableClass("{0}Node", "Plugin", node => node["Inheritable"]
                         ? new CodeTypeReference(typeof(GenericInheritableNode))
                         : new CodeTypeReference(typeof(GenericNode)),"Nodes")
            ;

        pluginConfig
            .AddEditableClass<ShellPluginClassGenerator>("Plugin")
            .NamespacesConfig(new[] { "Invert.Core.GraphDesigner", "Invert.uFrame", "Invert.uFrame.Editor" })
            .BaseTypeConfig(new CodeTypeReference(typeof(DiagramPlugin)))
            .ClassNameConfig(node => string.Format("{0}Plugin", node.Name))
            .FilenameConfig(node => Path.Combine("Editor", string.Format("{0}Plugin.cs", node.Name)))
            .DesignerFilenameConfig(node => Path.Combine("_DesignerFiles", Path.Combine("Editor", "Plugin.cs")))
            .OverrideTypedMethod(typeof(DiagramPlugin), "Initialize", CreatePluginInitializeMethod)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellNodeTypeNode>(), CreateGraphConfigProperty)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellGraphTypeNode>(), CreateConfigProperty)
            .MembersFor(p => p.Project.NodeItems.OfType<ShellCodeGeneratorNode>(), CreateGeneratorConfigProperty)
            ;

        graphConfig
            .AddEditableClass<ShellGraphClassGenerator>("Graph")
            .AsDerivedEditorEditableClass("{0}Graph", "Plugin",
                p => new CodeTypeReference(string.Format("GenericGraphData<{0}Node>", p.Name)),"Graphs")
            ;

        graphConfig.AddEditableClass<ShellGraphTypeNodeClassGenerator>("GraphNode")
            .BaseTypeConfig(new CodeTypeReference(typeof(GenericNode)))
            .ClassNameConfig(node => string.Format("{0}Node", node.Name))
            .FilenameConfig(node => Path.Combine("Editor", string.Format("{0}Node.cs", node.Name)))
            .DesignerFilenameConfig(node => Path.Combine("_DesignerFiles", Path.Combine("Editor", "Plugin.cs")));

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
    private CodeTypeMember CreateGeneratorConfigProperty(LambdaMemberGenerator<ShellCodeGeneratorNode> arg1)
    {
        return new CodeMemberField(string.Format("NodeGeneratorConfig<{0}>", arg1.Data.GeneratorFor.ClassName), arg1.Data.Name + "Config")
        {
            Attributes = MemberAttributes.Public
        };
    }
    private void CreatePluginInitializeMethod(PluginGraphNode pluginNode, CodeMemberMethod method)
    {
        foreach (var graphType in pluginNode.Project.NodeItems.OfType<ShellGraphTypeNode>())
        {
            var varName = graphType.Name;
            method.Statements.Add(new CodeSnippetExpression(string.Format("{1} = container.AddGraph<{0}Graph, {0}Node>(\"{0}\")", graphType.Name, varName)));
            foreach (var item in graphType.SubNodes)
            {
                method.Statements.Add(
                    new CodeSnippetExpression(string.Format("{0}.HasSubNode<{1}Node>()", varName, item.Name)));
            }

        }
        foreach (var nodeType in pluginNode.Project.NodeItems.OfType<ShellNodeTypeNode>())
        {
            var varName = nodeType.Name;
            method.Statements.Add(new CodeSnippetExpression(string.Format("{1} = container.AddNode<{0}Node>(\"{0}\")", nodeType.Name, varName)));
            if (nodeType["Inheritable"])
            {
                method.Statements.Add(new CodeSnippetExpression(string.Format("{0}.Inheritable()", varName)));
            }
            if (!string.IsNullOrEmpty(nodeType.DataBag["Color"]))
            {
                method.Statements.Add(new CodeSnippetExpression(string.Format("{0}.Color(NodeColor.{1})", varName,nodeType.DataBag["Color"])));
            }
            
         
            foreach (var item in nodeType.SubNodes)
            {
                method.Statements.Add(
                    new CodeSnippetExpression(string.Format("{0}.HasSubNode<{1}Node>()", varName, item.Name)));
            }

            foreach (var item in nodeType.Generators)
            {
                if (item["Designer Only"])
                {
                    method.Statements.Add(
                  new CodeSnippetExpression(string.Format("{0}Config = {1}.AddDesignerOnlyClass<{0}>(\"{0}\")", item.Name, varName)));
                }
                else
                {
                    method.Statements.Add(
                    new CodeSnippetExpression(string.Format("{0}Config = {1}.AddEditableClass<{0}>(\"{0}\")", item.Name, varName)));
                }
                
            }
        }

    }
}
public class GeneratorChannel : GenericConnectionReference
{

}
public class SubNodeOutput : GenericConnectionReference
{

}
public class ShellNodeTypeClassGenerator : NodeCodeGenerator<ShellNodeTypeNode>
{

}
public class ShellPluginClassGenerator : NodeCodeGenerator<PluginGraphNode>
{

}
public class ShellGraphClassGenerator : NodeCodeGenerator<ShellGraphTypeNode>
{

}
public class ShellGraphTypeNodeClassGenerator : NodeCodeGenerator<ShellGraphTypeNode>
{

}


public class ShellGenericNodeChildItemClassGenerator : NodeCodeGenerator<ShellGenericNodeChildItem> { }
public class ShellGenericReferenceItemClassGenerator : NodeCodeGenerator<ShellGenericReferenceItem> { }

public class ShellGenericInputReferenceClassGenerator : NodeCodeGenerator<ShellGenericInputReference> { }
public class ShellGenericOutputReferenceClassGenerator : NodeCodeGenerator<ShellGenericOutputReference> { }

public class ShellGenericNodeChildItem : GenericInheritableNode { }
public class ShellGenericReferenceItem : GenericInheritableNode { }

public class ShellGenericInputReference : GenericInheritableNode { }
public class ShellGenericOutputReference : GenericNode { }

public class ShellGraphTypeNode : ShellNodeTypeNode
{

}
public class PluginGraphNode : GenericNode { }

public class ShellNodeTypeInput : GenericNodeChildItem { }
public class ShellNodeTypeOutput : GenericNodeChildItem { }

public class ShellNodeTypeNode : GenericInheritableNode
{
    public IEnumerable<ShellNodeTypeNode> SubNodes
    {
        get
        {
            return GetConnectionReference<SubNodeOutput>().OutputsTo<ShellNodeTypeNode>();
        }
    }
    public IEnumerable<ShellCodeGeneratorNode> Generators
    {
        get
        {
            return GetConnectionReference<GeneratorChannel>().OutputsTo<ShellCodeGeneratorNode>();
        }
    }
    public virtual string ClassName
    {
        get { return Name + "Node"; }
    }
}
public class ShellNodeChildTypeNode : GenericNode { }

public class SelectColorCommand : EditorCommand<ShellNodeTypeNode>, IDynamicOptionsCommand
{
    public override void Perform(ShellNodeTypeNode node)
    {
        node.DataBag["Color"] = SelectedOption.Value.ToString();
    }

    public override string CanPerform(ShellNodeTypeNode node)
    {
        if (node == null) return "Invalid node type";
        return null;
    }

    public IEnumerable<UFContextMenuItem> GetOptions(object item)
    {
        var node = item as ShellNodeTypeNode;
        if (node == null)
        {
            yield break;
        }
        var names = Enum.GetNames(typeof (NodeColor));
        foreach (var name in names)
        {
            yield return new UFContextMenuItem()
            {
                Checked = node.DataBag["Color"] == name,
                Value = name,
                Name = "Color/" + name
            };
        }
    }

    public UFContextMenuItem SelectedOption { get; set; }
    public MultiOptionType OptionsType { get; private set; }
}
public class ShellCodeGeneratorNodeGenerator : NodeCodeGenerator<ShellCodeGeneratorNode> { }
public class ShellCodeGeneratorNode : GenericInheritableNode
{
    public GeneratorChannel ShellNodeGeneratorChannel
    {
        get
        {
            return this.InputFrom<GeneratorChannel>();
        }
    }
    public ShellNodeTypeNode GeneratorFor
    {
        get
        {
            var channel = ShellNodeGeneratorChannel;
            if (channel == null) return null;
            return channel.Node as ShellNodeTypeNode;
        }
    }
}
