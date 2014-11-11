using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
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

        public override void Perform(DiagramNodeViewModel node)
        {
            node.BeginEditing();
        }

        public override string CanPerform(DiagramNodeViewModel node)
        {
            if (node == null) return "Invalid argument";
            if (!node.IsLocal) return "Can't rename a node when its not local.";
            return null;
        }


    }
}