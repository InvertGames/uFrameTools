using System;
using System.Collections.Generic;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public static class FilterExtensions
    {
        public static IEnumerable<IDiagramNode> GetContainingNodesInProject(this IDiagramFilter filter, IProjectRepository repository)
        {

            foreach (var item in repository.Graphs)
            {
                var positionData = item.PositionData;

                FilterLocations locations;
                if (positionData.Positions.TryGetValue(filter.Identifier, out locations))
                {
                    foreach (var node in repository.NodeItems)
                    {
                        if (locations.Keys.Contains(node.Identifier))
                        {
                            yield return node;
                        }
                    }
                }

            }
            //foreach (var node in repository.NodeItems)
            //{
            //    if (node == filter) continue;
            //    var nodeAsFilter = node as IDiagramFilter;

            //    foreach (var item in repository.Graphs)
            //    {
            //        if (item.PositionData.HasPosition(filter, node))
            //        {
            //            yield return node;
            //        }
            //    }
            //}
            //return repository.NodeItems.Where(node => node != filter && repository.PositionData.HasPosition(filter, node));
        }
        public static IEnumerable<IDiagramNode> GetContainingNodes(this IDiagramFilter filter, INodeRepository repository)
        {

            return repository.NodeItems.Where(node => node != filter && repository.PositionData.HasPosition(filter, node));
        }

        public static IEnumerable<IDiagramNode> GetParentNodes(this IDiagramNode node)
        {
            foreach (var item in node.Project.PositionData.Positions)
            {
                if (item.Value.Keys.Contains(node.Identifier))
                {
                    yield return node.Project.NodeItems.FirstOrDefault(p => p.Identifier == item.Key);
                }
            }
        }
        public static IEnumerable<IDiagramNode> GetContainingNodesResursive(this IDiagramFilter filter, INodeRepository repository)
        {
            foreach (var item in filter.GetContainingNodes(repository))
            {
                yield return item;
                if (item is IDiagramFilter)
                {
                    var result = GetContainingNodesResursive(item as IDiagramFilter, repository);
                    foreach (var subItem in result)
                        yield return subItem;

                }
            }
        }
        public static bool IsAllowed(this IDiagramFilter filter, object item, Type t)
        {

            if (filter == item) return true;

            if (!AllowedFilterNodes.ContainsKey(filter.GetType())) return false;

            foreach (var x in AllowedFilterNodes[filter.GetType()])
            {
                if (t.IsAssignableFrom(x)) return true;
            }
            return false;
            // return InvertGraphEditor.AllowedFilterNodes[filter.GetType()].Contains(t);
        }

        public static bool IsItemAllowed(this IDiagramFilter filter, object item, Type t)
        {
            return true;
            //return uFrameEditor.AllowedFilterItems[filter.GetType()].Contains(t);
        }

        private static Dictionary<Type, List<Type>> _allowedFilterItems;

        private static Dictionary<Type, List<Type>> _allowedFilterNodes;

        public static Dictionary<Type, List<Type>> AllowedFilterItems
        {
            get { return _allowedFilterItems ?? (_allowedFilterItems = new Dictionary<Type, List<Type>>()); }
            set { _allowedFilterItems = value; }
        }

        public static Dictionary<Type, List<Type>> AllowedFilterNodes
        {
            get { return _allowedFilterNodes ?? (_allowedFilterNodes = new Dictionary<Type, List<Type>>()); }
            set { _allowedFilterNodes = value; }
        }
    }
}