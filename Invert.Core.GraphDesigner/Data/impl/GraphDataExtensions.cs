using System.Collections.Generic;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public static class GraphDataExtensions
    {

        public static IEnumerable<IDiagramNode> GetImportableItems(this INodeRepository t, IDiagramFilter filter)
        {
            return
                t.GetAllowedDiagramItems(filter)
                    .Where(p => !t.PositionData.HasPosition(t.CurrentFilter, p))
                    .ToArray();
        }

        public static IEnumerable<IDiagramNode> GetAllowedDiagramItems(this INodeRepository t, IDiagramFilter filter)
        {
            return t.NodeItems.Where(p => filter.IsAllowed(p, p.GetType()));
        }

        public static IEnumerable<IDiagramNode> GetDiagramItems(this IProjectRepository t, IDiagramFilter filter)
        {
            return filter.FilterItems(t);
        }

        //public static IEnumerable<ElementDataBase> GetElements(this IElementDesignerData t)
        //{
        //    return t.GetDiagramItems().OfType<ElementDataBase>();
        //}

        public static IDiagramFilter Container(this IDiagramNode node)
        {
            var container = node.Project.NodeItems.OfType<IDiagramFilter>()
                .FirstOrDefault(p => p.GetContainingNodes(node.Project).Contains(node));
            return container;
        }



        public static IEnumerable<IDiagramFilter> FilterPath(this IDiagramNode node)
        {
            return FilterPathInternal(node).Reverse();
        }

        private static IEnumerable<IDiagramFilter> FilterPathInternal(IDiagramNode node)
        {
            var container = node.Container();
            while (container != null)
            {
                yield return container;
                var filterNode = container as IDiagramNode;
                if (filterNode != null)
                {
                    container = filterNode.Container();
                }
                else
                {
                    break;
                }

            }
        }
        public static IEnumerable<IDiagramFilter> GetFilterPath(this IGraphData t)
        {
            return t.FilterState.FilterStack.Reverse();
        }

        public static IEnumerable<IDiagramFilter> GetFilters(this INodeRepository t)
        {
            return t.NodeItems.OfType<IDiagramFilter>();
        }



        //public static IEnumerable<EnumData> GetEnums(this INodeRepository t)
        //{
        //    return t.NodeItems.OfType<EnumData>();
        //}

        public static List<Refactorer> GetRefactorings(this INodeRepository data)
        {
            return
                data.NodeItems.OfType<IRefactorable>()
                    .SelectMany(p => p.Refactorings)
                    .Concat(
                        data.NodeItems.SelectMany(p => p.DisplayedItems).OfType<IRefactorable>().SelectMany(p => p.Refactorings))
                    .ToList();
        }

        public static void Prepare(this IGraphData designerData)
        {
            designerData.RefactorCount = 0;
            designerData.Initialize();
        }

        public static IEnumerable<IDiagramNode> FilterItems(this IGraphData designerData, INodeRepository repository)
        {
            return designerData.CurrentFilter.FilterItems(repository);
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

            while (designerData.NodeItems.Any(p => p != null && p.Name != null && p.Name.ToUpper() == tempName.ToUpper()))
            {
                tempName = name + index;
                index++;
            }
            return tempName;
        }

        public static IEnumerable<IDiagramNode> FilterItems(this IDiagramFilter filter, INodeRepository repo)
        {

            foreach (var item in repo.NodeItems)
            {
                if (item == null) continue;
                
                if (filter.IsAllowed(item, item.GetType()))
                {
                    if (filter.ImportedOnly && filter != item)
                    {
                        if (repo.PositionData.HasPosition(filter, item))
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

        public static void PopFilter(this IGraphData designerData, List<string> filterStack)
        {
            if (designerData.FilterState.FilterStack.Count < 1) return;
            designerData.FilterLeave();
            //filterStack.Remove(designerData.FilterStack.Peek().Name);

            designerData.FilterState.FilterPoped(designerData.FilterState.FilterStack.Pop());
            designerData.ApplyFilter();
        }

        public static void PopToFilter(this IGraphData designerData, IDiagramFilter filter1)
        {
            while (designerData.CurrentFilter != filter1)
            {
                designerData.PopFilter(null);
            }
        }

        public static void PopToFilter(this IGraphData designerData, string filterName)
        {
            while (designerData.CurrentFilter.Name != filterName)
            {
                designerData.PopFilter(null);
            }
        }

        public static void PushFilter(this IGraphData designerData, IDiagramFilter filter)
        {
            designerData.FilterLeave();
            designerData.FilterState.FilterStack.Push(filter);
            designerData.FilterState.FilterPushed(filter);
            designerData.ApplyFilter();
        }



        public static void ReloadFilterStack(this IGraphData designerData, List<string> filterStack)
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

        //public static IEnumerable<ElementDataBase> GetAssociatedElementsInternal(this INodeRepository designerData, ElementData data)
        //{
        //    var derived = GetAllBaseItems(designerData, data);
        //    foreach (var viewModelItem in derived)
        //    {
        //        var element = GetElement(designerData,viewModelItem);
        //        if (element != null)
        //        {
        //            yield return element;
        //            var subItems = GetAssociatedElementsInternal(designerData, element);
        //            foreach (var elementDataBase in subItems)
        //            {
        //                yield return elementDataBase;
        //            }
        //        }
        //    }
        //}
        public static IEnumerable<IDiagramNode> FilterItems(this IProjectRepository designerData, IDiagramFilter filter)
        {
            return filter.FilterItems(designerData);
        }


        //public static ElementDataBase[] GetAssociatedElements(this INodeRepository designerData, ElementData data)
        //{
        //    return GetAssociatedElementsInternal(designerData, data).Concat(new[] { data }).Distinct().ToArray();
        //}

        public static IDiagramNode RelatedNode(this ITypedItem item)
        {
            var gt = item as GenericTypedChildItem;
            if (gt != null)
            {
                return gt.RelatedTypeNode;
            }
            return item.Node.Project.NodeItems.FirstOrDefault(p => p.Identifier == item.RelatedType);
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


    }
}