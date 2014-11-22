using Invert.uFrame.Editor.ViewModels;

namespace Invert.Core.GraphDesigner
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
            InvertGraphEditor.CurrentProject.RemoveItem(node.NodeItem);
        }

        public override string CanPerform(ItemViewModel node)
        {
            return null;
        }
    }
}