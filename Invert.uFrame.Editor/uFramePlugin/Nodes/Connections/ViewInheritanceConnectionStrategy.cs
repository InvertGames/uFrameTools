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

        protected override bool IsConnected(ViewData output, ViewData input)
        {
            if (input.Identifier == output.Identifier) return false;

            return input.BaseViewIdentifier == output.Identifier;
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