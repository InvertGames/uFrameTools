using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class ExportCommand : EditorCommand<DiagramNodeViewModel>, IDiagramNodeCommand
    {
        public override void Perform(DiagramNodeViewModel node)
        {
            var diagramViewModel = node.DiagramViewModel;
            var nodeData = node.GraphItemObject as IDiagramNode;
            diagramViewModel.DiagramData.RemoveNode(nodeData);

            var exportedDiagram = node.DiagramViewModel.CurrentRepository.CreateNewDiagram(node.ExportGraphType, nodeData as IDiagramFilter);
            var repository = diagramViewModel.CurrentRepository;

            

        }

        public override string CanPerform(DiagramNodeViewModel node)
        {
            if (!node.IsLocal) return "Node must be local to export it.";
            if (node.GraphItemObject is IDiagramFilter) return null;
            if (node.ExportGraphType == null) return null;
            return "Node must be a filter to export it.";
        }
    }
}