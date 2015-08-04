namespace Invert.Core.GraphDesigner
{
    public class NodeHelpCommand : EditorCommand<DiagramNode>, IDiagramNodeCommand
    {
        public override string Name
        {
            get { return "Help and Documentation"; }
        }

        public override string Path
        {
            get { return "Help and Documentation"; }
        }

        public override void Perform(DiagramNode node)
        {
            // TODO 2.0 This was confusing
            //InvertGraphEditor.WindowManager.ShowHelpWindow(node.Project.CurrentGraph.RootFilter.GetType().Name, node.GetType());
        }

        public override string CanPerform(DiagramNode node)
        {
            if (node == null)
            {
                return "Show a diagram first.";
            }
            return null;
        }
    }
}