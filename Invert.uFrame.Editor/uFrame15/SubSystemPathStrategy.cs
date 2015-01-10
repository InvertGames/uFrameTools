using System.IO;
using Invert.Core.GraphDesigner;

public class SubSystemPathStrategy : DefaultCodePathStrategy
{
    public IDiagramFilter FindFilter(IDiagramNode node)
    {
        var allFilters = Data.GetFilters();
        foreach (var diagramFilter in allFilters)
        {
            if (node != diagramFilter && node.Project.PositionData.HasPosition(diagramFilter, node))
            {
                return diagramFilter;
            }
        }
        return null;
    }

    public SubSystemData FindSubsystem(IDiagramNode node)
    {
        var filter = FindFilter(node);
        if (filter == node) return null;
        if (filter == null) return null;

        while (!(filter is SubSystemData))
        {
            // Convert to node
            var filterNode = filter as IDiagramNode;

            // If its not a node at this point it must be hidden
            if (filterNode == null) return null;
            // Try again with the new filternode
            filter = FindFilter(filterNode);
            // if its null return
            if (filter == null)
            {
                return null;
            }
        }
        return filter as SubSystemData;
    }

    public string GetSubSystemPath(IDiagramNode node)
    {
        var subsystem = FindSubsystem(node);
        if (subsystem == null) return string.Empty;
        return subsystem.Name;
    }

    //public override string GetEditableControllerFilename(ElementData controllerName)
    //{
    //    return Path.Combine(GetSubSystemPath(controllerName), base.GetEditableControllerFilename(controllerName));
    //}

    //public override string GetEditableSceneManagerFilename(SceneManagerData nameAsSceneManager)
    //{
    //    return Path.Combine(GetSubSystemPath(nameAsSceneManager),base.GetEditableSceneManagerFilename(nameAsSceneManager);
    //}

    //public override string GetEditableViewFilename(ViewData nameAsView)
    //{
    //    return Path.Combine(GetSubSystemPath(nameAsView), base.GetEditableViewFilename(nameAsView));
    //}

    //public override string GetEditableViewModelFilename(ElementData nameAsViewModel)
    //{
    //    return Path.Combine(GetSubSystemPath(nameAsViewModel), base.GetEditableViewModelFilename(nameAsViewModel));
    //}

    //public override string GetEditableViewComponentFilename(ViewComponentData name)
    //{
    //    return Path.Combine(GetSubSystemPath(name), base.GetEditableViewComponentFilename(name));
    //}

}