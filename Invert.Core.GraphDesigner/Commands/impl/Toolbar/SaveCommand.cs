
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{

    /// <summary>
    /// Events that are fired during the compilation process.
    /// </summary>
    public interface ICompileEvents
    {
        /// <summary>
        /// When the "Save & Compile" button is clicked. This is called first
        /// </summary>
        /// <param name="repository">The repository we are compiling.</param>
        /// <param name="diagramData">The diagram that is currently open</param>
        void PreCompile(INodeRepository repository, IGraphData diagramData);
        /// <summary>
        /// When a file is generated, this method is called.
        /// </summary>
        /// <param name="generator"></param>
        void FileGenerated(CodeFileGenerator generator);
        /// <summary>
        /// When saving & compiling is complete.
        /// </summary>
        /// <param name="repository">The repository that compiled.</param>
        /// <param name="diagramData">The current graph that was open when compiling.</param>
        void PostCompile(INodeRepository repository, IGraphData diagramData);

        void FileSkipped(CodeFileGenerator codeFileGenerator);
    }
    public class SaveCommand : ElementsDiagramToolbarCommand
    {
        public override string Title
        {
            get { return "Save & Compile"; }
        }
       
        public override void Perform(DiagramViewModel diagram)
        {
            var issues = diagram.CurrentRepository.Validate().ToArray();
            if (issues.Any())
            {
                foreach (var item in issues)
                {
                    if (item.Siverity == ValidatorType.Error)
                    {
                        Debug.LogError(item.Message);
                    }
                }
                if (InvertGraphEditor.Platform.MessageBox("Issues", "Please fix all issues before compiling.", "Ok",
                    "Do it anyways"))
                {
                    return;
                }
               
            }

            InvertGraphEditor.Platform.SaveAssets();
            InvertGraphEditor.Platform.Progress(0, "Refactoring");

            InvertApplication.SignalEvent<ICompileEvents>(_=>_.PreCompile(diagram.CurrentRepository, diagram.DiagramData));

            // Go ahead and process any code refactors
            //ProcessRefactorings(diagram);
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
                    var fileGenerator = codeFileGenerator;
                    InvertApplication.SignalEvent<ICompileEvents>(_ => _.FileSkipped(fileGenerator));
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
                CodeFileGenerator generator = codeFileGenerator;
                InvertApplication.SignalEvent<ICompileEvents>(_=>_.FileGenerated(generator));
            }
            
            foreach (var allDiagramItem in diagram.DiagramData.NodeItems)
            {
                allDiagramItem.IsNewNode = false;
            }

            InvertApplication.SignalEvent<ICompileEvents>(_ => _.PostCompile(diagram.CurrentRepository, diagram.DiagramData));
            //RefactorApplied(diagram.DiagramData);
            InvertGraphEditor.Platform.Progress(101f, "Done");

            var projectService = InvertApplication.Container.Resolve<ProjectService>();

            foreach (var graph in projectService.CurrentProject.Graphs)
            {
                graph.ChangeData.Clear();
            }

            InvertGraphEditor.Platform.RefreshAssets();
            diagram.Save();
#if UNITY_DLL
            UnityEditor.AssetDatabase.SaveAssets();
#endif
           
          
        }

        /// <summary>
        /// Execute all the refactorings queued in the diagram
        /// </summary>
        public void ProcessRefactorings(DiagramViewModel diagram)
        {
            //var refactorer = new RefactorContext(diagram.DiagramData.GetRefactorings());
            
            //var files = InvertGraphEditor.GetAllFileGenerators(null,diagram.CurrentRepository).Where(p=>!p.AssetPath.EndsWith(".designer.cs")).Select(p => p.SystemPath).ToArray();

            //if (refactorer.Refactors.Count > 0)
            //{
            //    //uFrameEditor.Log(string.Format("{0} : {1}", refactorer.GetType().Name , refactorer.CurrentFilename));

            //    refactorer.Refactor(files);
            //}
            
            //InvertApplication.Log(string.Format("Applied {0} refactors.", refactorer.Refactors.Count));
            
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