public interface IDiagramLink
{
    ISelectable Source { get; }

    ISelectable Target { get; }

    void Draw(DiagramViewModel diagram);

    void DrawPoints(DiagramViewModel diagram);
}