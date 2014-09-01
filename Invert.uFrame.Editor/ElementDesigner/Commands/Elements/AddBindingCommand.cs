namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddBindingCommand : EditorCommand<ViewData>
    {
        public override void Perform(ViewData node)
        {
            AddBindingWindow.Init("Add Binding", node);
        }

        public override string CanPerform(ViewData node)
        {
            return null;
        }
    }
}