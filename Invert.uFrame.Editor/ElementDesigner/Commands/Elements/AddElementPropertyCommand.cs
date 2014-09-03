using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddElementPropertyCommand : EditorCommand<ElementNodeViewModel>
    {
        public override void Perform(ElementNodeViewModel nodeViewModel)
        {
            
            //var property = new ViewModelPropertyData()
            //{
            //    Node = nodeViewModel,
            //    DefaultValue = string.Empty,
            //    Name = nodeViewModel.Data.GetUniqueName("String1"),
            //    Type = typeof (string)
            //};
            //nodeViewModel.Properties.Add(property);
            //uFrameEditor.CurrentDiagram.Refresh();
            //property.IsSelected = true;
            //property.BeginEditing();
        }

        public override string CanPerform(ElementNodeViewModel node)
        {
            if (node == null) return "Arg can't be null";
            return null;
        }
    }
}