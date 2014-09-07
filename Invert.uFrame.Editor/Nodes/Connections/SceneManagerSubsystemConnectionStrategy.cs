using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class SceneManagerSubsystemConnectionStrategy : DefaultConnectionStrategy<SubSystemData, SceneManagerData>
    {
        public override Color ConnectionColor
        {
            get { return Color.green; }
        }

        protected override bool IsConnected(SubSystemData outputData, SceneManagerData inputData)
        {
            return inputData.SubSystemIdentifier == outputData.Identifier;
        }

        protected override void ApplyConnection(SubSystemData output, SceneManagerData input)
        {
            input.SubSystemIdentifier = output.Identifier;
        }

        protected override void RemoveConnection(SubSystemData output, SceneManagerData input)
        {
            input.SubSystemIdentifier = null;
        }
    }
}