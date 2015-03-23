using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class InputOutputStrategy : DefaultConnectionStrategy<IConnectable, IConnectable>
    {
        public override Color ConnectionColor
        {
            get { return Color.white; }
        }
        private static RegisteredConnection[] _connectionTypes;

        public static RegisteredConnection[] ConnectionTypes
        {
            get { return _connectionTypes ?? (_connectionTypes = InvertGraphEditor.Container.ResolveAll<RegisteredConnection>().ToArray()); }
        }


        public override void Remove(ConnectorViewModel output, ConnectorViewModel input)
        {
            
        }

        public override ConnectionViewModel Connect(DiagramViewModel diagramViewModel, ConnectorViewModel a, ConnectorViewModel b)
        {
             
            
            if (ConnectionTypes.Any(p => p.CanConnect(a.DataObject as IConnectable, b.DataObject as IConnectable)))
            {
                return base.Connect(diagramViewModel, a, b);
            }
            return null;
        }

    }

    //public class RegisteredConnectionStrategy : DefaultConnectionStrategy
    //{
    //    private static RegisteredConnection[] _connectionTypes;

    //    public static RegisteredConnection[] ConnectionTypes
    //    {
    //        get { return _connectionTypes ?? (_connectionTypes = InvertGraphEditor.Container.ResolveAll<RegisteredConnection>().ToArray()); }
    //    }

    //    public override Color ConnectionColor
    //    {
    //        get { return Color.white; }
    //    }

    //    public override ConnectionViewModel Connect(DiagramViewModel diagramViewModel, ConnectorViewModel a, ConnectorViewModel b)
    //    {
    //        if (ConnectionTypes.Any(p => p.CanConnect(a.DataObject.GetType(), b.DataObject.GetType())))
    //        {
    //            return CreateConnection(diagramViewModel, a, b, Apply);
    //        }
    //        return null;
    //    }
    //}

    public class RegisteredConnection
    {
        public Type TOutputType { get; set; }
        public Type TInputType { get; set; }

        public virtual bool CanConnect(IConnectable output, IConnectable input)
        {
            if (CanConnect(output.GetType(), input.GetType()))
            {
                if (output.CanOutputTo(input) && input.CanInputFrom(output))
                {
                    return true;
                }
            }
            return false;
        }
        public bool CanConnect(Type output, Type input)
        {
            if (TOutputType.IsAssignableFrom(output))
            {
                if (TInputType.IsAssignableFrom(input))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class CustomInputOutputStrategy<TOutput, TInput> : DefaultConnectionStrategy<TOutput, TInput>
        where TOutput : IGraphItem
        where TInput : IGraphItem
    {
        public NodeConfigSectionBase Configuration { get; set; }

        private Color _connectionColor = Color.white;

        public CustomInputOutputStrategy(Color connectionColor)
        {
            _connectionColor = connectionColor;
        }

        protected override bool CanConnect(TOutput output, TInput input)
        {

            return base.CanConnect(output, input);
        }

        public override Color ConnectionColor
        {
            get { return _connectionColor; }
        }

        public override void Remove(ConnectorViewModel output, ConnectorViewModel input)
        {
            InvertGraphEditor.DesignerWindow.DiagramViewModel.DiagramData.RemoveConnection(output.DataObject as IConnectable, input.DataObject as IConnectable);
        }
    }

    public class TypedItemConnectionStrategy : DefaultConnectionStrategy
    {
        public override Color ConnectionColor
        {
            get { return Color.white; }
        }

        public override void Remove(ConnectorViewModel output, ConnectorViewModel input)
        {
            var typedItem = output.DataObject as ITypedItem;
            if (typedItem != null)
            {
                typedItem.RemoveType();
            }
        }

        public override bool IsConnected(ConnectorViewModel output, ConnectorViewModel input)
        {
            if (output.DataObject == input.DataObject) return false;
            var typedItem = output.DataObject as ITypedItem;
            var classItem = input.DataObject as IClassTypeNode;
            if (typedItem != null && classItem != null)
            {
                if (typedItem.RelatedType == classItem.Identifier)
                {
                    return true;
                }
            }
            return false;
        }

        public override ConnectionViewModel Connect(DiagramViewModel diagramViewModel, ConnectorViewModel a, ConnectorViewModel b)
        {
            var typedItem = a.DataObject as GenericTypedChildItem;
            var clsType = b.DataObject as IClassTypeNode;
            if (clsType != null && typedItem != null)
            {
                if (a.Direction == ConnectorDirection.Output && b.Direction == ConnectorDirection.Input)
                return CreateConnection(diagramViewModel, a, b, Apply);
            }
            return null;
            return base.Connect(diagramViewModel, a, b);
        }

        protected override void ApplyConnection(IGraphData graph, IConnectable output, IConnectable input)
        {
            //base.ApplyConnection(graph, output, input);
            var typedItem = output as ITypedItem;
            
            if (typedItem != null)
            {
                typedItem.RelatedType = input.Identifier;
            }
        }

        //public override void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        //{
        //    base.GetConnections(connections, info);
        //    foreach (var item in info.Outputs.Where(p => p.ConnectorFor.DataObject is ITypedItem))
        //    {
        //        var referenceItem = item.ConnectorFor.DataObject as ITypedItem;
        //        if (referenceItem == null) continue;
        //        var sourceObject = referenceItem.RelatedNode();
        //        if (sourceObject == null) continue;
        //        foreach (var input in info.Inputs.Where(p => p.ConnectorFor.DataObject == sourceObject))
        //        {
        //            connections.Add(new ConnectionViewModel(info.DiagramViewModel)
        //            {
        //                Remove = Remove,
        //                Name = item.Name + "->" + input.Name,
        //                ConnectorA = item,
        //                ConnectorB = input,
        //                Color = new Color(0.3f, 0.4f, 0.75f)
        //            });
        //        }
        //    }
        //}

        public override void Remove(ConnectionViewModel connectionViewModel)
        {
            
            base.Remove(connectionViewModel);
            var obj = connectionViewModel.ConnectorA.ConnectorFor.DataObject as ITypedItem;
            if (obj != null)
            {
                obj.RelatedType = null;
            }
        }
    }
}