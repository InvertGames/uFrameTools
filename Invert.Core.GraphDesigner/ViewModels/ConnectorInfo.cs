using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.Core.GraphDesigner
{
    public class ConnectorInfo
    {
        private ConnectorViewModel[] _inputs;
        private ConnectorViewModel[] _outputs;

        public ConnectorInfo(ConnectorViewModel[] allConnectors, DiagramViewModel viewModel, IProjectRepository currentRepository)
        {
            AllConnectors = allConnectors;
            DiagramData = viewModel.DiagramData;
            DiagramViewModel = viewModel;
            CurrentRepository = currentRepository;
        }

        public ConnectorViewModel[] AllConnectors
        {
            get;
            set;
        }
        public DiagramViewModel DiagramViewModel { get; set; }
        public IGraphData DiagramData { get; set; }
        public IProjectRepository CurrentRepository { get; set; }

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

        public IEnumerable<ConnectionViewModel> ConnectionsByData<TSource, TTarget>(DefaultConnectionStrategy<TSource, TTarget> strategy)
            where TSource : IGraphItem
            where TTarget : IGraphItem
        {
            var alreadyConnected = new List<string>();
            foreach (var output in OutputsWith<TSource>())
            {
                foreach (var input in InputsWith<TTarget>())
                {
                    //if (strategy.Filter != null)
                    //{
                    //    if (!strategy.Filter((TSource) output.DataObject, (TTarget) input.DataObject))
                    //    {
                    //        continue;
                    //    }
                    //}
                    var tempId = output.DataObject.GetHashCode().ToString() + input.DataObject.GetHashCode();
                    if (alreadyConnected.Contains(tempId)) continue;
                    //if (output.ConnectorForType != null && input.ConnectorForType != null)
                    //{
                    //    if (input.ConnectorForType.IsAssignableFrom(output.ConnectorForType))
                    //    {
                            if (strategy.IsConnected((TSource) output.DataObject, (TTarget) input.DataObject))
                            {
                                yield return new ConnectionViewModel(DiagramViewModel)
                                {
                                    IsStateLink = strategy.IsStateLink,
                                    Color = strategy.ConnectionColor,
                                    ConnectorA = output,
                                    ConnectorB = input,
                                    Remove = strategy.Remove,
                                    Apply = strategy.Apply
                                };
                                alreadyConnected.Add(tempId);
                            }
                    //    }

                    //}
                    //else
                    //{
                    //    if (strategy.IsConnected((TSource)output.DataObject, (TTarget)input.DataObject))
                    //    {
                    //        yield return new ConnectionViewModel(DiagramViewModel)
                    //        {
                    //            IsStateLink = strategy.IsStateLink,
                    //            Color = strategy.ConnectionColor,
                    //            ConnectorA = output,
                    //            ConnectorB = input,
                    //            Remove = strategy.Remove,
                    //            Apply = strategy.Apply
                    //        };
                    //        alreadyConnected.Add(tempId);
                    //    }
                    //}
                  
                }
            }
        }
    }
}