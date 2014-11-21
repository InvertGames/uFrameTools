using System.Reflection;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;

public class ShellMemberNodeViewModel : GenericNodeViewModel<ShellMemberGeneratorNode>
{
    public ShellMemberNodeViewModel(ShellMemberGeneratorNode graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {

    }

    public override NodeConfig<ShellMemberGeneratorNode> NodeConfig
    {
        get { return InvertGraphEditor.Container.Resolve<NodeConfig>(DataObject.GetType().Name) as NodeConfig<ShellMemberGeneratorNode>; }
    }

    protected override void CreateContent()
    {
        base.CreateContent();

    }
}