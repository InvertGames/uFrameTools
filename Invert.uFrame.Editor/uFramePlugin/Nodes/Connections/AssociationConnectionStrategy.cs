using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class AssociationConnectionStrategy : DefaultConnectionStrategy<IBindableTypedItem, IDesignerType>
    {
        public override Color ConnectionColor
        {
            get { return Color.white; }
        }

        protected override bool CanConnect(IBindableTypedItem output, IDesignerType input)
        {
            
            return base.CanConnect(output, input);
        }

        protected override bool IsConnected(IBindableTypedItem output, IDesignerType input)
        {
            if (string.IsNullOrEmpty(output.RelatedTypeName)) return false;
            return output.RelatedType == input.Identifier;
        }

        protected override void ApplyConnection(IBindableTypedItem output, IDesignerType input)
        {
            output.SetType(input);
        }

        protected override void RemoveConnection(IBindableTypedItem output, IDesignerType input)
        {
            output.RemoveType();
        }
    }
}