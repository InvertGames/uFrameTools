using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{

    public class HideCommand : EditorCommand<DiagramNodeViewModel>, IDiagramNodeCommand
    {
        public override string Group
        {
            get { return "File"; }
        }
        public override void Perform(DiagramNodeViewModel node)
        {
            node.Hide();
        }

        public override string CanPerform(DiagramNodeViewModel node)
        {
            if (node == null) return "Diagram Item must not be null.";
            return null;
        }
    }
}