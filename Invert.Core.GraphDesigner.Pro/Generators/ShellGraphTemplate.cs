using System.CodeDom;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;

[TemplateClass("Graphs", MemberGeneratorLocation.Both, ClassNameFormat = "{0}Graph", IsEditorExtension = true)]
#if UNITY_DLL
public class ShellGraphTemplate : UnityGraphData<GenericGraphData<GenericNode>>,IClassTemplate<ShellGraphTypeNode>
#else
public class ShellGraphTemplate : GenericGraphData<GenericNode>, IClassTemplate<ShellGraphTypeNode>
#endif
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.IsDesignerFile)
        {
#if UNITY_DLL
            Ctx.SetBaseTypeArgument("GenericGraphData<{0}>",Ctx.Data.RootNode.ClassName);
#else
            Ctx.SetType("GenericGraphData<{0}>", Ctx.Data.RootNode.ClassName);
#endif
        }
    }

    public TemplateContext<ShellGraphTypeNode> Ctx { get; set; }
}


[TemplateClass("Plugins", MemberGeneratorLocation.Both, ClassNameFormat = "{0}", IsEditorExtension = true)]
public class ShellPluginTemplate : DiagramPlugin, IClassTemplate<ShellPluginNode>
{
    #region Template Setup
    public void TemplateSetup()
    {
        Ctx.AddIterator("NodeConfigProperty", _ => _.Graph.NodeItems.OfType<ShellNodeTypeNode>());
        Ctx.AddIterator("GetSelectionCommand", _ => _.Graph.NodeItems.OfType<ShellChildItemTypeNode>().Where(x => x["Typed"]));
    }

    [TemplateMethod("Get{0}SelectionCommand",MemberGeneratorLocation.Both, true)]
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

    [TemplateProperty("{0}",AutoFillType.NameAndTypeWithBackingField)]
    public NodeConfig<GenericNode> NodeConfigProperty {
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

    [TemplateMethod(MemberGeneratorLocation.Both,true)]
    public override void Initialize(uFrameContainer container)
    {
        if (!Ctx.IsDesignerFile) return;
        Ctx.CurrentMethodAttribute.CallBase = false;
        var method = Ctx.CurrentMethod;
        foreach (var item in Ctx.Data.Graph.NodeItems.OfType<ShellChildItemTypeNode>())
        {
            if (!item["Typed"]) continue;
            method._(
                "container.RegisterInstance<IEditorCommand>(Get{0}SelectionCommand(), typeof({1}).Name + \"TypeSelection\");", item.Name, item.ClassName);

        }
        foreach (var itemType in Ctx.Data.Graph.NodeItems.OfType<IShellNode>().Where(p => p.IsValid))
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
        var graphTypes = Ctx.Data.Graph.NodeItems.OfType<ShellGraphTypeNode>().Where(p => p.IsValid).ToArray();
        foreach (var nodeType in Ctx.Data.Graph.NodeItems.OfType<ShellNodeTypeNode>().Where(p => p.IsValid))
        {
            InitializeNodeType(method, nodeType, graphTypes.FirstOrDefault(p => p.RootNode == nodeType));
        }

        foreach (var nodeType in Ctx.Data.Graph.NodeItems.OfType<IShellConnectable>().Where(p => p.IsValid))
        {
            foreach (var item in nodeType.ConnectableTo)
            {
                method._("container.Connectable<{0},{1}>()", nodeType.ClassName, item.SourceItem.ClassName);
            }

        }
        foreach (var nodeType in Ctx.Data.Graph.NodeItems.OfType<IReferenceNode>().Where(p => p.IsValid))
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

    }
    
}