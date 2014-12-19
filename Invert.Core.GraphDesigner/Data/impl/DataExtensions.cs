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
    public static ICodePathStrategy GetPathStrategy(this IDiagramNode node, IProjectRepository project)
    {
        return GetDiagram(node,project).CodePathStrategy;
    }
    public static ICodePathStrategy GetPathStrategy(this IDiagramNode node)
    {
        return GetDiagram(node, node.Project as IProjectRepository).CodePathStrategy;
    }

    public static string GetAssetPath(this IDiagramNode node, IProjectRepository project)
    {
        return GetDiagram(node, project).CodePathStrategy.AssetPath;
    }

    public static string GetAssetPath(this IDiagramNode node)
    {
        return GetDiagram(node, node.Project as IProjectRepository).CodePathStrategy.AssetPath;
    }
}