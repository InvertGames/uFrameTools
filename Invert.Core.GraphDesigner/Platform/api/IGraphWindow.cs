namespace Invert.Core.GraphDesigner
{
    public interface IGraphWindow : ICommandHandler
    {
        DiagramViewModel DiagramViewModel { get; }
        float Scale { get; set; }
        void RefreshContent();
    }
}