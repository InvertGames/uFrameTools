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
            if (nodeViewModel.GraphItem.SubSystem == null)
            {
                EditorUtility.DisplayDialog("Subsystem not linked.",
                    "In order to add transitions you need to link a subsystem, when the subsystem is linked any instance with commands will be available for scene transitions",
                    "Okay, I got it");
                return;
            }
            
            var allCommands = nodeViewModel.ImportedInstances.Select(p=>p.RelatedNode()).OfType<ElementData>()
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
    public class AddInstanceCommand : EditorCommand<SubSystemViewModel>
    {
        public override void Perform(SubSystemViewModel nodeViewModel)
        {
            //var data = nodeViewModel.DiagramViewModel.Data;
            var subsystem = nodeViewModel.GraphItem;
            if (subsystem == null)
            {
                EditorUtility.DisplayDialog("Missing Subsystem",
                    "You need to associate a sub-system with this scene manager before adding registered elements.",
                    "OK");

            }
            var elements = nodeViewModel.GraphItem.GetContainingNodes(uFrameEditor.CurrentProject).OfType<ElementData>().ToArray();
            ItemSelectionWindow.Init("Select Command", elements, (item) =>
            {
                uFrameEditor.ExecuteCommand((n) =>
                {
                    nodeViewModel.AddInstance(item as ElementData);
                });

            });
        }

        public override string CanPerform(SubSystemViewModel node)
        {
            if (node == null) return "Arg can't be null";
            return null;
        }
    }
}