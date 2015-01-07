namespace Invert.Core.GraphDesigner
{
    public class GraphService : IFeature
    {
        public ProjectService ProjectService { get; set; }

        public GraphService(ProjectService projectService)
        {
            ProjectService = projectService;
        }

        public void AddItem(IGraphData data, IGraphItem item)
        {
            
        }
        public void LoadGraph(IGraphData graph)
        {
            
        }
    }
}