using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Invert.uFrame.Editor.Connections
{

    public class GraphNode
    {
        
    }

    public interface IConnectable
    {
        IGraphItem GraphItem { get; set; }
        Func<Rect> Position { get; set; }
        IConnectionStrategy Strategy { get; set; }
        ConnectableIcon EmptyIcon { get; }
        ConnectableIcon ConnectedIcon { get; }
    }

    public enum ConnectableIcon
    {
        Left,
        Right,
        Down,
        Up,
        LeftAndRight,
        UpAndDown
    }

    public interface IConnection
    {
        IConnectable A { get; set; }
        IConnectable B { get; set; }

        Color Color { get; }
        string Text { get; }

        void Apply();
        void Disconnect();
    }

    public class InheritanceConnection : IConnection
    {
        public IConnectable BaseConnectable
        {
            get { return A; }
            set { A = value; }
        }

        public IConnectable DerivedConnectable
        {
            get { return B; }
            set { B = value; }
        }

        public ElementData BaseElement
        {
            get { return BaseConnectable.GraphItem as ElementData; }
        }

        public ElementData DerivedElement
        {
            get { return DerivedConnectable.GraphItem as ElementData; }
        }

        public IConnectable A { get; set; }
        public IConnectable B { get; set; }

        public Color Color
        {
            get { return BaseElement.Data.Settings.InheritanceLinkColor; }
        }

        public string Text
        {
            get { return string.Format("Derive {0} from {1}", DerivedElement.Name, BaseElement.Name); }
        }

        public void Apply()
        {
            DerivedElement.BaseIdentifier = BaseElement.Identifier;
        }

        public void Disconnect()
        {
            DerivedElement.BaseIdentifier = null;
        }

    }

    public interface IConnectionStrategy
    {
        IConnection Connect(IConnectable a, IConnectable b);
        IEnumerable<IConnection> GetConnections(IConnectable[] connectables);
    }


    public class InheirtanceConnectionStrategy : IConnectionStrategy
    {
        public IEnumerable<IConnection> GetConnections(IConnectable[] connectables)
        {
            foreach (var baseConnectable in connectables.OfType<IConnectionOutput>())
            {
                var baseElement = baseConnectable.GraphItem as ElementData;
                if (baseElement == null) continue;

                foreach (var derivedConnectable in connectables.OfType<IConnectionInput>())
                {
                    var derivedElement = derivedConnectable.GraphItem as ElementData;
                    if (derivedElement == null) continue;
                    if (derivedElement.Identifier == baseElement.Identifier) continue;

                    if (derivedElement.BaseTypeShortName == baseElement.Name)
                    {
                        yield return new InheritanceConnection()
                        {
                            BaseConnectable = baseConnectable,
                            DerivedConnectable = derivedConnectable
                        };
                    }
                }
            }
        }

        public IConnection Connect(IConnectable a, IConnectable b)
        {
            var baseElement = a.GraphItem as ElementData;
            var derivedElement = b.GraphItem as ElementData;

            if (baseElement != null && derivedElement != null)
            {
                return new InheritanceConnection()
                {
                    BaseConnectable = a,
                    DerivedConnectable = b
                };
            }

            return null;
        }


    }

    public interface IConnectionInput : IConnectable
    {

    }

    public interface IConnectionOutput : IConnectable
    {

    }

    public class ConnectionInput : IConnectionInput
    {
        public IConnectionStrategy Strategy { get; set; }
        public ConnectableIcon EmptyIcon { get; set; }
        public ConnectableIcon ConnectedIcon { get; set; }


        public IGraphItem GraphItem { get; set; }
        public Func<Rect> Position { get; set; }

    }
    public class ConnectionOutput : IConnectionOutput
    {
        public IConnectionStrategy Strategy { get; set; }
        public ConnectableIcon EmptyIcon { get; set; }
        public ConnectableIcon ConnectedIcon { get; set; }
        public IGraphItem GraphItem { get; set; }
        public Func<Rect> Position { get; set; }
    }
    //foreach (var connection in Connections)
    //{
    //    ConnectionDrawer.Draw(connection);
    //}
}
