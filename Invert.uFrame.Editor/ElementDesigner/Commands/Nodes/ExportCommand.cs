using System.Collections.Generic;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class ExportCommand : EditorCommand<DiagramNodeViewModel>, IDiagramNodeCommand
    {
        public override string Group
        {
            get { return "Moving"; }
        }
        public override void Perform(DiagramNodeViewModel node)
        {
            var diagramViewModel = node.DiagramViewModel;
            var nodeData = node.GraphItemObject as IDiagramNode;
            var repository = diagramViewModel.CurrentRepository;
            var filter = nodeData as IDiagramFilter;
            repository.ExportNode(node.ExportGraphType,diagramViewModel.DiagramData,node.DataObject as IDiagramNode,true);

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