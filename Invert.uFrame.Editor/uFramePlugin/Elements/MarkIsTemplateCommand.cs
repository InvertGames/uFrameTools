using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class MarkIsTemplateCommand : EditorCommand<ElementNodeViewModel>, IDiagramNodeCommand
    {
        public override string Group
        {
            get { return "Flags"; }
        }
        public override string Name
        {
            get { return "Is Abstract"; }
        }

        public override string CanPerform(ElementNodeViewModel arg)
        {
            if (arg == null)
            {
                return "Must be an element to perform this operation.";
            }
            return null;
        }

        public override bool IsChecked(ElementNodeViewModel arg)
        {
            return arg.IsTemplate;
        }

        public override void Perform(ElementNodeViewModel arg)
        {
            arg.GraphItem.IsTemplate = !arg.IsTemplate;
        }
    }
}