using Invert.uFrame.Editor.ViewModels;

namespace Invert.Core.GraphDesigner
{
    public class DeleteItemCommand : EditorCommand<DiagramNodeItem>, IDiagramNodeItemCommand, IKeyBindable
    {
        public override string Name
        {
            get { return "Delete"; }
        }

        public override void Perform(DiagramNodeItem node)
        {
            InvertApplication.Log("Deleting Item");
            if (node == null) return;
          
            var project = node.Node.Graph.Project;
            project.RemoveItem(node);
     
        }

        public override string CanPerform(DiagramNodeItem node)
        {
            return null;
        }
    }
}