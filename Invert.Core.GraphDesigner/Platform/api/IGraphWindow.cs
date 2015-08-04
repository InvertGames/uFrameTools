using Invert.Core.GraphDesigner.Two;

namespace Invert.Core.GraphDesigner
{
    public interface IGraphWindow : ICommandHandler
    {
        DiagramViewModel DiagramViewModel { get; }
        float Scale { get; set; }
        DiagramDrawer DiagramDrawer { get; set; }
        void RefreshContent();
        void ProjectChanged(Workspace project);
    }
}