
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Invert.uFrame.Editor.Refactoring;
using UnityEditor;
using UnityEngine;

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
            AssetDatabase.SaveAssets();
            // Go ahead and process any code refactors
            ProcessRefactorings(diagram);

            //var codeGenerators = uFrameEditor.GetAllCodeGenerators(item.Data).ToArray();

            var fileGenerators = uFrameEditor.GetAllFileGenerators(uFrameEditor.CurrentProject.GeneratorSettings, uFrameEditor.CurrentProject).ToArray();
            Debug.Log(string.Format("{0} file generators", fileGenerators.Length));
            foreach (var codeFileGenerator in fileGenerators)
            {
                UnityEngine.Debug.Log(codeFileGenerator.Filename);
            }
        
            foreach (var codeFileGenerator in fileGenerators)
            {
               
                // Grab the information for the file
                var fileInfo = new FileInfo(System.IO.Path.Combine(diagram.Data.Settings.CodePathStrategy.AssetPath, codeFileGenerator.Filename));
                // Make sure we are allowed to generate the file
                if (!codeFileGenerator.CanGenerate(fileInfo))
                {
                    Debug.Log("Can't generate " + fileInfo.FullName);
                    continue;
                }
              
                // Get the path to the directory
                var directory = System.IO.Path.GetDirectoryName(fileInfo.FullName);
                // Create it if it doesn't exist
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                try {
                    uFrameEditor.Log(string.Format("Writing file with {0} with filename {1}", codeFileGenerator.GetType().Name, codeFileGenerator.Filename));
                // Write the file
                File.WriteAllText(fileInfo.FullName, codeFileGenerator.ToString());
                    } catch(Exception ex)
                {
                    Debug.LogError(ex);
                        Debug.Log("Coudln't create file " + fileInfo.FullName);
                }

            }
            foreach (var allDiagramItem in diagram.Data.NodeItems)
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
            
            var files = uFrameEditor.GetAllFileGenerators(diagram.CurrentRepository.GeneratorSettings).Where(p=>!p.Filename.EndsWith(".designer.cs")).Select(p => System.IO.Path.Combine(diagram.Data.Settings.CodePathStrategy.AssetPath, p.Filename)).ToArray();

            
            if (refactorer.Refactors.Count > 0)
            {
                uFrameEditor.Log(string.Format("{0} : {1}", refactorer.GetType().Name , refactorer.CurrentFilename));
                refactorer.Refactor(files);
            }
            
            UnityEngine.Debug.Log(string.Format("Applied {0} refactors.", refactorer.Refactors.Count));
            
        }

        public void RefactorApplied(IElementDesignerData data)
        {
            data.RefactorCount = 0;
            var refactorables = data.NodeItems.OfType<IRefactorable>()
                .Concat(data.NodeItems.SelectMany(p => p.Items).OfType<IRefactorable>());
            foreach (var refactorable in refactorables)
            {
                refactorable.RefactorApplied();
            }
        }
    }
}