using Invert.uFrame.Editor.ViewModels;

namespace Invert.Core.GraphDesigner
{
    public class RenameCommand : EditorCommand<DiagramNodeViewModel>, IDiagramNodeCommand
    {
        public override string Group
        {
            get { return "File"; }
        }

        public override decimal Order
        {
            get { return -1; }
        }

        public override bool CanProcessMultiple
        {
            get { return false; }
        }

        public override void Perform(DiagramNodeViewModel node)
        {
            node.BeginEditing();
        }

        public override string CanPerform(DiagramNodeViewModel node)
        {
            var selected = node.DataObject as IDiagramNode;
            if (selected == null) return "Invalid argument";
            if (!node.IsLocal) return "Can't rename a node when its not local.";
            if (selected.Graph.Identifier != InvertGraphEditor.CurrentDiagramViewModel.GraphData.Identifier)
                return "Must be local to rename.";
            return null;
        }


    }
}