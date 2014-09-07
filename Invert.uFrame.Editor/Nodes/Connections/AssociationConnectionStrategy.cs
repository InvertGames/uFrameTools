using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class AssociationConnectionStrategy : DefaultConnectionStrategy<IViewModelItem, ElementData>
    {
        public override Color ConnectionColor
        {
            get { return uFrameEditor.Settings.TransitionLinkColor; }
        }

        protected override bool CanConnect(IViewModelItem output, ElementData input)
        {
            
            return base.CanConnect(output, input);
        }

        protected override bool IsConnected(IViewModelItem outputData, ElementData inputData)
        {
            if (string.IsNullOrEmpty(outputData.RelatedTypeName)) return false;
            return outputData.RelatedTypeName == inputData.Name;
        }

        protected override void ApplyConnection(IViewModelItem output, ElementData input)
        {
            output.SetType(input);
        }

        protected override void RemoveConnection(IViewModelItem output, ElementData input)
        {
            output.RemoveType();
        }
    }
}