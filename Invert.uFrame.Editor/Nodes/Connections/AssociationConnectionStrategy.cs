using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class AssociationConnectionStrategy : DefaultConnectionStrategy<IViewModelItem, IDesignerType>
    {
        public override Color ConnectionColor
        {
            get { return uFrameEditor.Settings.TransitionLinkColor; }
        }

        protected override bool CanConnect(IViewModelItem output, IDesignerType input)
        {
            
            return base.CanConnect(output, input);
        }

        protected override bool IsConnected(IViewModelItem outputData, IDesignerType inputData)
        {
            if (string.IsNullOrEmpty(outputData.RelatedTypeName)) return false;
            return outputData.RelatedType == inputData.Identifier;
        }

        protected override void ApplyConnection(IViewModelItem output, IDesignerType input)
        {
            output.SetType(input);
        }

        protected override void RemoveConnection(IViewModelItem output, IDesignerType input)
        {
            output.RemoveType();
        }
    }
}