using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class ImportCommand : EditorCommand<DiagramNodeViewModel>, IDiagramContextCommand
    {
        public override void Perform(DiagramNodeViewModel node)
        {

        }

        public override string CanPerform(DiagramNodeViewModel node)
        {
            //if (node.ExportGraphType == null) return "This node is not exportable.";
            if (node.IsLocal) return "Node must be external to import it.";
            if (node.GraphItemObject is IDiagramFilter) return null;
            return "Node must be a filter to export it.";
        }
    }
}