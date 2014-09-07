using System.Linq;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class ViewComponentInheritanceConnectionStrategy : DefaultConnectionStrategy<ViewComponentData, ViewComponentData>
    {
        public override Color ConnectionColor
        {
            get { return Color.green; }
        }

        protected override bool CanConnect(ViewComponentData output, ViewComponentData input)
        {
            if (output.Element == null)
            {
                return false;
            }

            if (input.Identifier == output.BaseIdentifier)
            {
                return false;
            }
            if (output.AllBaseTypes.Any(p => p.Identifier == input.Identifier))
            {
                return false;
            }
            return true;
        }

        protected override bool IsConnected(ViewComponentData outputData, ViewComponentData inputData)
        {
            return inputData.BaseIdentifier == outputData.Identifier;
        }

        protected override void ApplyConnection(ViewComponentData output, ViewComponentData input)
        {
            input.SetBaseViewComponent(output);
        }

        protected override void RemoveConnection(ViewComponentData output, ViewComponentData input)
        {
            input.RemoveBaseViewComponent();
        }
    }
}