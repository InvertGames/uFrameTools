using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.PrettyPrinter;
using UnityEditor;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{
    public interface IRefactoringEvents
    {

        void ProcessRefactors(IChangeData change, List<IRefactorer> refactors);
    }
    public class uFrameRefactoring : DiagramPlugin, ICompileEvents, INodeItemEvents, IRefactoringEvents
    {
        public override void Initialize(uFrameContainer container)
        {
            ListenFor<INodeItemEvents>();
            ListenFor<ICompileEvents>();
            ListenFor<IRefactoringEvents>();
        }

        public void PreCompile(INodeRepository repository, IGraphData diagramData)
        {

        }

        public void FileGenerated(CodeFileGenerator generator)
        {

        }

        public void PostCompile(INodeRepository repository, IGraphData diagramData)
        {
            var project = repository as IProjectRepository;
            if (project == null) return;
            var changes = project.Graphs.SelectMany(p => p.ChangeData).ToArray();
            var refactors = new List<IRefactorer>();

            // Send out an event to decorate the refactorings
            foreach (var change in changes)
            {
                var change1 = change;
                InvertApplication.SignalEvent<IRefactoringEvents>(_ => _.ProcessRefactors(change1, refactors));
            }

            // Grab all the editable files
            var files = InvertGraphEditor.GetAllFileGenerators(null, repository)
                .Where(p => p.Generators.All(x => !x.AlwaysRegenerate)).ToArray();

            foreach (var file in files)
            {
                if (!File.Exists(file.SystemPath)) continue;
                InvertApplication.Log(string.Format("Refactoring {0}", file.SystemPath));

                var fileText = File.ReadAllText(file.SystemPath);
                var parser = ParserFactory.CreateParser(SupportedLanguage.CSharp, new StringReader(fileText));
                try
                {
                    parser.Parse();
                }
                catch (Exception ex)
                {
                    InvertApplication.LogError(string.Format("Couldn't parse file for refactoring because {0}", ex.Message));
                    continue;
                }
                try
                {
                    foreach (var refactor in refactors)
                    {
                        parser.CompilationUnit.AcceptVisitor(refactor, null);
                    }
                }
                catch (Exception ex)
                {
                    InvertApplication.LogError(string.Format("Couldn't refactor file because {0}", ex.Message));
                    continue;
                }

                var outputVisitor = new CSharpOutputVisitor();
                outputVisitor.BeforeNodeVisit += node =>
                {

                    foreach (var refactor in refactors)
                    {
                        refactor.OutputNodeVisiting(node, outputVisitor.OutputFormatter as CSharpOutputFormatter);
                    }

                };
                outputVisitor.AfterNodeVisit += node =>
                {

                        foreach (var refactor in refactors)
                        {
                            refactor.OutputNodeVisited(node, outputVisitor.OutputFormatter as CSharpOutputFormatter);
                        }
  
                };
                parser.CompilationUnit.AcceptVisitor(outputVisitor, null);
                File.WriteAllText(file.SystemPath, outputVisitor.Text);
            }

        }

        public void FileSkipped(CodeFileGenerator codeFileGenerator)
        {

        }

        public void Deleted(IDiagramNodeItem node)
        {

        }

        public void Hidden(IDiagramNodeItem node)
        {

        }

        public void Renamed(IDiagramNodeItem node, string previousName, string newName)
        {
            var graphNode = node;


            // Grab all the generated files
            var generators = graphNode.GetAllEditableFilesForNode().ToArray();

            var hasChanges = false;
            foreach (var item in generators)
            {
                node.Name = newName;
                var newFilename = item.FullPathName;
                // Set the node back to what it was for a second
                node.Name = previousName;
                var oldFilename = item.FullPathName;

                if (!File.Exists(oldFilename))
                {
                    InvertApplication.Log(string.Format("Skipping {0} because it doesn't exist.", item.FullPathName));
                    continue;
                }

                InvertApplication.Log(string.Format("Renaming {0} to {1}", oldFilename, newFilename));
                File.Move(oldFilename, newFilename);
                if (File.Exists(oldFilename + ".meta"))
                    File.Move(oldFilename + ".meta", newFilename + ".meta");


                hasChanges = true;
            }
            if (hasChanges)
            {
                //InvertGraphEditor.ExecuteCommand(new SaveCommand());
                AssetDatabase.Refresh();

            }
            node.Name = newName;

        }

        public void ProcessRefactors(IChangeData change, List<IRefactorer> refactors)
        {
            var changeData = change as NameChange;
            if (changeData != null)
            {
                var classRefactorable = change.Item as IClassRefactorable;
                if (classRefactorable != null)
                {
                    foreach (var format in classRefactorable.ClassNameFormats)
                    {
                        refactors.Add(new RenameTypeRefactorer()
                        {
                            Old = string.Format(format, changeData.Old),
                            New = string.Format(format, changeData.New)
                        });
                    }
                }
            }
        }
    }
}