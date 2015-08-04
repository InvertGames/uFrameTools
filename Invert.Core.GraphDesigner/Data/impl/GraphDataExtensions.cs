using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner.Two;
using Invert.Data;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public static class GraphDataExtensions
    {
        public static void ShowInFilter(this IDiagramFilter filter, IDiagramNode node, Vector2 position, bool collapsed = false)
        {
            filter.Repository.Add(new FilterItem()
            {
                FilterId = filter.Identifier,
                NodeId = node.Identifier,
                Position = position,
                Collapsed = collapsed
            });
            var filterNode = filter as IDiagramNode;
            if (filterNode != null)
            {
                filterNode.NodeAddedInFilter(node);
            }
        }
        public static void HideInFilter(this IDiagramFilter filter, IDiagramNode node)
        {
            filter.Repository.RemoveAll<FilterItem>(p => p.FilterId == filter.Identifier && p.NodeId == node.Identifier);
        }
        public static IEnumerable<IDiagramNode> GetImportableItems(this IDiagramFilter filter)
        {
            if (filter == null)
                throw new ArgumentNullException("filter");
            var items = filter.FilterNodes().Select(p=>p.Identifier).ToArray();

            return
                filter.GetAllowedDiagramItems()
                    .Where(p => !items.Contains(p.Identifier))
                    .ToArray();
        }

        public static IEnumerable<IDiagramNode> GetAllowedDiagramItems(this IDiagramFilter filter)
        {

            return filter.Repository.AllOf<IDiagramNode>().Where(p => filter.IsAllowed(p, p.GetType()));
        }

        public static IDiagramFilter Container(this IDiagramNode node)
        {
            foreach (var item in node.Repository.All<FilterItem>())
            {
                if (item.NodeId == node.Identifier)
                {
                    return item.Filter;
                }
            }
            return null;
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
            return t.FilterStack.Reverse();
        }

        public static IEnumerable<IDiagramFilter> GetFilters(this INodeRepository t)
        {
            return t.NodeItems.OfType<IDiagramFilter>();
        }



        //public static IEnumerable<EnumData> GetEnums(this INodeRepository t)
        //{
        //    return t.NodeItems.OfType<EnumData>();
        //}


        public static void Prepare(this IGraphData designerData)
        {
            designerData.RefactorCount = 0;
            designerData.Initialize();
        }

        //public static IEnumerable<IDiagramNode> FilterItems(this IGraphData designerData, INodeRepository repository)
        //{
        //    return designerData.CurrentFilter.FilterItems(repository);
        //}

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

        public static IEnumerable<IDiagramNode> FilterNodes(this IDiagramFilter filter)
        {
            //var filterNode = filter as IDiagramNode;
            //if (filterNode != null)
            //{
            //    yield return filterNode;
            //}
            foreach (var item in filter.Repository.All<FilterItem>().Where(p=>p.FilterId == filter.Identifier))
            {
                var node = item.Node;
                if (node == null)
                {

                    throw new Exception(string.Format("Filter item node is null {0}", item.NodeId));
                    continue;
                }
                //if (item == null) continue;
                yield return node;
            }
        }
        public static IEnumerable<FilterItem> FilterItems(this IDiagramFilter filter)
        {
            var found = false;
            foreach (FilterItem p in filter.Repository.All<FilterItem>())
            {
                if (p.FilterId == filter.Identifier && p.NodeId == filter.Identifier)
                {
                    found = true;
                }
                if (p.FilterId == filter.Identifier) yield return p;
                
            }
            if (!found && filter is IDiagramNode)
            {
                var filterItem = filter.Repository.Create<FilterItem>();
                filterItem.FilterId = filter.Identifier;
                filterItem.NodeId = filter.Identifier;
                yield return filterItem;
            }
        }

        //public static void PushFilter(this IGraphData designerData, IDiagramFilter filter)
        //{
        //    var node = filter as IDiagramNode;
            
        //    var position = node == null ? Vector2.zero : designerData.GetItemLocation(node);

        //    designerData.FilterLeave();
        //    designerData.FilterState.FilterStack.Push(filter);
        //    designerData.FilterState.FilterPushed(filter);
            
        //    designerData.ApplyFilter();
        //    if (!designerData.PositionData.HasPosition(filter, node))
        //    {
        //        designerData.SetItemLocation(node,position);
        //    }
            
        //}



        //public static void ReloadFilterStack(this IGraphData designerData, List<string> filterStack)
        //{
        //    if (filterStack.Count != (designerData.FilterState.FilterStack.Count))
        //    {
        //        foreach (var filterName in filterStack)
        //        {
        //            var filter = designerData.GetFilters().FirstOrDefault(p => p.Name == filterName);
        //            if (filter == null)
        //            {
        //                filterStack.Clear();
        //                designerData.FilterState.FilterStack.Clear();
        //                break;
        //            }
        //            designerData.PushFilter(filter);
        //        }
        //    }
        //}

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

        //public static IEnumerable<IDiagramNode> FilterItems(this IDiagramFilter filter)
        //{
        //    return filter.FilterItems();
        //}


        //public static ElementDataBase[] GetAssociatedElements(this INodeRepository designerData, ElementData data)
        //{
        //    return GetAssociatedElementsInternal(designerData, data).Concat(new[] { data }).Distinct().ToArray();
        //}

        public static IDiagramNode RelatedNode(this ITypedItem item)
        {
            var gt = item as GenericTypedChildItem;
            if (gt != null)
            {
                return gt.RelatedTypeNode as IDiagramNode;
            }
            
            return item.Repository.GetById<IDiagramNode>(item.RelatedType);
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