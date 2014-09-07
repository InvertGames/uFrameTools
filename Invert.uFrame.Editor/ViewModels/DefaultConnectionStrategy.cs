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
        public override ConnectionViewModel Connect(ConnectorViewModel a, ConnectorViewModel b)
        {
            return ConnectIO<TOutputData, TInputData>(a, b, Apply);
        }

        protected sealed override void Apply(ConnectionViewModel connectionViewModel)
        {
            var output = connectionViewModel.ConnectorA.DataObject as TOutputData;
            var input = connectionViewModel.ConnectorB.DataObject as TInputData;
            if (output != null && input != null)
            {
                Debug.Log("Applying Connection");
                ApplyConnection(output, input);
            }
        }

        public override void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        {
            base.GetConnections(connections, info);
            connections.AddRange(info.ConnectionsByData<TOutputData,TInputData>(ConnectionColor,IsConnected,Remove,Apply));
        }

        protected abstract bool IsConnected(TOutputData outputData, TInputData inputData);
        

        protected abstract void ApplyConnection(TOutputData output, TInputData input);

        protected override void Remove(ConnectionViewModel connectionViewModel)
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

        public virtual ConnectionViewModel Connect(ConnectorViewModel a, ConnectorViewModel b)
        {
            return null;
        }

        public virtual void GetConnections(List<ConnectionViewModel> connections, ConnectorInfo info)
        {

        }

        protected ConnectionViewModel ConnectIO<TOutput,TInput>(ConnectorViewModel a, ConnectorViewModel b, Action<ConnectionViewModel> apply)
        {
            if (a.ConnectorFor.DataObject is TOutput && b.ConnectorFor.DataObject is TInput)
            {
                if (a.Direction == ConnectorDirection.Output && b.Direction == ConnectorDirection.Input)
                {
                    return new ConnectionViewModel()
                    {
                        ConnectorA = a,
                        ConnectorB = b,
                        Apply = apply
                    };
                }
            }
            return null;
        }

        protected abstract void Apply(ConnectionViewModel connectionViewModel);
        protected abstract void Remove(ConnectionViewModel connectionViewModel);
    }
}