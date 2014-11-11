using System.Linq;
using Invert.Core.GraphDesigner;
using UnityEditor;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
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
            var elements = nodeViewModel.GraphItem.GetContainingNodes(InvertGraphEditor.CurrentProject).OfType<ElementData>().ToArray();
            ItemSelectionWindow.Init("Select Command", elements, (item) =>
            {
                InvertGraphEditor.ExecuteCommand((n) =>
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