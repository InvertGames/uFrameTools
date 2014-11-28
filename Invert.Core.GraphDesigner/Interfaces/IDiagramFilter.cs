using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;

namespace Invert.Core.GraphDesigner
{
    public interface IDiagramFilter
    {
        string Identifier { get; }
        bool ImportedOnly { get; }
        bool IsExplorerCollapsed { get; set; }

        FilterLocations Locations { get; set; }
        FilterCollapsedDictionary CollapsedValues { get; set; }
        string Name { get; set; }

        //bool IsAllowed(object item, Type t);
        //bool IsItemAllowed(object item, Type t);
    }

    public static class FilterExtensions
    {
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
            
            if (!InvertGraphEditor.AllowedFilterNodes.ContainsKey(filter.GetType())) return false;

            foreach (var x in InvertGraphEditor.AllowedFilterNodes[filter.GetType()])
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
    }
}