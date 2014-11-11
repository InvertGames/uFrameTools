//using Invert.uFrame.Editor.ViewModels;

//namespace Invert.uFrame.Editor.ElementDesigner.Commands
//{
//    public class MarkIsMultiInstanceCommand : EditorCommand<ElementNodeViewModel>, IDiagramNodeCommand
//    {
//        public override string Group
//        {
//            get { return "Flags"; }
//        }
//        public override string Name
//        {
//            get { return "Has Multiple Instances?"; }
//        }

//        public override string CanPerform(ElementNodeViewModel arg)
//        {
//            if (arg == null)
//            {
//                return "Must be an element to perform this operation.";
//            }
//            if (!arg.IsLocal) return "The node muse be local to the current diagram.";
//            return null;
//        }

//        public override bool IsChecked(ElementNodeViewModel arg)
//        {
//            return arg.IsMultiInstance;
//        }

//        public override void Perform(ElementNodeViewModel arg)
//        {
//            arg.GraphItem.IsMultiInstance = !arg.IsMultiInstance;
//        }
//    }
//}