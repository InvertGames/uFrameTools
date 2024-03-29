using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class DeleteCommand : EditorCommand<DiagramViewModel>, IDiagramNodeCommand,IKeyBindable
    {
        public override string Group
        {
            get { return "File"; }
        }

        public override decimal Order
        {
            get { return 3; }
        }

        public override void Perform(DiagramViewModel node)
        {

            if (node == null) return;
            var selected = node.SelectedNode;

            var pathStrategy = node.DiagramData.CodePathStrategy;

            var generators = selected.CodeGenerators.Where(p => !p.IsDesignerFile).ToArray();

            var customFiles = generators.Select(p=>p.Filename).ToArray();
            var customFileFullPaths = generators.Select(p=>System.IO.Path.Combine(pathStrategy.AssetPath, p.Filename)).Where(File.Exists).ToArray();

            if (selected.IsFilter)
            {
                
                if (selected.HasFilterItems)
                {
                    EditorUtility.DisplayDialog("Delete sub items first.",
                        "There are items defined inside this item please hide or delete them before removing this item.", "OK");
                    return;
                }
            }
            if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to delete this?", "Yes", "No"))
            {
                uFrameEditor.ExecuteCommand((d) =>
                {
                    node.CurrentRepository.RemoveNode(selected.GraphItemObject);
                });
                
                if (customFileFullPaths.Length > 0)
                {
                    if (EditorUtility.DisplayDialog("Confirm",
                        "You have files associated with this. Delete them too?" + Environment.NewLine +
                        string.Join(Environment.NewLine, customFiles), "Yes Delete Them", "Don't Delete them"))
                    {
                        foreach (var customFileFullPath in customFileFullPaths)
                        {
                            File.Delete(customFileFullPath);
                        }
                        var saveCommand = uFrameEditor.Container.Resolve<IToolbarCommand>("SaveCommand");
                        //Execute the save command
                        uFrameEditor.ExecuteCommand(saveCommand);
                    }
                }
            }
        }

        public override string CanPerform(DiagramViewModel diagram)
        {
            if (diagram.SelectedNode == null) return "Select something first.";
            if (!diagram.SelectedNode.IsLocal) return "Must be local to delete. Use hide instead.";
            return null;
        }
    }
}