using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor
{
  

    public static class uFrameDataExtensions
    {
        public static IEnumerable<ITypedItem> GetAllBaseItems(this INodeRepository designerData, ElementData data)
        {
            var current = data;
            while (current != null)
            {
                foreach (var item in current.Items)
                {
                    if (item is ITypedItem)
                    {
                        yield return item as IBindableTypedItem;
                    }
                }

                current = designerData.GetAllElements().FirstOrDefault(p => p.Identifier == current.BaseIdentifier);
            }
        }

        public static ElementData GetViewModel(this INodeRepository designerData, string elementName)
        {
            return designerData.GetElements().FirstOrDefault(p => p.Name == elementName);
        }
        public static IEnumerable<RegisteredInstanceData> GetAllRegisteredElements(this INodeRepository t)
        {
            return t.NodeItems.OfType<SubSystemData>().SelectMany(p => p.Instances);
        }
        public static IEnumerable<ElementData> GetAllElements(this INodeRepository t)
        {
            return t.NodeItems.OfType<ElementData>();
        }

        public static IEnumerable<SceneManagerData> GetSceneManagers(this INodeRepository t)
        {
            return t.NodeItems.OfType<SceneManagerData>();
        }

        public static IEnumerable<SubSystemData> GetSubSystems(this INodeRepository t)
        {
            return t.NodeItems.OfType<SubSystemData>();
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
    }
}