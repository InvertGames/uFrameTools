
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Invert.uFrame.Editor.Refactoring;
using UnityEditor;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class SaveCommand : ElementsDiagramToolbarCommand
    {
        public override string Title
        {
            get { return "Save & Compile"; }
        }

        public override void Perform(DiagramViewModel diagram)
        {
            // Go ahead and process any code refactors
            ProcessRefactorings(diagram);

            //var codeGenerators = uFrameEditor.GetAllCodeGenerators(item.Data).ToArray();

            var fileGenerators = uFrameEditor.GetAllFileGenerators(diagram.Data).ToArray();
            
            foreach (var codeFileGenerator in fileGenerators)
            {
                // Grab the information for the file
                var fileInfo = new FileInfo(System.IO.Path.Combine(diagram.Data.Settings.CodePathStrategy.AssetPath, codeFileGenerator.Filename));
                // Make sure we are allowed to generate the file
                if (!codeFileGenerator.CanGenerate(fileInfo)) continue;
                // Get the path to the directory
                var directory = System.IO.Path.GetDirectoryName(fileInfo.FullName);
                // Create it if it doesn't exist
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                // Write the file
                File.WriteAllText(fileInfo.FullName, codeFileGenerator.ToString());
                //Debug.Log("Created file: " + fileInfo.FullName);

            }
            foreach (var allDiagramItem in diagram.Data.LocalNodes)
            {
                allDiagramItem.IsNewNode = false;
            }
            RefactorApplied(diagram.Data);
            AssetDatabase.Refresh();
            
            diagram.Save();
            diagram.DeselectAll();

        }

        /// <summary>
        /// Execute all the refactorings queued in the diagram
        /// </summary>
        public void ProcessRefactorings(DiagramViewModel diagram)
        {
            var refactorer = new RefactorContext(diagram.Data.GetRefactorings());
            
            var files = uFrameEditor.GetAllFileGenerators(diagram.Data).Where(p=>!p.Filename.EndsWith(".designer.cs")).Select(p => System.IO.Path.Combine(diagram.Data.Settings.CodePathStrategy.AssetPath, p.Filename)).ToArray();

            
            if (refactorer.Refactors.Count > 0)
            {
#if DEBUG
                UnityEngine.Debug.Log(string.Format("{0} : {1}", refactorer.GetType().Name , refactorer.CurrentFilename));
#endif
                refactorer.Refactor(files);
            }
            
            UnityEngine.Debug.Log(string.Format("Applied {0} refactors.", refactorer.Refactors.Count));
            
        }
        public void RefactorApplied(IElementDesignerData data)
        {
            data.RefactorCount = 0;
            var refactorables = data.LocalNodes.OfType<IRefactorable>()
                .Concat(data.LocalNodes.SelectMany(p => p.Items).OfType<IRefactorable>());
            foreach (var refactorable in refactorables)
            {
                refactorable.RefactorApplied();
            }
        }
    }
}