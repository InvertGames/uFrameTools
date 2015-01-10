
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class SaveCommand : ElementsDiagramToolbarCommand
    {
        public override string Title
        {
            get { return "Save & Compile"; }
        }

        public override void Perform(DiagramViewModel diagram)
        {
            InvertGraphEditor.Platform.SaveAssets();
            InvertGraphEditor.Platform.Progress(0, "Refactoring");
            // Go ahead and process any code refactors
            ProcessRefactorings(diagram);

            //var codeGenerators = uFrameEditor.GetAllCodeGenerators(item.Data).ToArray();
            //var generatorSettings = InvertGraphEditor.CurrentProject.GeneratorSettings;
            var fileGenerators = InvertGraphEditor.GetAllFileGenerators(null, diagram.CurrentRepository).ToArray();
            var length = 100f/(fileGenerators.Length +1);
           // Debug.Log(fileGenerators.Length);
            var index = 0;
            foreach (var codeFileGenerator in fileGenerators)
            {
                index++;
                 InvertGraphEditor.Platform.Progress(length*index,"Generating " + System.IO.Path.GetFileName(codeFileGenerator.AssetPath));
                // Grab the information for the file
                var fileInfo = new FileInfo(codeFileGenerator.SystemPath);
                    //Debug.Log(codeFileGenerator.SystemPath + ": " + fileInfo.Exists);
                
                // Make sure we are allowed to generate the file
                if (!codeFileGenerator.CanGenerate(fileInfo))
                {
                    //Debug.Log("Can't generate " + fileInfo.FullName);
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
                    InvertApplication.LogException(ex);
                    InvertApplication.Log("Coudln't create file " + fileInfo.FullName);
                }

            }
            
            foreach (var allDiagramItem in diagram.DiagramData.NodeItems)
            {
                allDiagramItem.IsNewNode = false;
            }
            RefactorApplied(diagram.DiagramData);
            InvertGraphEditor.Platform.Progress(101f, "Done");
            InvertGraphEditor.Platform.RefreshAssets();
            diagram.Save();
          
        }

        /// <summary>
        /// Execute all the refactorings queued in the diagram
        /// </summary>
        public void ProcessRefactorings(DiagramViewModel diagram)
        {
            var refactorer = new RefactorContext(diagram.DiagramData.GetRefactorings());
            
            var files = InvertGraphEditor.GetAllFileGenerators(null,diagram.CurrentRepository).Where(p=>!p.AssetPath.EndsWith(".designer.cs")).Select(p => p.SystemPath).ToArray();


            
            if (refactorer.Refactors.Count > 0)
            {
                //uFrameEditor.Log(string.Format("{0} : {1}", refactorer.GetType().Name , refactorer.CurrentFilename));

                refactorer.Refactor(files);
            }
            
            InvertApplication.Log(string.Format("Applied {0} refactors.", refactorer.Refactors.Count));
            
        }

        public void RefactorApplied(IGraphData data)
        {
            data.RefactorCount = 0;
            var refactorables = data.NodeItems.OfType<IRefactorable>()
                .Concat(data.NodeItems.SelectMany(p => p.DisplayedItems).OfType<IRefactorable>());
            foreach (var refactorable in refactorables)
            {
                refactorable.RefactorApplied();
            }
        }
    }
}