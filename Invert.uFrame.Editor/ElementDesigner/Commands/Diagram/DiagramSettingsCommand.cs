namespace Invert.uFrame.Editor.ElementDesigner
{
    public class DiagramSettingsCommand : ElementsDiagramToolbarCommand
    {
        

        public override void Perform(DiagramViewModel node)
        {
            ElementDiagramSettingsWindow.ShowWindow(node);
        }
    }
}