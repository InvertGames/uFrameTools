using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class MoveUpCommand : EditorCommand<ItemViewModel>, IDiagramNodeItemCommand, IKeyBindable
    {
        public override string Name
        {
            get { return "Move Up"; }
        }
        public override void Perform(ItemViewModel node)
        {
            node.NodeItem.Node.MoveItemUp(node.NodeItem);
        }

        public override string CanPerform(ItemViewModel node)
        {
            if (node != null && node.NodeItem != null) return null;
            return "Can't move item.";
        }
    }
}