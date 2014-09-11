using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;

public interface IDiagramFilter
{
    string Identifier { get; }
    bool ImportedOnly { get; }

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
        return repository.NodeItems.Where(node => node != filter && repository.PositionData.HasPosition(filter,node));
    }
    public static bool IsAllowed(this IDiagramFilter filter, object item, Type t)
    {
        if (filter == item) return true;
        
        if (!uFrameEditor.AllowedFilterNodes.ContainsKey(filter.GetType())) return false;

        return uFrameEditor.AllowedFilterNodes[filter.GetType()].Contains(t);
    }

    public static bool IsItemAllowed(this IDiagramFilter filter, object item, Type t)
    {
        return true;
        //return uFrameEditor.AllowedFilterItems[filter.GetType()].Contains(t);
    }
}