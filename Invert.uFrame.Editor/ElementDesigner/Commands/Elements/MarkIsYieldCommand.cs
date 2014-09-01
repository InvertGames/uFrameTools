namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class MarkIsYieldCommand : EditorCommand<ViewModelCommandData>, IDiagramNodeItemCommand
    {
        public override string Name
        {
            get { return "Is Yield Command"; }
        }

        public override string CanPerform(ViewModelCommandData arg)
        {
            if (arg == null)
            {
                return "Must be a command to perform this operation.";
            }
                
            return null;
        }

        public override bool IsChecked(ViewModelCommandData arg)
        {
            return arg.IsYield;
        }

        public override void Perform(ViewModelCommandData arg)
        {
            arg.IsYield = !arg.IsYield;
        }
    }
}