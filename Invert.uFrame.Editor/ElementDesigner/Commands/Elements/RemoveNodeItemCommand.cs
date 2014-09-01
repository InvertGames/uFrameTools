namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class RemoveNodeItemCommand : EditorCommand<ElementsDiagram>, IDiagramNodeItemCommand
    {
        public override string Name
        {
            get { return "Remove Item"; }
        }


        public override bool IsChecked(ElementsDiagram arg)
        {
            return false;
        }

        public override void Perform(ElementsDiagram arg)
        {
 
            var diagramNodeItem = arg.SelectedItem as IDiagramNodeItem;
            if (diagramNodeItem != null)
                diagramNodeItem.Remove(arg.SelectedData);
        }

        public override string CanPerform(ElementsDiagram node)
        {
            return null;
        }
    }
}