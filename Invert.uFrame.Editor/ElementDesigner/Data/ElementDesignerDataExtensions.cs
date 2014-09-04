using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.Refactoring;

public static class ElementDesignerDataExtensions
{
    public static IEnumerable<IDiagramNode> GetImportableItems(this IElementsDataRepository t, IDiagramFilter filter)
    {
        return
            t.GetAllowedDiagramItems(filter)
                .Where(p => !filter.Locations.Keys.Contains(p.Identifier))
                .ToArray();
    }
    public static IEnumerable<ElementData> GetAllElements(this INodeRepository t)
    {
        if (t == null)
        {
            throw new Exception("Designer data can't be null.");
        }
        if (t.NodeItems == null)
        {
            throw new Exception("All diagram items is null.");
        }
        return t.NodeItems.OfType<ElementData>();
    }

    public static IEnumerable<IDiagramNode> GetAllowedDiagramItems(this IElementsDataRepository t, IDiagramFilter filter)
    {
        return t.NodeItems.Where(p => filter.IsAllowed(p, p.GetType()));
    }
    public static IEnumerable<IDiagramNode> GetDiagramItems(this IElementsDataRepository t, IDiagramFilter filter)
    {
        return filter.FilterItems(t.NodeItems);
    }

    //public static IEnumerable<ElementDataBase> GetElements(this IElementDesignerData t)
    //{
    //    return t.GetDiagramItems().OfType<ElementDataBase>();
    //}

    public static IEnumerable<IDiagramFilter> GetFilterPath(this IElementDesignerData t)
    {
        return t.FilterState.FilterStack.Reverse();
    }

    public static IEnumerable<IDiagramFilter> GetFilters(this INodeRepository t)
    {
        return t.NodeItems.OfType<IDiagramFilter>();
    }

    public static IEnumerable<SceneManagerData> GetSceneManagers(this INodeRepository t)
    {
        return t.NodeItems.OfType<SceneManagerData>();
    }

    public static IEnumerable<ISubSystemData> GetSubSystems(this INodeRepository t)
    {
        return t.NodeItems.OfType<ISubSystemData>();
    }

    public static IEnumerable<ViewComponentData> GetViewComponents(this INodeRepository t)
    {
        return t.NodeItems.OfType<ViewComponentData>();
    }

    public static IEnumerable<ElementData> GetElements(this INodeRepository t)
    {
        return t.NodeItems.OfType<ElementData>();
    }

    public static IEnumerable<ViewData> GetViews(this INodeRepository t)
    {
        return t.NodeItems.OfType<ViewData>();
    }

    public static IEnumerable<EnumData> GetEnums(this INodeRepository t)
    {
        return t.NodeItems.OfType<EnumData>();
    }

    public static List<Refactorer> GetRefactorings(this INodeRepository data)
    {
        return
            data.NodeItems.OfType<IRefactorable>()
                .SelectMany(p => p.Refactorings)
                .Concat(data.NodeItems.SelectMany(p => p.Items).OfType<IRefactorable>().SelectMany(p => p.Refactorings))
                .ToList();
    }
    public static void Prepare(this IElementDesignerData designerData)
    {
        designerData.RefactorCount = 0;
        foreach (var allDiagramItem in designerData.NodeItems)
        {
            allDiagramItem.Data = designerData;
        }
        designerData.Initialize();
    }
    public static IEnumerable<IDiagramNode> FilterItems(this IElementDesignerData designerData, IEnumerable<IDiagramNode> allDiagramItems)
    {
        return designerData.CurrentFilter.FilterItems(allDiagramItems);
    }   
    public static void FilterLeave(this INodeRepository data)
    {
    }

    public static void ApplyFilter(this INodeRepository designerData)
    {
        
        designerData.UpdateLinks();
        //foreach (var item in designerData.AllDiagramItems)
        //{
            
            
        //}
        //UpdateLinks();
    }

    public static void CleanUpFilters(this INodeRepository designerData)
    {
        var diagramItems = designerData.NodeItems.Select(p => p.Identifier);

        foreach (var diagramFilter in designerData.GetFilters())
        {
            var removeKeys = diagramFilter.Locations.Keys.Where(p => !diagramItems.Contains(p)).ToArray();
            foreach (var removeKey in removeKeys)
            {
                diagramFilter.Locations.Remove(removeKey);
            }
        }
        //UpdateLinks();
    }

    public static string GetUniqueName(this INodeRepository designerData, string name)
    {
        var tempName = name;
        var index = 1;
        while (designerData.NodeItems.Any(p => p.Name.ToUpper() == tempName.ToUpper()))
        {
            tempName = name + index;
            index++;
        }
        return tempName;
    }

