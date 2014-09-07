using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class ViewComponentElementConnectionStrategy : DefaultConnectionStrategy<ElementData, ViewComponentData>
    {
        public override Color ConnectionColor
        {
            get { return Color.green; }
        }

        protected override bool CanConnect(ElementData output, ViewComponentData input)
        {
            return true;
        }

        protected override bool IsConnected(ElementData outputData, ViewComponentData inputData)
        {
            return inputData.ElementIdentifier == outputData.Identifier;
        }

        protected override void ApplyConnection(ElementData output, ViewComponentData input)
        {
            input.SetElement(output);
        }

        protected override void RemoveConnection(ElementData output, ViewComponentData input)
        {
            input.RemoveElement();
        }
    }
}