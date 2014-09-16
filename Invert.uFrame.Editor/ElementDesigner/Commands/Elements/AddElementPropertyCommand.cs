using System.Linq;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;

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
            var allCommands = nodeViewModel.GraphItem.Instances.Select(p=>p.RelatedNode()).OfType<ElementData>()
                .SelectMany(p => p.Commands).ToArray();

            ItemSelectionWindow.Init("Select Command", allCommands, (item) =>
            {

                uFrameEditor.ExecuteCommand((n) =>
                  {
                      nodeViewModel.AddCommandTransition(item as ViewModelCommandData);
                  });
            });
        }

        public override string CanPerform(SceneManagerViewModel node)
        {
            if (node == null) return "Arg can't be null";
            return null;
        }
    }
    public class AddInstanceCommand : EditorCommand<SceneManagerViewModel>
    {
        public override void Perform(SceneManagerViewModel nodeViewModel)
        {
            //var data = nodeViewModel.DiagramViewModel.Data;
            var subsystem = nodeViewModel.GraphItem.SubSystem;
            if (subsystem == null)
            {
                EditorUtility.DisplayDialog("Missing Subsystem",
                    "You need to associate a sub-system with this scene manager before adding registered elements.",
                    "OK");

            }
            var elements = nodeViewModel.GraphItem.SubSystem.GetContainingNodes(uFrameEditor.CurrentProject).OfType<ElementData>().ToArray();
            ItemSelectionWindow.Init("Select Command", elements, (item) =>
            {
                uFrameEditor.ExecuteCommand((n) =>
                {
                    nodeViewModel.AddInstance(item as ElementData);
                });

            });
        }

        public override string CanPerform(SceneManagerViewModel node)
        {
            if (node == null) return "Arg can't be null";
            return null;
        }
    }
}