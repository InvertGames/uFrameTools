using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class ComputedPropertyInputStrategy : DefaultConnectionStrategy<ViewModelPropertyData, ViewModelPropertyData>
    {
        public override Color ConnectionColor
        {
            get { return Color.white; }
        }

        protected override bool IsConnected(ViewModelPropertyData outputData, ViewModelPropertyData inputData)
        {
            return inputData.DependantPropertyIdentifiers.Contains(outputData.Identifier);
        }

        protected override void ApplyConnection(ViewModelPropertyData output, ViewModelPropertyData input)
        {
            input.DependantPropertyIdentifiers.Add(output.Identifier);
        }

        protected override void RemoveConnection(ViewModelPropertyData output, ViewModelPropertyData input)
        {
            input.DependantPropertyIdentifiers.Remove(output.Identifier);
        }
    }
}