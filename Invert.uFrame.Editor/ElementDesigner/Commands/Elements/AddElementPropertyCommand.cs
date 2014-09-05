using System.Linq;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddElementPropertyCommand : EditorCommand<ElementNodeViewModel>
    {
        public override void Perform(ElementNodeViewModel nodeViewModel)
        {
            nodeViewModel.AddProperty();
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
    public class AddTransitionCommand : EditorCommand<SceneManagerViewModel>
    {
        public override void Perform(SceneManagerViewModel nodeViewModel)
        {
            var data = nodeViewModel.DiagramViewModel.Data;
            var allCommands = uFrameEditor.Repository.NodeItems.OfType<ElementData>()
                .Where(p => !p.IsMultiInstance)
                .SelectMany(p => p.Commands).ToArray();

            ItemSelectionWindow.Init("Select Command", allCommands, (item) =>
            {
                nodeViewModel.AddCommandTransition(item as ViewModelCommandData);
            });
        }

        public override string CanPerform(SceneManagerViewModel node)
        {
            if (node == null) return "Arg can't be null";
            return null;
        }
    }
}