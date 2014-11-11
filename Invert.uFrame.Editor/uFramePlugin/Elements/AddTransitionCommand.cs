using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
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
            
            var allCommands = nodeViewModel.ImportedInstances.Select(p=>ElementDesignerDataExtensions.RelatedNode(p)).OfType<ElementData>()
                .SelectMany(p => p.Commands).ToArray();

            ItemSelectionWindow.Init("Select Command", allCommands, (item) =>
            {

                InvertGraphEditor.ExecuteCommand((n) =>
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
}