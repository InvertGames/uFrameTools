using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.uFrame.Editor.Nodes
{
    public class SceneTransitionConnectionStrategy : DefaultConnectionStrategy<SceneManagerTransition,SceneManagerData>
    {
        public override Color ConnectionColor
        {
            get { return uFrameEditor.Settings.TransitionLinkColor; }
        }

        protected override bool IsConnected(SceneManagerTransition outputData, SceneManagerData inputData)
        {
            return outputData.ToIdentifier == inputData.Identifier;
        }

        protected override void ApplyConnection(SceneManagerTransition output, SceneManagerData input)
        {
            output.ConnectTo(input);
        }

        protected override void RemoveConnection(SceneManagerTransition output, SceneManagerData input)
        {
            output.Disconnect();
        }
    }
}