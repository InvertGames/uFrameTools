using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IConnectable : IGraphItem
    {
       
        IGraphData Graph { get; }
        IEnumerable<ConnectionData> Inputs { get; }
        IEnumerable<ConnectionData> Outputs { get; }

        bool AllowInputs { get; }
        bool AllowOutputs { get; }

        bool AllowMultipleInputs { get; }
        bool AllowMultipleOutputs { get; }
        Color Color { get; }

        //void OnConnectionApplied(IConnectable output, IConnectable input);
        bool CanOutputTo(IConnectable input);
        bool CanInputFrom(IConnectable output);

        void OnOutputConnectionRemoved(IConnectable input);
        void OnInputConnectionRemoved(IConnectable output);
        void OnConnectedToInput(IConnectable input);
        void OnConnectedFromOutput(IConnectable output);

        TType InputFrom<TType>();
        IEnumerable<TType> InputsFrom<TType>();
        IEnumerable<TType> OutputsTo<TType>();
        TType OutputTo<TType>();

    }

    public static class ConnectableExtensions
    {

        public static IEnumerable<ITypedItem> References(this IClassTypeNode node)
        {
           return node.Node.Repository.AllOf<ITypedItem>().Where(p => p.RelatedType == node.Identifier);
        }
        public static IEnumerable<TType> ReferencesOf<TType>(this IClassTypeNode node)
        {
            return node.Node.Repository.AllOf<ITypedItem>().Where(p => p.RelatedType == node.Identifier).OfType<TType>();
        }
        public static TType ReferenceOf<TType>(this IClassTypeNode node) where TType : ITypedItem
        {
            return node.Node.Repository.AllOf<ITypedItem>().OfType<TType>().FirstOrDefault(p => p.RelatedType == node.Identifier);
        }

        //public static IEnumerable<TType> InputsFrom<TType>(this IConnectable connectable)
        //    where TType : IGraphItem
        //{
        //    return connectable.Inputs.Select(p => p.Output).OfType<TType>();
        //}
        //public static TType InputFrom<TType>(this IConnectable connectable)
        //    where TType : IGraphItem
        //{
        //    return connectable.Inputs.Select(p => p.Output).OfType<TType>().FirstOrDefault();
        //}


        //public static IEnumerable<TType> OutputsTo<TType>(this IConnectable connectable)
        //    where TType : IGraphItem
        //{
        //    return connectable.Outputs.Select(p => p.Input).OfType<TType>();
        //}
        //public static TType OutputTo<TType>(this IConnectable connectable)
        //    where TType : IGraphItem
        //{
        //    return connectable.Outputs.Select(p => p.Input).OfType<TType>().FirstOrDefault();
        //}



    }
}