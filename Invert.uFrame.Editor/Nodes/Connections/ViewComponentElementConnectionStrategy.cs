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

        protected override bool IsConnected(ViewData outputData, ViewComponentData inputData)
        {
            return inputData.ViewIdentifier == outputData.Identifier;
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