    public static IEnumerable<IDiagramNode> FilterItems(this IDiagramFilter filter, IEnumerable<IDiagramNode> allDiagramItems)
    {
        
        foreach (var item in allDiagramItems)
        {
            if (filter.IsAllowed(item, item.GetType()))
            {
                if (filter.ImportedOnly && filter != item)
                {
                    if (filter.Locations.Keys.Contains(item.Identifier))
                    {
                        yield return item;
                       
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }
    }

    public static void PopFilter(this IElementDesignerData designerData,List<string> filterStack)
    {
        designerData.FilterLeave();
        //filterStack.Remove(designerData.FilterStack.Peek().Name);

        designerData.FilterState.FilterPoped(designerData.FilterState.FilterStack.Pop());
        designerData.ApplyFilter();
    }

    public static void PopToFilter(this IElementDesignerData designerData, IDiagramFilter filter1)
    {
        while (designerData.CurrentFilter != filter1)
        {
            designerData.PopFilter(null);
        }
    }

    public static void PopToFilter(this IElementDesignerData designerData, string filterName)
    {
        while (designerData.CurrentFilter.Name != filterName)
        {
            designerData.PopFilter(null);
        }
    }

    public static void PushFilter(this IElementDesignerData designerData, IDiagramFilter filter)
    {
        designerData.FilterLeave();
        designerData.FilterState.FilterStack.Push(filter);
        designerData.FilterState.FilterPushed(filter);
        designerData.ApplyFilter();
    }



    public static void ReloadFilterStack(this IElementDesignerData designerData,List<string> filterStack )
    {
        if (filterStack.Count != (designerData.FilterState.FilterStack.Count))
        {
            foreach (var filterName in filterStack)
            {
                var filter = designerData.GetFilters().FirstOrDefault(p => p.Name == filterName);
                if (filter == null)
                {
                    filterStack.Clear();
                    designerData.FilterState.FilterStack.Clear();
                    break;
                }
                designerData.PushFilter(filter);
            }
        }
    }

    public static void UpdateLinks(this INodeRepository designerData)
    {
        //designerData.CleanUpFilters();
        //designerData.Links.Clear();

        //var items = designerData.GetDiagramItems().SelectMany(p => p.Items).Where(p => designerData.CurrentFilter.IsItemAllowed(p, p.GetType())).ToArray();
        //var diagramItems = designerData.GetDiagramItems().ToArray();
        //foreach (var item in items)
        //{
        //    designerData.Links.AddRange(item.GetLinks(diagramItems));
        //}

        //var diagramFilter = designerData.CurrentFilter as IDiagramNode;
        //if (diagramFilter != null)
        //{
        //    var diagramFilterItems = diagramFilter.Items.OfType<IDiagramNode>().ToArray();
        //    foreach (var diagramItem in diagramItems)
        //    {
        //        designerData.Links.AddRange(diagramItem.GetLinks(diagramFilterItems));
        //    }
        //}

        //var models = designerData.GetDiagramItems().ToArray();

        //foreach (var viewModelData in models)
        //{
        //    //viewModelData.Filter = CurrentFilter;
        //    designerData.Links.AddRange(viewModelData.GetLinks(diagramItems));
        //}
    }
    public static IEnumerable<ElementDataBase> GetAssociatedElementsInternal(this INodeRepository designerData, ElementDataBase data)
    {
        var derived = GetAllBaseItems(designerData, data);
        foreach (var viewModelItem in derived)
        {
            var element = GetElement(designerData,viewModelItem);
            if (element != null)
            {
                yield return element;
                var subItems = GetAssociatedElementsInternal(designerData, element);
                foreach (var elementDataBase in subItems)
                {
                    yield return elementDataBase;
                }
            }
        }
    }
    public static IEnumerable<IDiagramNode> FilterItems(this IElementsDataRepository designerData, IDiagramFilter filter)
    {
        return filter.FilterItems(designerData.NodeItems);
    }

    public static IEnumerable<IViewModelItem> GetAllBaseItems(this INodeRepository designerData, ElementDataBase data)
    {
        var current = data;
        while (current != null)
        {
            foreach (var item in current.Items)
            {
                if (item is IViewModelItem)
                {
                    yield return item as IViewModelItem;
                }
            }

            current = designerData.GetAllElements().FirstOrDefault(p => p.AssemblyQualifiedName == current.BaseTypeName);
        }
    }

    public static ElementDataBase[] GetAssociatedElements(this INodeRepository designerData, ElementDataBase data)
    {
        return GetAssociatedElementsInternal(designerData, data).Concat(new[] { data }).Distinct().ToArray();
    }

    public static IDiagramNode RelatedNode(this IViewModelItem item)
    {
        return item.Node.Data.NodeItems.FirstOrDefault(p => p.Name == item.RelatedTypeName);
    }
    public static ElementDataBase GetElement(this INodeRepository designerData, IViewModelItem item)
    {
        
        if (item.RelatedTypeName == null)
        {
            return null;
        }
        return designerData.GetAllElements().FirstOrDefault(p =>p!=null && p.Name == item.RelatedTypeName);
    }

    //public static IEnumerable<IDiagramFilter> GetFilters(this IElementDesignerData designerData, IDiagramFilter filter)
    //{
    //    //yield return DefaultFilter;
    //    //yield return SceneFlowFilter;

    //    foreach (var allDiagramItem in designerData.FilterItems(filter).OfType<IDiagramFilter>())
    //    {
    //        yield return allDiagramItem;
    //    }
    //}

    //public static string GetUniqueName(this IElementDesignerData designerData, string name)
    //{
    //    var tempName = name;
    //    var index = 1;
    //    while (designerData.AllDiagramItems.Any(p => p.Name.ToUpper() == tempName.ToUpper()))
    //    {
    //        tempName = name + index;
    //        index++;
    //    }
    //    return tempName;
    //}

    public static ElementData GetViewModel(this INodeRepository designerData, string elementName)
    {
        return designerData.GetElements().FirstOrDefault(p => p.Name == elementName);
    }

}