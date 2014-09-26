using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class ViewInheritanceConnectionStrategy : DefaultConnectionStrategy<ViewData, ViewData>
    {
        public override Color ConnectionColor
        {
            get { return Color.green; }
        }

        protected override bool IsConnected(ViewData outputData, ViewData inputData)
        {
            if (inputData.Identifier == outputData.Identifier) return false;

            return inputData.BaseViewIdentifier == outputData.Identifier;
        }

        protected override void ApplyConnection(ViewData output, ViewData input)
        {
            input.SetBaseView(output);
        }

        protected override void RemoveConnection(ViewData output, ViewData input)
        {
            input.ClearBaseView();
        }
    }
}