using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
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
    public class AssociationConnectionStrategy : DefaultConnectionStrategy<IViewModelItem, ElementData>
    {
        public override Color ConnectionColor
        {
            get { return uFrameEditor.Settings.TransitionLinkColor; }
        }

        protected override bool IsConnected(IViewModelItem outputData, ElementData inputData)
        {
            return outputData.RelatedTypeName == inputData.Name;
        }

        protected override void ApplyConnection(IViewModelItem output, ElementData input)
        {
            output.SetType(input);
        }

        protected override void RemoveConnection(IViewModelItem output, ElementData input)
        {
            output.RemoveType();
        }
    }
}