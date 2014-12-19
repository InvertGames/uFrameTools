using System;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public static class ProjectUtil
    {
        public static void PushNode(this IGraphData sourceDiagram, IGraphData targetDiagram, IDiagramNode node, bool addToRootFilter = false)
        {
            var position = sourceDiagram.PositionData[sourceDiagram.CurrentFilter, node.Identifier];
            sourceDiagram.RemoveNode(node);
            sourceDiagram.PositionData[sourceDiagram.CurrentFilter, node.Identifier] = position;
            targetDiagram.AddNode(node);
            if (addToRootFilter)
            {
                targetDiagram.PositionData[targetDiagram.RootFilter, node.Identifier] = position;
            }
        }

        public static void ExportNode(this IProjectRepository project, Type diagramType, IGraphData sourceDiagram, IDiagramNode node, bool moveChildren)
        {

            var filter = node as IDiagramFilter;
            if (filter == null) return;
            var location = project.GetItemLocation(node);

            // Create the diagram
            IGraphData targetDiagram = project.CreateNewDiagram(diagramType, node as IDiagramFilter);
            // Remove it from the source
            sourceDiagram.RemoveNode(node);
            // Add the positions
            project.SetItemLocation(node, location);

            // Move all of the nodes position data
            var allNodes = filter.GetContainingNodesResursive(project).ToArray();
            foreach (var diagramNode in allNodes)
            {
                foreach (var positionData in sourceDiagram.PositionData.Positions)
                {
                    if (diagramNode.Identifier == positionData.Key)
                        targetDiagram.PositionData.Positions.Add(positionData.Key, positionData.Value);
                }
            }


        }

    }
}