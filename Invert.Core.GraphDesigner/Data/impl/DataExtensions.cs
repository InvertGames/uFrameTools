using System.Linq;
using Invert.Core.GraphDesigner;

public static class DataExtensions
{
    public static IGraphData GetDiagram(this IDiagramNode node, IProjectRepository project)
    {
        return project.Graphs.FirstOrDefault(p => p.NodeItems.Contains(node));
    }
    public static IGraphData GetDiagram(this IDiagramNode node)
    {
        return ((IProjectRepository)node.Project).Graphs.FirstOrDefault(p => p.NodeItems.Contains(node));
    }
}