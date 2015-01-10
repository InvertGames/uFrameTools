using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class MarkIsYieldCommand : EditorCommand<ElementCommandItemViewModel>, IDiagramNodeItemCommand
    {
        public override string Name
        {
            get { return "Is Yield Command"; }
        }

        public override string CanPerform(ElementCommandItemViewModel arg)
        {
            if (arg == null)
            {
                return "Must be a command to perform this operation.";
            }
                
            return null;
        }

        public override bool IsChecked(ElementCommandItemViewModel arg)
        {
            return arg.ElementItem.IsYield;
        }

        public override void Perform(ElementCommandItemViewModel arg)
        {
            arg.ElementItem.IsYield = !arg.ElementItem.IsYield;
        }
    }
}