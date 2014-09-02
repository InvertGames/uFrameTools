using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class RemoveNodeItemCommand : EditorCommand<DiagramViewModel>, IDiagramNodeItemCommand
    {
        public override string Name
        {
            get { return "Remove Item"; }
        }


        public override bool IsChecked(DiagramViewModel arg)
        {
            return false;
        }

        public override void Perform(DiagramViewModel arg)
        {
            var diagramNodeItem = arg.SelectedNodeItem as ItemViewModel;
            if (diagramNodeItem != null)
                diagramNodeItem.Remove();
        }

        public override string CanPerform(DiagramViewModel node)
        {
            return null;
        }
    }
}