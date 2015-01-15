using System.Collections.Generic;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public interface IConnectable : IGraphItem
    {
        IEnumerable<ConnectionData> Inputs { get; }
        IEnumerable<ConnectionData> Outputs { get; }

        bool AllowMultipleInputs { get; }
        bool AllowMultipleOutputs { get; }

        void OnConnectionApplied(IConnectable output, IConnectable input);
    }

    public static class ConnectableExtensions
    {
        public static IEnumerable<TType> InputsFrom<TType>(this IConnectable connectable)
            where TType : IGraphItem
        {
            return connectable.Inputs.Select(p => p.Output).OfType<TType>();
        }
        public static TType InputFrom<TType>(this IConnectable connectable)
            where TType : IGraphItem
        {
            return connectable.Inputs.Select(p => p.Output).OfType<TType>().FirstOrDefault();
        }
        public static IEnumerable<TType> OutputsTo<TType>(this IConnectable connectable)
            where TType : IGraphItem
        {
            return connectable.Outputs.Select(p => p.Input).OfType<TType>();
        }
        public static TType OutputTo<TType>(this IConnectable connectable)
            where TType : IGraphItem
        {
            return connectable.Outputs.Select(p => p.Input).OfType<TType>().FirstOrDefault();
        }
    }
}