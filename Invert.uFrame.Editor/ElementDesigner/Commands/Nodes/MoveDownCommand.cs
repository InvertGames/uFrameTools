namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class MoveDownCommand : EditorCommand<IDiagramNodeItem>, IDiagramNodeItemCommand, IKeyBindable
    {
        public override string Name
        {
            get { return "Move Down"; }
        }

        public override void Perform(IDiagramNodeItem node)
        {
            node.Node.MoveItemDown(node);
        }

        public override string CanPerform(IDiagramNodeItem node)
        {
            if (node != null && node.Node != null) return null;
            return "Can't move item.";
        }
    }
}