using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddBindingCommand : EditorCommand<ViewNodeViewModel>
    {
        public override void Perform(ViewNodeViewModel node)
        {
            AddBindingWindow.Init("Add Binding", node);
        }

        public override string CanPerform(ViewNodeViewModel node)
        {
            return null;
        }
    }
}