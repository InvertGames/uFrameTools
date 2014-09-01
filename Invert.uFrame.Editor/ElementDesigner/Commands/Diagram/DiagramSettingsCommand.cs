namespace Invert.uFrame.Editor.ElementDesigner
{
    public class DiagramSettingsCommand : ElementsDiagramToolbarCommand
    {
        

        public override void Perform(ElementsDiagram node)
        {
            ElementDiagramSettingsWindow.ShowWindow(node.Data);
        }
    }
}