using JetBrains.Annotations;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IBreadcrumbsStyleSchema
    {

        object GetIcon(string name, Color tint = default(Color));

    }

    internal struct IconCacheItem
    {
        public string Name { get; set; }
        public Color TintColor { get; set; }

    }
}