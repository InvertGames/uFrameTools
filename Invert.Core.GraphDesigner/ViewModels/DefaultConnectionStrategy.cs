using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public abstract class DefaultConnectionStrategy<TOutputData, TInputData> : DefaultConnectionStrategy
        where TOutputData : class
        where TInputData : class
    {


        public abstract Color ConnectionColor { get; }
        public override ConnectionViewModel Connect(DiagramViewModel diagramViewModel, ConnectorViewModel a, ConnectorViewModel b)
        {
            return TryConnect<TOutputData, TInputData>(diagramViewModel, a, b, Apply, CanConnect);
        }

        protected virtual bool CanConnect(TOutputData output, TInputData input)
        {
            return true;
        }

        public override void Apply(ConnectionViewModel connectionViewModel)
        {
            var output = connectionViewModel.ConnectorA.DataObject as TOutputData;
            var input = connectionViewModel.ConnectorB.DataObject as TInputData;
            if (output != null && input != null)
            {
                ApplyConnection(output, input);
            }
        }

        public override void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        {
            base.GetConnections(connections, info);
            connections.AddRange(info.ConnectionsByData<TOutputData,TInputData>(this));
            //foreach (var item in info.DiagramData.Connections.Cont)
        }

 

        public abstract bool IsConnected(TOutputData output, TInputData input);
        

        protected abstract void ApplyConnection(TOutputData output, TInputData input);

        public override void Remove(ConnectionViewModel connectionViewModel)
        {
            var output = connectionViewModel.ConnectorA.DataObject as TOutputData;
            var input = connectionViewModel.ConnectorB.DataObject as TInputData;
            if (output != null && input != null)
            {
                RemoveConnection(output, input);
            }

        }

        protected abstract void RemoveConnection(TOutputData output, TInputData input);
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
                    if (a.ConnectorForType != null && b.ConnectorForType != null)
                    {
                        if (a.ConnectorForType == b.ConnectorForType)
                        {
                            if (canConnect != null &&
                       !canConnect((TOutput)a.ConnectorFor.DataObject, (TInput)b.ConnectorFor.DataObject))
                                return null;

                            return new ConnectionViewModel(diagramViewModel)
                            {
                                IsStateLink = this.IsStateLink,
                                ConnectorA = a,
                                ConnectorB = b,
                                Apply = apply
                            };
                        }
                        return null;
                    }
                    if (canConnect != null &&
                        !canConnect((TOutput) a.ConnectorFor.DataObject, (TInput) b.ConnectorFor.DataObject))
                        return null;

                    return new ConnectionViewModel(diagramViewModel)
                    {
                        IsStateLink = this.IsStateLink,
                        ConnectorA = a,
                        ConnectorB = b,
                        Apply = apply
                    };
                }
            }
            return null;
        }

        public abstract void Apply(ConnectionViewModel connectionViewModel);
        public abstract void Remove(ConnectionViewModel connectionViewModel);
    }
}