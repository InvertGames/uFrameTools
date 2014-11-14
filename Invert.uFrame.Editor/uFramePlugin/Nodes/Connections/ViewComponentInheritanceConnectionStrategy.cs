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
            if (output.Identifier == input.Identifier) return false;
            if (output.View == null)
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

        public override bool IsConnected(ViewComponentData output, ViewComponentData input)
        {
            return input.BaseIdentifier == output.Identifier;
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