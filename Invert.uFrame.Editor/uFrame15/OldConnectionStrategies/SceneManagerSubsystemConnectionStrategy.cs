//using Invert.uFrame.Editor.ViewModels;
//using UnityEngine;

//namespace Invert.uFrame.Editor.Nodes
//{
//    public class SceneManagerSubsystemConnectionStrategy : DefaultConnectionStrategy<SubSystemData, SceneManagerData>
//    {
//        public override Color ConnectionColor
//        {
//            get { return Color.green; }
//        }

//        public override bool IsConnected(SubSystemData output, SceneManagerData input)
//        {
//            return input.SubSystemIdentifier == output.Identifier;
//        }

//        protected override void ApplyConnection(SubSystemData output, SceneManagerData input)
//        {
//            input.SubSystemIdentifier = output.Identifier;
//        }

//        protected override void RemoveConnection(SubSystemData output, SceneManagerData input)
//        {
//            input.SubSystemIdentifier = null;
//        }
//    }
//}