using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Schemas.impl
{
    public abstract class NodeStyleSchema : INodeStyleSchema
    {
        public object CollapsedHeaderStyleObject { get; protected set; }
        public object ExpandedHeaderStyleObject { get; protected set; }
        public object TitleStyleObject { get; protected set; }
        public object SubTitleStyleObject { get; protected set; }
        public bool ShowTitle { get; protected set; }
        public bool ShowSubtitle { get; protected set; }
        public bool ShowIcon { get; protected set; }
        public int TitleFontSize { get; protected set; }
        public int SubTitleFontSize { get; protected set; }
        public Color TitleColor { get; protected set; }
        public Color SubTitleColor { get; protected set; }
        public string TitleFont { get; protected set; }
        public string SubTitleFont { get; protected set; }
        public string HeaderImage { get; protected set; }
        public NodeColor HeaderColor { get; protected set; }
        public FontStyle TitleFontStyle { get; protected set; }
        public FontStyle SubTitleFontStyle { get; protected set; }
        public RectOffset HeaderPadding { get; protected set; }

        public virtual INodeStyleSchema RecomputeStyles()
        {
            return this;
        }

        protected abstract INodeStyleSchema GetInstance();

        public virtual INodeStyleSchema Clone()
        {
            var newSceham = GetInstance();
            newSceham.WithHeaderImage(HeaderImage);
            newSceham.WithIcon(ShowIcon);
            newSceham.WithTitle(ShowTitle);
            newSceham.WithHeaderPadding(HeaderPadding);
            newSceham.WithSubTitle(ShowSubtitle);
            newSceham.WithTitleFont(TitleFont, TitleFontSize, TitleColor,
                TitleFontStyle);
            newSceham.WithSubTitleFont(SubTitleFont, SubTitleFontSize, SubTitleColor,
                SubTitleFontStyle);
            return newSceham;
        }

        public virtual INodeStyleSchema WithHeaderColor(NodeColor color)
        {
            HeaderColor = color;
            return this;
        }

        public virtual INodeStyleSchema WithHeaderImage(string image)
        {
            HeaderImage = image;
            return this;
        }

        public virtual INodeStyleSchema WithTitleFont(string font, int? fontsize, Color? color, FontStyle? style)
        {
            TitleFontStyle = style.GetValueOrDefault(TitleFontStyle);
            TitleFontSize = fontsize.GetValueOrDefault(TitleFontSize);
            TitleColor = color.GetValueOrDefault(TitleColor);
            if (!string.IsNullOrEmpty(font))
                TitleFont = font;
            return this;
        }

        public virtual INodeStyleSchema WithSubTitleFont(string font, int? fontsize, Color? color, FontStyle? style)
        {
            SubTitleFontStyle = style.GetValueOrDefault(SubTitleFontStyle);
            SubTitleFontSize = fontsize.GetValueOrDefault(SubTitleFontSize);
            SubTitleColor = color.GetValueOrDefault(SubTitleColor);
            if (!string.IsNullOrEmpty(font))
                SubTitleFont = font;
            return this;
        }

        public INodeStyleSchema WithTitle(bool shown)
        {
            ShowTitle = shown;
            return this;
        }

        public INodeStyleSchema WithSubTitle(bool shown)
        {
            ShowSubtitle = shown;
            return this;
        }

        public INodeStyleSchema WithIcon(bool shown)
        {
            ShowIcon = shown;
            return this;
        }

        public INodeStyleSchema WithHeaderPadding(RectOffset padding)
        {
            HeaderPadding = padding;
            return this;
        }

        internal struct IconColorItem
        {
            public readonly string Name;
            public readonly Color Color;
            public readonly bool Expanded;
            private readonly int _hashCode;

            public IconColorItem(string name, Color color, bool expanded)
            {
                Name = name;
                Color = color;
                Expanded = expanded;

                unchecked
                {
                    _hashCode = Name.GetHashCode();
                    _hashCode = (_hashCode * 397) ^ Color.GetHashCode();
                    _hashCode = (_hashCode * 397) ^ Expanded.GetHashCode();
                }
            }

            private sealed class EqualityComparer : IEqualityComparer<IconColorItem>
            {
                public bool Equals(IconColorItem x, IconColorItem y)
                {
                    return 
                        x.Name == y.Name && 
                        x.Expanded == y.Expanded &&
                        x.Color.r == y.Color.r && 
                        x.Color.g == y.Color.g &&
                        x.Color.b == y.Color.b &&
                        x.Color.a == y.Color.a;
                }

                public int GetHashCode(IconColorItem obj)
                {
                    return obj._hashCode;
                }
            }

            private static readonly IEqualityComparer<IconColorItem> _comparerInstance = new EqualityComparer();

            public static IEqualityComparer<IconColorItem> Comparer
            {
                get
                {
                    return _comparerInstance;
                }
            }
        }

        private static Dictionary<IconColorItem, object> ImagePool = new Dictionary<IconColorItem, object>(IconColorItem.Comparer);

        public object GetHeaderImage(bool expanded, Color color = default(Color), string iconName = null)
        {
            var item = new IconColorItem(iconName, color, expanded);

            object image;
            bool containsImage = ImagePool.TryGetValue(item, out image);

            if (containsImage && (Equals(image, null) || image.Equals(null)))
            {
                ImagePool.Remove(item);
                containsImage = false;
            }

            if (!containsImage)
            {
                image = ConstructHeaderImage(expanded, color, iconName);
                ImagePool.Add(item, image);
            }

            return image;
        }

        public object GetIconImage(string iconName, Color color = default(Color))
        {
            var item = new IconColorItem(iconName, color, false);

            object image;
            bool containsImage = ImagePool.TryGetValue(item, out image);

            if (containsImage && (Equals(image, null) || image.Equals(null)))
            {
                ImagePool.Remove(item);
                containsImage = false;
            }

            if (!containsImage)
            {
                image = ConstructIcon(iconName, color);
                ImagePool.Add(item, image);
            }

            return image;
        }

        protected abstract object ConstructHeaderImage(bool expanded, Color color = default(Color), string iconName = null);
        protected abstract object ConstructIcon(string iconName, Color color = default(Color));

    }
}
