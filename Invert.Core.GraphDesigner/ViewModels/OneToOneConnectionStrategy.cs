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

        protected override bool IsConnected(TSource output, TTarget input)
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
}