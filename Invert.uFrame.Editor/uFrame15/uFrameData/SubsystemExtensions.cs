using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public static class SubsystemExtensions
{
    public static IEnumerable<SubSystemData> GetAllImportedSubSystems(this SubSystemData subsystem)
    {
        return GetAllImportedSubSystems(subsystem, subsystem.Project);
    }

    public static IEnumerable<SubSystemData> GetAllImportedSubSystems(this SubSystemData subsystem,INodeRepository data)
    {
        
        var subSystem = data.NodeItems.OfType<SubSystemData>()
            .Where(p => subsystem.Imports.Contains(p.Identifier) ).ToArray();

        foreach (var subSystemData in subSystem)
        {
            yield return subSystemData;
           
        }
        foreach (var item in subSystem)
        {
            foreach (var systemData in item.GetAllImportedSubSystems(data))
            {
                yield return systemData;
            }
        }
    }
    public static IEnumerable<string> GetAllImports(this SubSystemData subsystem)
    {
        return subsystem.GetAllImportedSubSystems(subsystem.Project).SelectMany(p => p.Imports).Concat(subsystem.Imports);
    }
    public static IEnumerable<IDiagramNodeItem> GetIncludedItems(this SubSystemData subsystem)
    {
        foreach (var allDiagramItem in subsystem.Project.NodeItems.OfType<SubSystemData>())
        {
            if (subsystem.Imports.Contains(allDiagramItem.Identifier))
            {
                foreach (var item in allDiagramItem.GetSubItems())
                {
                    yield return item;
                }
            }
        }
    }
    public static IEnumerable<IDiagramNodeItem> GetSubItems(this SubSystemData subsystem)
    {
        var items =
            subsystem.Project.NodeItems.OfType<ElementData>()
                .Where(p => subsystem.Project.PositionData.HasPosition(subsystem,p))
                .Cast<IDiagramNodeItem>();

        foreach (var item in items)
            yield return item;
    }
    //public static IEnumerable<ViewModelCommandData> GetIncludedCommands(this SubSystemData subsystem)
    //{
    //    return subsystem.GetIncludedElements().Where(p => !p.IsMultiInstance).SelectMany(p => p.Commands);
    //}

    public static IEnumerable<ElementData> GetIncludedElements(this SubSystemData subsystem)
    {
        var list = new List<ElementData>();
        foreach (var diagramSubItem in subsystem.GetSubItems().OfType<ElementData>())
        {
            list.Add(diagramSubItem);
        }
        foreach (var allDiagramItem in subsystem.Project.NodeItems.OfType<SubSystemData>())
        {
            if (subsystem.Imports.Contains(allDiagramItem.Identifier))
            {
                foreach (var nested in allDiagramItem.GetIncludedElements())
                {
                    list.Add(nested);
                }
            }
        }
        return list.Distinct();
    }
}