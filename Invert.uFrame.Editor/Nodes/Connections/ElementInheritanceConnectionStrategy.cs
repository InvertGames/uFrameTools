using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
   
    public class ElementInheritanceConnectionStrategy : DefaultConnectionStrategy<ElementData,ElementData>
    {
        public override Color ConnectionColor
        {
            get { return Color.green; }
        }

        protected override bool IsConnected(ElementData outputData, ElementData inputData)
        {
            return inputData.BaseIdentifier == outputData.Identifier;
        }

        protected override void ApplyConnection(ElementData output, ElementData input)
        {
            input.SetBaseElement(output);
        }

        protected override void RemoveConnection(ElementData output, ElementData input)
        {
            input.RemoveBaseElement();
        }
    }
}