using System.Linq;

public static class DataExtensions
{
    public static GraphData GetDiagram(this IDiagramNode node, IProjectRepository project)
    {
        return project.Diagrams.FirstOrDefault(p => p.NodeItems.Contains(node));
    }
    public static GraphData GetDiagram(this IDiagramNode node)
    {
        return ((IProjectRepository) node.Project).Diagrams.FirstOrDefault(p => p.NodeItems.Contains(node));
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