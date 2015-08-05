
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Invert.Core.GraphDesigner;
using Invert.Data;
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

        void PreCompile(IGraphConfiguration configuration, IDataRecord[] compilingRecords);


        /// <summary>
        /// When saving & compiling is complete.
        /// </summary>
        void PostCompile(IGraphConfiguration configuration, IDataRecord[] compilingRecords);

        /// <summary>
        /// When a file is generated, this method is called.
        /// </summary>
        void FileGenerated(CodeFileGenerator generator);
        void FileSkipped(CodeFileGenerator codeFileGenerator);
    }

    public interface IOnCompilerError
    {
        void Error(ErrorInfo info);
    }

    public class SaveCommand : ElementsDiagramToolbarCommand
    {
        public override string Title
        {
            get { return "Save & Compile"; }
        }
       
        public override void Perform(DiagramViewModel diagram)
        {
            //var service = InvertApplication.Container.Resolve<WorkspaceService>();
            //var issuesList = new List<ErrorInfo>();
            //foreach (var graph in service.CurrentWorkspace.Graphs)
            //{
            //    issuesList.AddRange(graph.Validate());
            //}
            
         
            //var issues = issuesList.ToArray();
            //if (issues.Any())
            //{
            //    foreach (var item in issues)
            //    {
            //        var item1 = item;
            //        InvertApplication.SignalEvent<IOnCompilerError>(_=>_.Error(item1));
            //    }
            //}
            //else
            //{

                InvertApplication.SignalEvent<ITaskHandler>(_ => { _.BeginTask(Generate()); });
           // }

           
          
        }

        public IEnumerator Generate()
        {
            

            var repository = InvertGraphEditor.Container.Resolve<IRepository>();
            repository.Commit();
            var config = InvertGraphEditor.Container.Resolve<IGraphConfiguration>();
            var items = repository.AllOf<IDataRecord>().ToArray();
            
            yield return
                new TaskProgress(0f, "Refactoring");
            
            // Grab all the file generators
            var fileGenerators = InvertGraphEditor.GetAllFileGenerators(config, items).ToArray();
            var length = 100f / (fileGenerators.Length + 1);
            
            var index = 0;
            
            foreach (var codeFileGenerator in fileGenerators)
            {
                index++;
                yield return new TaskProgress(length * index, "Generating " + System.IO.Path.GetFileName(codeFileGenerator.AssetPath));
                // Grab the information for the file
                var fileInfo = new FileInfo(codeFileGenerator.SystemPath);
                // Make sure we are allowed to generate the file
                if (!codeFileGenerator.CanGenerate(fileInfo))
                {
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
                try
                {
                    // Write the file
                    File.WriteAllText(fileInfo.FullName, codeFileGenerator.ToString());
                }
                catch (Exception ex)
                {
                    InvertApplication.LogException(ex);
                    InvertApplication.Log("Coudln't create file " + fileInfo.FullName);
                }
                CodeFileGenerator generator = codeFileGenerator;
                InvertApplication.SignalEvent<ICompileEvents>(_ => _.FileGenerated(generator));
            }
            InvertApplication.SignalEvent<ICompileEvents>(_ => _.PostCompile(config,items));

            yield return
                new TaskProgress(100f, "Complete");

#if UNITY_DLL
            repository.Commit();
            if (InvertGraphEditor.Platform != null) // Testability
            InvertGraphEditor.Platform.RefreshAssets();
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
            //var refactorables = data.NodeItems.OfType<IRefactorable>()
            //    .Concat(data.NodeItems.SelectMany(p => p.DisplayedItems).OfType<IRefactorable>());
            //foreach (var refactorable in refactorables)
            //{
            //    refactorable.RefactorApplied();
            //}
        }
    }

    

}