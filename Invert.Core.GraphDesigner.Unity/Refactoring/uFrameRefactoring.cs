﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Invert.ICSharpCode.NRefactory.CSharp;
using Invert.ICSharpCode.NRefactory.CSharp.Expressions;
using Invert.ICSharpCode.NRefactory.Editor;
using Invert.IOC;
using UnityEditor;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{
    public interface IRefactoringEvents
    {

        void ProcessRefactors(IChangeData change, List<IRefactorer> refactors);
    }
    public class uFrameRefactoring : DiagramPlugin, ICompileEvents, INodeItemEvents, IRefactoringEvents
    {
        public override void Initialize(UFrameContainer container)
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
                //InvertApplication.Log(string.Format("Refactoring {0}", file.SystemPath));

                var fileText = File.ReadAllText(file.SystemPath);
                CSharpParser parser = new CSharpParser();
                SyntaxTree tree = null;
                
                var document = new StringBuilderDocument(fileText);
                var formattingOptions = FormattingOptionsFactory.CreateAllman();
                var o = new TextEditorOptions() {TabsToSpaces = true};
                var script = new DocumentScript(document, formattingOptions, o);
                try
                {
          

                    tree = parser.Parse(document,"filename.cs");
                    if (parser.HasErrors)
                    {
                        InvertApplication.Log(string.Format("Couldn't parse file for refactoring because: {0}", string.Join(Environment.NewLine, parser.Errors.Select(p=>p.Message).ToArray())));
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    InvertApplication.LogError(string.Format("Couldn't parse file for refactoring because: {0}", ex.Message));
                    continue;
                }
                if (tree == null)
                {
            
                    continue;
                }
                
                foreach (var refactor in refactors)
                {
                    refactor.Script = script;
                    tree.AcceptVisitor(refactor);
                }
                try
                {
                
            
                }
                catch (Exception ex)
                {
                    InvertApplication.LogError(string.Format("Couldn't refactor file because {0}", ex.Message));
                    continue;
                }

                File.WriteAllText(file.SystemPath, document.Text);
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
                    //InvertApplication.Log(string.Format("Skipping {0} because it doesn't exist.", item.FullPathName));
                    continue;
                }

                //InvertApplication.Log(string.Format("Renaming {0} to {1}", oldFilename, newFilename));
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
                var templates = change.Item.GetTemplates().ToArray();
                var classRefactorable = templates.OfType<IClassRefactorable>().SelectMany(p => p.ClassNameFormats).ToArray();
                foreach (var format in classRefactorable.Distinct())
                {
                    //InvertApplication.Log(string.Format("Adding {0} : {1}", string.Format(format, changeData.Old), string.Format(format, changeData.New)));

                    refactors.Add(new RenameTypeRefactorer
                    {
                        Old = string.Format(format, changeData.Old),
                        New = string.Format(format, changeData.New)
                    });
                }
                var methodRefactorable = templates.OfType<IMethodRefactorable>().SelectMany(p => p.MethodFormats).ToArray();
                foreach (var format in methodRefactorable.Distinct())
                {
                    //InvertApplication.Log(string.Format("Adding {0} : {1}", string.Format(format, changeData.Old), string.Format(format, changeData.New)));

                    refactors.Add(new RenameTypeRefactorer
                    {
                        Old = string.Format(format, changeData.Old),
                        New = string.Format(format, changeData.New)
                    });
                }

            }
            var addChange = change as GraphItemAdded; 
            var isNodeAdded =
                   change.Item.Node.Graph.ChangeData.OfType<GraphItemAdded>()
                       .FirstOrDefault(p => p.ItemIdentifier == change.Item.Node.Identifier) != null;

            if (addChange != null && !isNodeAdded)
            {
                var members = change.Item.Node.GetEditableOutputMembers(_ => _.Identifier == change.ItemIdentifier && _ != change.Item.Node)
                    .Where(p => p != null && p.MemberAttribute != null && (p.MemberAttribute.Location == TemplateLocation.EditableFile || p.MemberAttribute.Location == TemplateLocation.Both))
                    .ToArray();

                var firstMember = members.FirstOrDefault();
                if (firstMember != null)
                {
                    
                    var text = CodeDomHelpers.GenerateCodeFromMembers(members.Select(p => p.MemberOutput).ToArray());
                   //  InvertApplication.Log(string.Format("Inserting test {0} {1}",firstMember.Decleration.Name, text));
                    var insertTextRefactorer = new InsertTextAtBottomRefactorer()
                    {
                        ClassName = firstMember.Decleration.Name,
                        Text = text
                    };
                    //InvertApplication.Log("Adding insert text refactoring");
                    refactors.Add(insertTextRefactorer);
                }
                else
                {
                    //InvertApplication.Log(string.Format("Added item has no outputmembers {0} ", change.Item.Name));
                }
            }



        }
    }
}