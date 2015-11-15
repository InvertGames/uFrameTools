using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public abstract class ConnectorStyleSchema : IConnectorStyleSchema
    {
        private readonly Dictionary<SideDirectionItem, object> TexturesCache = new Dictionary<SideDirectionItem, object>(SideDirectionItem.Comparer);
        protected string _emptyInputIconCode;
        protected string _emptyOutputIconCode;
        protected string _filledInputIconCode;
        protected string _filledOutputIconCode;
        protected string _emptyTwoWayIconCode;
        protected string _filledTwoWayIconCode;

        public object GetTexture(ConnectorSide side, ConnectorDirection direction, bool connected, Color tint = default(Color))
        {

            var item = new SideDirectionItem(side, direction, connected, tint);

            object image;
            bool containsImage = TexturesCache.TryGetValue(item, out image);

            if (containsImage && (Equals(image, null) || image.Equals(null)))
            {
                TexturesCache.Remove(item);
                containsImage = false;
            }

            if (!containsImage)
            {
                image = ConstructTexture(side, direction, connected, tint);
                TexturesCache.Add(item, image);
            }

            return image;
        }

        protected abstract object ConstructTexture(ConnectorSide side, ConnectorDirection direction, bool connected, Color tint = default(Color));

        public IConnectorStyleSchema WithInputIcons(string emptyIcon, string filledIcon)
        {
            _emptyInputIconCode = emptyIcon;
            _filledInputIconCode = filledIcon;
            return this;
        }

        public IConnectorStyleSchema WithOutputIcons(string emptyIcon, string filledIcon)
        {
            _emptyOutputIconCode = emptyIcon;
            _filledOutputIconCode = filledIcon;
            return this;
        }

        public IConnectorStyleSchema WithTwoWayIcons(string emptyIcon, string filledIcon)
        {
            _emptyTwoWayIconCode = emptyIcon;
            _filledTwoWayIconCode = filledIcon;
            return this;
        }

        public IConnectorStyleSchema WithDefaultIcons()
        {
            return WithInputIcons("DiagramArrowRightEmpty", "DiagramArrowRight").
                   WithTwoWayIcons("DiagramArrowRightEmpty", "DiagramArrowRight").
                   WithOutputIcons("DiagramArrowRightEmpty", "DiagramArrowRight");
        }

        internal struct SideDirectionItem
        {
            public readonly ConnectorSide Side;
            public readonly ConnectorDirection Direction;
            public readonly bool IsConnected;
            public readonly Color Tint;
            private readonly int _hashCode;

            public SideDirectionItem(ConnectorSide side, ConnectorDirection direction, bool isConnected, Color tint)
            {
                Side = side;
                Direction = direction;
                IsConnected = isConnected;
                Tint = tint;

                unchecked
                {
                    _hashCode = (int) Side;
                    _hashCode = (_hashCode * 397) ^ (int) Direction;
                    _hashCode = (_hashCode * 397) ^ IsConnected.GetHashCode();
                    _hashCode = (_hashCode * 397) ^ Tint.GetHashCode();
                }
            }

            private sealed class EqualityComparer : IEqualityComparer<SideDirectionItem>
            {
                public bool Equals(SideDirectionItem x, SideDirectionItem y)
                {
                    return 
                        x.Side == y.Side && 
                        x.Direction == y.Direction && 
                        x.IsConnected == y.IsConnected && 
                        x.Tint.r == y.Tint.r && 
                        x.Tint.g == y.Tint.g &&
                        x.Tint.b == y.Tint.b &&
                        x.Tint.a == y.Tint.a;
                }

                public int GetHashCode(SideDirectionItem obj)
                {
                    return obj._hashCode;
                }
            }

            private static readonly IEqualityComparer<SideDirectionItem> _comparerInstance = new EqualityComparer();

            public static IEqualityComparer<SideDirectionItem> Comparer
            {
                get
                {
                    return _comparerInstance;
                }
            }
        }
    }
}
