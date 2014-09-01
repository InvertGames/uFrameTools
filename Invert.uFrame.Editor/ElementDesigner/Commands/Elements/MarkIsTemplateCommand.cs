namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class MarkIsTemplateCommand : EditorCommand<ElementData>, IDiagramNodeCommand
    {
        public override string Group
        {
            get { return "Flags"; }
        }
        public override string Name
        {
            get { return "Is Template"; }
        }

        public override string CanPerform(ElementData arg)
        {
            if (arg == null)
            {
                return "Must be an element to perform this operation.";
            }

            return null;
        }

        public override bool IsChecked(ElementData arg)
        {
            return arg.IsTemplate;
        }

        public override void Perform(ElementData arg)
        {
            arg.IsTemplate = !arg.IsTemplate;
        }
    }
}