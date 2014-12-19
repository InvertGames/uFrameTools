using Invert.uFrame.Editor.ViewModels;

namespace Invert.Core.GraphDesigner
{
    public class MoveDownCommand : EditorCommand<ItemViewModel>, IDiagramNodeItemCommand, IKeyBindable
    {
        public override string Name
        {
            get { return "Move Down"; }
        }

        public override void Perform(ItemViewModel node)
        {
            node.NodeItem.Node.MoveItemDown(node.NodeItem);
        }

        public override string CanPerform(ItemViewModel node)
        {
            if (node != null && node.NodeItem != null) return null;
            return "Can't move item.";
        }
    }
}