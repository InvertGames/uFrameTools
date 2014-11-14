using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class ViewComponentElementConnectionStrategy : DefaultConnectionStrategy<ViewData, ViewComponentData>
    {
        public override Color ConnectionColor
        {
            get { return Color.white; }
        }

        protected override bool CanConnect(ViewData output, ViewComponentData input)
        {
            return true;
        }

        public override bool IsConnected(ViewData output, ViewComponentData input)
        {
            return input.ViewIdentifier == output.Identifier;
        }

        protected override void ApplyConnection(ViewData output, ViewComponentData input)
        {
            input.SetView(output);
        }

        protected override void RemoveConnection(ViewData output, ViewComponentData input)
        {
            input.RemoveView();
        }
    }
}