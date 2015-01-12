using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public abstract class DefaultConnectionStrategy<TOutputData, TInputData> : DefaultConnectionStrategy
        where TOutputData : IGraphItem
        where TInputData : IGraphItem
    {


        public abstract Color ConnectionColor { get; }
        public override ConnectionViewModel Connect(DiagramViewModel diagramViewModel, ConnectorViewModel a, ConnectorViewModel b)
        {
            if (a.Validator != null)
                if (!a.Validator(a.DataObject as IDiagramNodeItem, b.DataObject as IDiagramNodeItem))
                {
                    // Debug.Log("Output validator denied this connection");
                    return null;
                }

            if (b.Validator != null)
                if (!b.Validator(a.DataObject as IDiagramNodeItem, b.DataObject as IDiagramNodeItem))
                {
                    // Debug.Log("Input validator denied this connection");
                    return null;
                }
                    
            return TryConnect<TOutputData, TInputData>(diagramViewModel, a, b, Apply, CanConnect);
        }

        protected virtual bool CanConnect(TOutputData output, TInputData input)
        {
            return true;
        }

        public override void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        {
            base.GetConnections(connections, info);
            connections.AddRange(info.ConnectionsByData(this));
            //foreach (var item in info.DiagramData.Connections.Cont)
        }

        public virtual bool IsConnected(INodeRepository currentRepository, TOutputData output, TInputData input)
        {
            
            return
               currentRepository.Connections.Any(
                   p => p.OutputIdentifier == output.Identifier && p.InputIdentifier == input.Identifier);
        }
        protected override void ApplyConnection(IGraphData graph, IGraphItem output, IGraphItem input)
        {
            base.ApplyConnection(graph, output, input);
            ApplyConnection(graph, (TOutputData)output, (TInputData)input);
        }

        protected virtual void ApplyConnection(IGraphData graph, TOutputData output, TInputData input)
        {
            
        }

        protected virtual void RemoveConnection(IGraphData graph, TOutputData output, TInputData input)
        {
            
        }

    }

    public abstract class DefaultConnectionStrategy : IConnectionStrategy
    {
        public virtual bool IsStateLink
        {
            get { return false; }
        }
        public virtual ConnectionViewModel Connect(DiagramViewModel diagramViewModel, ConnectorViewModel a, ConnectorViewModel b)
        {
            return null;
        }

        public virtual void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        {

        }

        protected ConnectionViewModel TryConnect<TOutput, TInput>(DiagramViewModel diagramViewModel, ConnectorViewModel a, ConnectorViewModel b, Action<ConnectionViewModel> apply, Func<TOutput, TInput, bool> canConnect = null)
        {
            if (a.ConnectorFor.DataObject is TOutput && b.ConnectorFor.DataObject is TInput)
            {
                if (a.Direction == ConnectorDirection.Output && b.Direction == ConnectorDirection.Input)
                {
                    //if (a.ConnectorForType != null && b.ConnectorForType != null)
                    //{
                        //if (b.ConnectorForType.IsAssignableFrom(a.ConnectorForType))
                        //{
                            if (canConnect != null &&
                                !canConnect((TOutput)a.ConnectorFor.DataObject, (TInput)b.ConnectorFor.DataObject))
                                return null;

                            return CreateConnection<TOutput, TInput>(diagramViewModel, a, b, apply);
                        //}
                        //return null;
                    //}
                    //if (canConnect != null &&
                    //    !canConnect((TOutput) a.ConnectorFor.DataObject, (TInput) b.ConnectorFor.DataObject))
                    //    return null;

                    //return new ConnectionViewModel(diagramViewModel)
                    //{
                    //    IsStateLink = this.IsStateLink,
                    //    ConnectorA = a,
                    //    ConnectorB = b,
                    //    Apply = apply
                    //};
                }
            }
            return null;
        }

        protected ConnectionViewModel CreateConnection<TOutput, TInput>(DiagramViewModel diagramViewModel, ConnectorViewModel a,
            ConnectorViewModel b, Action<ConnectionViewModel> apply)
        {
            return new ConnectionViewModel(diagramViewModel)
            {
                IsStateLink = this.IsStateLink,
                ConnectorA = a,
                ConnectorB = b,
                Apply = apply
            };
        }

        public virtual void Apply(ConnectionViewModel connectionViewModel)
        {
            var output = connectionViewModel.ConnectorA.DataObject as IGraphItem;
            var input = connectionViewModel.ConnectorB.DataObject as IGraphItem;
            var diagramData = connectionViewModel.DiagramViewModel.DiagramData;

            //if (!connectionViewModel.ConnectorA.AllowMultiple)
            //{

            //    diagramData.ClearOutput(output);
            //}
            if (!connectionViewModel.ConnectorB.AllowMultiple)
            {
                diagramData.ClearInput(input);
            }

            ApplyConnection(diagramData, output, input);
            
            //base.Apply(connectionViewModel);
        }

        protected virtual void ApplyConnection(IGraphData graph, IGraphItem output, IGraphItem input)
        {
            graph.AddConnection(output, input);
        }

        protected virtual void RemoveConnection(IGraphData graph, IGraphItem output, IGraphItem input)
        {
            graph.RemoveConnection(output, input);
        }
        public virtual void Remove(ConnectionViewModel connectionViewModel)
        {
            var output = connectionViewModel.ConnectorA.DataObject as IGraphItem;
            var input = connectionViewModel.ConnectorB.DataObject as IGraphItem;

            RemoveConnection(connectionViewModel.DiagramViewModel.DiagramData,output,input);

        }
    }
}