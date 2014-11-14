using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class OneToOneConnectionStrategy<TSource, TTarget> :
        DefaultConnectionStrategy<TSource, TTarget>
        where TSource : class, IConnectable
        where TTarget : class, IConnectable
    {
        private Color _connectionColor;

        public OneToOneConnectionStrategy()
            : this(Color.blue)
        {
        }

        public OneToOneConnectionStrategy(Color connectionColor)
        {
            _connectionColor = connectionColor;
        }

        public override Color ConnectionColor
        {
            get { return _connectionColor; }

        }

        public override bool IsConnected(TSource output, TTarget input)
        {
            return output.ConnectedGraphItemIds.Contains(input.Identifier);
        }

        protected override void ApplyConnection(TSource output, TTarget input)
        {
            var item = output.ConnectedGraphItems.OfType<TTarget>().FirstOrDefault();
            if (item == null)
            {
                output.ConnectedGraphItemIds.Add(input.Identifier);
            }
            else
            {
                output.ConnectedGraphItemIds.Remove(item.Identifier);
                output.ConnectedGraphItemIds.Add(input.Identifier);
            }
            
        }

        protected override void RemoveConnection(TSource output, TTarget input)
        {
            output.ConnectedGraphItemIds.Remove(input.Identifier);
        }
    }

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


    public class InputReferenceConnectionStrategy<TSource, TReferenceType> : DefaultConnectionStrategy<TSource, TReferenceType>
        where TSource : class, IConnectable
        where TReferenceType : GenericConnectionReference, new()
    {
        private Color _connectionColor = Color.white;

        public override Color ConnectionColor
        {
            get { return _connectionColor; }
        }

        public InputReferenceConnectionStrategy(Color color)
        {
            _connectionColor = color;
        }

        protected override bool CanConnect(TSource output, TReferenceType input)
        {
            return true;
            //Debug.Log(output.GetType().Name + " : " + input.GetType().Name);
            return base.CanConnect(output, input);
        }

        public override ConnectionViewModel Connect(ConnectorViewModel a, ConnectorViewModel b)
        {
            //return CreateConnection(a, b);
            //Debug.Log(a.DataObject.GetType().Name + " : " + b.DataObject.GetType().Name);
            return base.Connect(a, b);
        }

        public override bool IsConnected(TSource output, TReferenceType input)
        {
            var node = input.Node as GenericNode;

            return node.GetConnectionReference<TReferenceType>().ConnectedGraphItemIds.Contains(output.Identifier);

            //node.GetInput<TReferenceType>();
            //    var inputItem = input.Node.ContainedItems.OfType<TReferenceType>().FirstOrDefault();
            //    if (inputItem == null) return false;
            //    return inputItem.ConnectedGraphItemIds.Contains(output.Identifier);
        }

        protected override void ApplyConnection(TSource output, TReferenceType input)
        {
            var node = input.Node as GenericNode;

            node.GetConnectionReference<TReferenceType>().ConnectedGraphItemIds.Add(output.Identifier);
        }

        protected override void RemoveConnection(TSource output, TReferenceType input)
        {
            var node = input.Node as GenericNode;
            node.GetConnectionReference<TReferenceType>().ConnectedGraphItemIds.Remove(output.Identifier);
        }
    }

    public class GenericConnectionStrategy<TOutput, TInput> : IConnectionStrategy where TInput : class where TOutput : class
    {
        private Color _color = UnityEngine.Color.white;
        public bool IsStateLink { get; set; }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public Action<TOutput,TInput> Apply { get; set; }
        public Action<TOutput,TInput> Remove { get; set; }
        public Func<TOutput,TInput,bool> IsConnected { get; set; }

 
        public GenericConnectionStrategy()
        {
        }

        public ConnectionViewModel Connect(ConnectorViewModel a, ConnectorViewModel b)
        {
            if (a.Direction == ConnectorDirection.Output && b.Direction == ConnectorDirection.Input)
            if (typeof(TOutput).IsAssignableFrom(a.ConnectorForType)  &&  typeof(TInput).IsAssignableFrom(b.ConnectorForType))
            {
                
                return new ConnectionViewModel()
                {
                    IsStateLink = IsStateLink,
                    Color = Color,
                    ConnectorA = a,
                    ConnectorB = b,
                    Apply = ApplyConnection,
                    Remove = RemoveConnection
                };
            }
            return null;
        }
        private void RemoveConnection(ConnectionViewModel connectionViewModel)
        {
            var output = connectionViewModel.ConnectorA.DataObject as TOutput;
            var input = connectionViewModel.ConnectorB.DataObject as TInput;
            if (output != null && input != null)
            {
                Remove(output, input);
            }

        }
    
        public void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        {
            var inputs = info.Inputs.Where(p => p.DataObject is TInput).ToArray();
            var outputs = info.Outputs.Where(p => p.DataObject is TOutput);
            var alreadyConnected = new List<string>();
            foreach (var output in outputs)
            {
                foreach (var input in inputs)
                {
                    var tempId = output.DataObject.GetHashCode().ToString() + input.DataObject.GetHashCode();
                    if (alreadyConnected.Contains(tempId)) continue;

                    if (IsConnected((TOutput)output.DataObject, (TInput)input.DataObject))
                    {
                        connections.Add(new ConnectionViewModel()
                        {
                            IsStateLink = IsStateLink,
                            Color = Color,
                            ConnectorA = output,
                            ConnectorB = input,
                            Apply = ApplyConnection,
                            Remove = RemoveConnection
                        });
                        alreadyConnected.Add(tempId);
                    }
                }
            }
        }

        private void ApplyConnection(ConnectionViewModel connectionViewModel)
        {
            var output = connectionViewModel.ConnectorA.DataObject as TOutput;
            var input = connectionViewModel.ConnectorB.DataObject as TInput;
            if (output != null && input != null)
            {
                Apply(output, input);
            }
        }
    }

}