using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ConnectorInfo
    {
        private ConnectorViewModel[] _inputs;
        private ConnectorViewModel[] _outputs;

        public ConnectorInfo(ConnectorViewModel[] allConnectors)
        {
            AllConnectors = allConnectors;
        }

        public ConnectorViewModel[] AllConnectors
        {
            get;
            set;
        }

        public ConnectorViewModel[] Inputs
        {
            get { return _inputs ?? (_inputs = AllConnectors.Where(p => p.Direction == ConnectorDirection.Input).ToArray()); }

        }
        public ConnectorViewModel[] Outputs
        {
            get { return _outputs ?? (_outputs = AllConnectors.Where(p => p.Direction == ConnectorDirection.Output).ToArray()); }
        }

        public IEnumerable<ConnectorViewModel> InputsWith<TData>()
        {
            return Inputs.Where(p => p.DataObject is TData);
        }
        public IEnumerable<ConnectorViewModel> OutputsWith<TData>()
        {
            return Outputs.Where(p => p.DataObject is TData);
        }

        public IEnumerable<ConnectionViewModel> ConnectionsByData<TSource, TTarget>(Color color, Func<TSource, TTarget, bool> isConnected, Action<ConnectionViewModel> remove, Action<ConnectionViewModel> apply = null, bool isStateLink = false)
        {
            foreach (var output in OutputsWith<TSource>())
            {
                foreach (var input in InputsWith<TTarget>())
                {
                    if (isConnected((TSource)output.DataObject, (TTarget)input.DataObject))
                    {
                        yield return new ConnectionViewModel()
                        {
                            IsStateLink = isStateLink,
                            Color = color,
                            ConnectorA = output,
                            ConnectorB = input,
                            Remove = remove,
                            Apply = apply
                        };
                    }
                }
            }
        }
    }
}