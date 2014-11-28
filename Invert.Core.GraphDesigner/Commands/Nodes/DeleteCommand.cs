using System;
using System.IO;
using System.Linq;

namespace Invert.Core.GraphDesigner
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
                    InvertGraphEditor.Platform.MessageBox("Delete sub items first.",
                        "There are items defined inside this item please hide or delete them before removing this item.", "OK");
                    return;
                }
            }
            if (InvertGraphEditor.Platform.MessageBox("Confirm", "Are you sure you want to delete this?", "Yes", "No"))
            {
                node.CurrentRepository.RemoveNode(selected.GraphItemObject);
                if (customFileFullPaths.Length > 0)
                {
                    if (InvertGraphEditor.Platform.MessageBox("Confirm",
                        "You have files associated with this. Delete them too?" + Environment.NewLine +
                        string.Join(Environment.NewLine, customFiles), "Yes Delete Them", "Don't Delete them"))
                    {
                        foreach (var customFileFullPath in customFileFullPaths)
                        {
                            File.Delete(customFileFullPath);
                        }
                        var saveCommand = InvertGraphEditor.Container.Resolve<IToolbarCommand>("SaveCommand");
                        //Execute the save command
                        InvertGraphEditor.ExecuteCommand(saveCommand);
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