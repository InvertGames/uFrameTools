using System.Linq;
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

        protected override bool CanConnect(ElementData output, ElementData input)
        {
            if (output.Identifier == input.Identifier) return false;
            if (input.DerivedElements.Any(p => p.Identifier == output.Identifier)) return false;
            return base.CanConnect(output, input);
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