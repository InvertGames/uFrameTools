
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
            var generatorSettings = uFrameEditor.CurrentProject.GeneratorSettings;
            var fileGenerators = uFrameEditor.GetAllFileGenerators(generatorSettings, uFrameEditor.CurrentProject).ToArray();
            Debug.Log(fileGenerators.Length);
            foreach (var codeFileGenerator in fileGenerators)
            {
            
            
                // Grab the information for the file
                var fileInfo = new FileInfo(codeFileGenerator.SystemPath);
                    Debug.Log(codeFileGenerator.SystemPath + ": " + fileInfo.Exists);
                
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
                   // uFrameEditor.Log(string.Format("Writing file with {0} with filename {1}", codeFileGenerator.GetType().Name, codeFileGenerator.Filename));
                // Write the file
                File.WriteAllText(fileInfo.FullName, codeFileGenerator.ToString());
                    } catch(Exception ex)
                {
                    Debug.LogError(ex);
                        Debug.Log("Coudln't create file " + fileInfo.FullName);
                }

            }
            
            foreach (var allDiagramItem in diagram.DiagramData.NodeItems)
            {
                allDiagramItem.IsNewNode = false;
            }
            RefactorApplied(diagram.DiagramData);
            AssetDatabase.Refresh();
            
            diagram.Save();
            diagram.DeselectAll();

        }

        /// <summary>
        /// Execute all the refactorings queued in the diagram
        /// </summary>
        public void ProcessRefactorings(DiagramViewModel diagram)
        {
            var refactorer = new RefactorContext(diagram.DiagramData.GetRefactorings());
            
            var files = uFrameEditor.GetAllFileGenerators(diagram.CurrentRepository.GeneratorSettings).Where(p=>!p.AssetPath.EndsWith(".designer.cs")).Select(p => p.SystemPath).ToArray();


            
            if (refactorer.Refactors.Count > 0)
            {
                //uFrameEditor.Log(string.Format("{0} : {1}", refactorer.GetType().Name , refactorer.CurrentFilename));

                refactorer.Refactor(files);
            }
            
            UnityEngine.Debug.Log(string.Format("Applied {0} refactors.", refactorer.Refactors.Count));
            
        }

        public void RefactorApplied(IGraphData data)
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