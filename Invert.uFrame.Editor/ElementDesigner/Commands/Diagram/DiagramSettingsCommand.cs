namespace Invert.uFrame.Editor.ElementDesigner
{
    public class DiagramSettingsCommand : ElementsDiagramToolbarCommand
    {
        public override string Name
        {
            get { return "Settings"; }
        }

        public override void Perform(DiagramViewModel node)
        {
            ElementDiagramSettingsWindow.ShowWindow(node);
        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.BottomRight; }
        }
    }
}