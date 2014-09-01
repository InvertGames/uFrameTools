namespace Invert.uFrame.Editor.ElementDesigner
{
    public abstract class ElementsDiagramToolbarCommand : ToolbarCommand<ElementsDiagram>
    {
        public override string CanPerform(ElementsDiagram node)
        {
            if (node == null) return "No Diagram Open";
            return null;
        }
    }
}