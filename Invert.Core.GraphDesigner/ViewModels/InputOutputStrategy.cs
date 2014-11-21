using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    //public class OneToOneConnectionStrategy<TSource, TTarget> :
    //    DefaultConnectionStrategy<TSource, TTarget>
    //    where TSource : class, IConnectable
    //    where TTarget : class, IConnectable
    //{
    //    private Color _connectionColor;

    //    public OneToOneConnectionStrategy()
    //        : this(Color.blue)
    //    {
    //    }

    //    public OneToOneConnectionStrategy(Color connectionColor)
    //    {
    //        _connectionColor = connectionColor;
    //    }

    //    public override Color ConnectionColor
    //    {
    //        get { return _connectionColor; }

    //    }

    //    public override bool IsConnected(TSource output, TTarget input)
    //    {
    //        return output.ConnectedGraphItemIds.Contains(input.Identifier);
    //    }

    //    protected override void ApplyConnection(TSource output, TTarget input)
    //    {
    //        var item = output.ConnectedGraphItems.OfType<TTarget>().FirstOrDefault();
    //        if (item == null)
    //        {
    //            output.ConnectedGraphItemIds.Add(input.Identifier);
    //        }
    //        else
    //        {
    //            output.ConnectedGraphItemIds.Remove(item.Identifier);
    //            output.ConnectedGraphItemIds.Add(input.Identifier);
    //        }

    //    }

    //    protected override void RemoveConnection(TSource output, TTarget input)
    //    {
    //        output.ConnectedGraphItemIds.Remove(input.Identifier);
    //    }
    //}

    //public class CustomConnectionStrategy<TSource, TTarget> :
    //    DefaultConnectionStrategy<TSource, TTarget>
    //    where TSource : class, IConnectable
    //    where TTarget : class, IConnectable
    //{
    //    private Color _connectionColor;
    //    public Action<TSource, TTarget> Apply { get; set; }
    //    public Action<TSource, TTarget> Remove { get; set; }

    //    public CustomConnectionStrategy(Color connectionColor, Func<TSource, TTarget, bool> isConnected, Action<TSource, TTarget> apply, Action<TSource, TTarget> remove)
    //    {
    //        _connectionColor = connectionColor;
    //        Apply = apply;
    //        Remove = remove;

    //    }

    //    public override Color ConnectionColor
    //    {
    //        get { return _connectionColor; }
    //    }

    //    public override bool IsConnected(TSource output, TTarget input)
    //    {
    //        if (Filter == null) return false;
    //        return Filter(output, input);
    //    }

    //    protected override void ApplyConnection(TSource output, TTarget input)
    //    {
    //        if (Apply != null)
    //        {
    //            Apply(output, input);
    //        }
    //    }

    //    protected override void RemoveConnection(TSource output, TTarget input)
    //    {
    //        if (Remove != null)
    //        {
    //            Remove(output, input);
    //        }
    //    }
    //}

    //public class GenericConnectionStrategy<TOutput, TInput> : IConnectionStrategy
    //    where TInput : class
    //    where TOutput : class
    //{
    //    private Color _color = UnityEngine.Color.white;
    //    public bool IsStateLink { get; set; }

    //    public Color Color
    //    {
    //        get { return _color; }
    //        set { _color = value; }
    //    }

    //    public Action<IConnectable, IConnectable> Apply { get; set; }
    //    public Action<IConnectable, IConnectable> Remove { get; set; }
    //    public Func<IConnectable, IConnectable, bool> IsConnected { get; set; }

    //    public bool AllowMultipleInputs { get; set; }
    //    public bool AllowMultipleOutputs { get; set; }
    //    public GenericConnectionStrategy()
    //    {
    //    }

    //    public ConnectionViewModel Connect(DiagramViewModel diagramViewModel, ConnectorViewModel a, ConnectorViewModel b)
    //    {
    //        if (a.Direction == ConnectorDirection.Output && b.Direction == ConnectorDirection.Input)
    //            if (a.DataObject is TOutput || a.DataObject is TInput || b.DataObject is TOutput || b.DataObject is TInput)
    //                if (a.ConnectorForType == b.ConnectorForType)
    //                {

    //                    return new ConnectionViewModel(diagramViewModel)
    //                    {
    //                        IsStateLink = IsStateLink,
    //                        Color = Color,
    //                        ConnectorA = a,
    //                        ConnectorB = b,
    //                        Apply = ApplyConnection,
    //                        Remove = RemoveConnection
    //                    };
    //                }
    //        return null;
    //    }
    //    private void RemoveConnection(ConnectionViewModel connectionViewModel)
    //    {
    //        var output = connectionViewModel.ConnectorA.DataObject as IConnectable;
    //        var input = connectionViewModel.ConnectorB.DataObject as IConnectable;
    //        if (output != null && input != null)
    //        {
    //            Remove(output, input);
    //        }

    //    }

    //    //public void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
    //    //{
    //    //    var inputs = info.Inputs.Where(p => p.DataObject is IConnectable).ToArray();
    //    //    var outputs = info.Outputs.Where(p => p.DataObject is IConnectable);
    //    //    var alreadyConnected = new List<string>();
    //    //    foreach (var output in outputs)
    //    //    {
    //    //        foreach (var input in inputs)
    //    //        {
    //    //            var tempId = output.DataObject.GetHashCode().ToString() + input.DataObject.GetHashCode();
    //    //            if (alreadyConnected.Contains(tempId)) continue;

    //    //            if (IsConnected(output.DataObject as IConnectable, input.DataObject as IConnectable))
    //    //            {
    //    //                connections.Add(new ConnectionViewModel(info.DiagramViewModel)
    //    //                {
    //    //                    IsStateLink = IsStateLink,
    //    //                    Color = Color,
    //    //                    ConnectorA = output,
    //    //                    ConnectorB = input,
    //    //                    Apply = ApplyConnection,
    //    //                    Remove = RemoveConnection
    //    //                });
    //    //                alreadyConnected.Add(tempId);
    //    //            }
    //    //        }
    //    //    }
    //    //}

    //    //private void ApplyConnection(ConnectionViewModel connectionViewModel)
    //    //{
    //    //    Debug.Log(string.Format("Applying -> {0}:{1}", connectionViewModel.ConnectorA.ConnectorForType, connectionViewModel.ConnectorB.ConnectorForType));
    //    //    var output = connectionViewModel.ConnectorA.DataObject as IConnectable;
    //    //    var input = connectionViewModel.ConnectorB.DataObject as IConnectable;

    //    //    if (output != null && input != null)
    //    //    {
    //    //        var diagram = connectionViewModel.DiagramViewModel.DiagramData;
    //    //        if (!connectionViewModel.ConnectorB.Configuration.AllowMultiple)
    //    //        {
    //    //            diagram.ClearInput(input);
    //    //        }
    //    //        if (!connectionViewModel.ConnectorA.Configuration.AllowMultiple)
    //    //        {
    //    //            diagram.ClearOutput(output);
    //    //        }
    //    //        diagram.AddConnection(output, input);
    //    //    }
    //    //}
    //}

    public class InputOutputStrategy : DefaultConnectionStrategy<IConnectable, IConnectable>
    {
        public override Color ConnectionColor
        {
            get { return Color.white; }
        }

        public override ConnectionViewModel Connect(DiagramViewModel diagramViewModel, ConnectorViewModel a, ConnectorViewModel b)
        {
            //if (!(a.DataObject is GenericSlot) && !(b.DataObject is GenericSlot))
            //{
            //    return null;
            //}
            if (a.Validator != null)
                if (!a.Validator(a.DataObject as IDiagramNodeItem, b.DataObject as IDiagramNodeItem))
                    return null;
            if (b.Validator != null)
                if (!b.Validator(a.DataObject as IDiagramNodeItem, b.DataObject as IDiagramNodeItem))
                    return null;

            return base.Connect(diagramViewModel, a, b);
        }


        public override bool IsConnected(IConnectable output, IConnectable input)
        {
            return
                InvertGraphEditor.CurrentProject.Connections.Any(
                    p => p.OutputIdentifier == output.Identifier && p.InputIdentifier == input.Identifier);
        }

    }

}