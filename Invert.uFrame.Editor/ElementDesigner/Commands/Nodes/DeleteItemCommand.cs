using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class DeleteItemCommand : EditorCommand<ItemViewModel>, IDiagramNodeItemCommand, IKeyBindable
    {
        public override string Name
        {
            get { return "Delete"; }
        }

        public override void Perform(ItemViewModel node)
        {
            if (node == null) return;
            uFrameEditor.CurrentProject.RemoveItem(node.NodeItem);
        }

        public override string CanPerform(ItemViewModel node)
        {
            return null;
        }
    }
}