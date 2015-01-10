//using Invert.uFrame.Editor.ViewModels;
//using UnityEngine;

//namespace Invert.uFrame.Editor.Nodes
//{
//    public class SceneTransitionConnectionStrategy : DefaultConnectionStrategy<SceneManagerTransition,SceneManagerData>
//    {
//        public override Color ConnectionColor
//        {
//            get { return Color.white; }
//        }

//        public override bool IsStateLink
//        {
//            get { return true; }
//        }

//        public override bool IsConnected(SceneManagerTransition output, SceneManagerData input)
//        {
//            return output.ToIdentifier == input.Identifier;
//        }

//        protected override void ApplyConnection(SceneManagerTransition output, SceneManagerData input)
//        {
//            output.ConnectTo(input);
//        }

//        protected override void RemoveConnection(SceneManagerTransition output, SceneManagerData input)
//        {
//            output.Disconnect();
//        }
//    }
//}