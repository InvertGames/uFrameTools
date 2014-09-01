namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class MoveUpCommand : EditorCommand<IDiagramNodeItem>, IDiagramNodeItemCommand, IKeyBindable
    {
        public override string Name
        {
            get { return "Move Up"; }
        }
        public override void Perform(IDiagramNodeItem node)
        {
            node.Node.MoveItemUp(node);
        }

        public override string CanPerform(IDiagramNodeItem node)
        {
            if (node != null && node.Node != null) return null;
            return "Can't move item.";
        }
    }
}