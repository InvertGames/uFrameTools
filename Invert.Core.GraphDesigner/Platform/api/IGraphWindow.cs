namespace Invert.Core.GraphDesigner
{
    public interface IGraphWindow : ICommandHandler
    {
        DiagramViewModel DiagramViewModel { get; }
        float Scale { get; set; }
        DiagramDrawer DiagramDrawer { get; set; }
        void RefreshContent();
        void ProjectChanged(IProjectRepository project);
    }
}