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
            var repo = node.NodeViewModel.DiagramViewModel.CurrentRepository;
            repo.RemoveItem(node.NodeItem);
        }

        public override string CanPerform(ItemViewModel node)
        {
            return null;
        }
    }
}