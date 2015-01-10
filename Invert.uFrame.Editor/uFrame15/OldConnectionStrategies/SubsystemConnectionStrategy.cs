//using System.Linq;
//using Invert.uFrame.Editor.ViewModels;
//using UnityEngine;

//namespace Invert.uFrame.Editor.Nodes
//{
//    public class SubsystemConnectionStrategy : DefaultConnectionStrategy<SubSystemData, SubSystemData>
//    {
//        public override Color ConnectionColor
//        {
//            get { return Color.green; }
//        }

//        protected override bool CanConnect(SubSystemData output, SubSystemData input)
//        {
//            if (output == input) return false;
//            return !output.GetAllImports().Contains(input.Identifier);
//        }

//        public override bool IsConnected(SubSystemData output, SubSystemData input)
//        {
//            return input.Imports.Contains(output.Identifier);
//        }

//        protected override void ApplyConnection(SubSystemData output, SubSystemData input)
//        {
//            input.Imports.Add(output.Identifier);
//        }

//        protected override void RemoveConnection(SubSystemData output, SubSystemData input)
//        {
//            input.Imports.Remove(output.Identifier);
//        }
//    }
//}