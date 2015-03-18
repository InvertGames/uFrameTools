using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.PrettyPrinter;
using ICSharpCode.NRefactory.Visitors;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{
    public class URefactor : DiagramPlugin, INodeItemEvents, IDesignerWindowEvents, ICompileEvents
    {
        public override bool Ignore
        {
            get { return true; }
        }

        public Refactoring Refactoring = new Refactoring();

        public override void Initialize(uFrameContainer container)
        {
            InvertApplication.ListenFor<INodeItemEvents>(this);
            InvertApplication.ListenFor<ICompileEvents>(this);
            InvertApplication.ListenFor<IDesignerWindowEvents>(this);
        }

        public void Deleted(IDiagramNodeItem node)
        {

        }

        public void Hidden(IDiagramNodeItem node)
        {

        }

        public void Renamed(IDiagramNodeItem node, string previousName, string newName)
        {
            if (string.IsNullOrEmpty(previousName)) return;

            // Hey, wait just a second :p
            var multiClassTypeNode = node as IClassRefactorable;
            if (multiClassTypeNode != null)
            {
                foreach (var item in multiClassTypeNode.ClassNameFormats)
                {
                    var existing =
                        Refactoring.Refactorers.OfType<TypeRenameRefactorer>().FirstOrDefault(p => p.Item == node && p.Format == item);
                    if (existing != null)
                    {
                        existing.New = string.Format(item, newName);
                    }
                    else
                    {
                        Refactoring.Refactorers.Add(new TypeRenameRefactorer()
                        {
                            Old = string.Format(item, previousName),
                            New = string.Format(item, newName),
                            Format = item,
                            Item = node
                        });
                    }

                }
            }

            var identifierNode = node as IIdentifierRefactorable;
            if (identifierNode != null)
            {
                foreach (var item in identifierNode.IdentifierFormats)
                {
                    var existing =
                         Refactoring.Refactorers.OfType<IdentifierRenameRefactorer>().FirstOrDefault(p => p.Item == node && p.Format == item);
                    if (existing != null)
                    {
                        existing.New = string.Format(item, newName);
                    }
                    else
                    {
                        Refactoring.Refactorers.Add(new IdentifierRenameRefactorer()
                        {
                            Old = string.Format(item, previousName),
                            New = string.Format(item, newName),
                            Format = item,
                            Item = node
                        });
                    }
                }
            }
            Refactoring.Refactorers.RemoveAll(p => !p.IsValid);

            var items = Refactoring.Process(node.Node.Project);
            foreach (var item in items)
            {
                InvertApplication.Log(item.Filename);
                InvertApplication.Log(item.AfterText);
            }
        }

        public void ProcessInput()
        {
            //if (Refactoring.Refactorers.Count < 1) return;
            EditorGUI.HelpBox(new Rect(20f, 20f, 200, 30), string.Format("You have {0} refactors pending.", Refactoring.Refactorers.Count), MessageType.Info);
            if (GUI.Button(new Rect(20f, 60f, 200, 30), "Apply Now"))
            {
                var currentProject = InvertApplication.Container.Resolve<ProjectService>().CurrentProject;
                Refactoring.ProcessAndApply(currentProject);
            }
        }

        public void BeforeDrawGraph(Rect diagramRect)
        {

        }

        public void AfterDrawGraph(Rect diagramRect)
        {

        }

        public void DrawComplete()
        {

        }

        public void PreCompile(INodeRepository repository, IGraphData diagramData)
        {
            Refactoring.ProcessAndApply(repository);
        }

        public void FileGenerated(CodeFileGenerator generator)
        {

        }

        public void PostCompile(INodeRepository repository, IGraphData diagramData)
        {
            Refactoring = new Refactoring();
        }
    }

    public class Refactoring
    {
        private List<Refactorer> _refactorers;

        public List<Refactorer> Refactorers
        {
            get { return _refactorers ?? (_refactorers = new List<Refactorer>()); }
            set { _refactorers = value; }
        }

        public IEnumerable<Refactor> Process(INodeRepository repository)
        {
            Refactorers.RemoveAll(p => !p.IsValid);
            var files = InvertGraphEditor.GetAllFileGenerators(null, repository, false, null).Where(p => p.Generators.Any(x => !x.AlwaysRegenerate));
            foreach (var file in files)
            {
                if (!File.Exists(file.SystemPath)) continue;
                var text = File.ReadAllText(file.SystemPath);
                var parser = ParserFactory.CreateParser(SupportedLanguage.CSharp, new StringReader(text));
                parser.Parse();
                var changed = false;
                foreach (var item in Refactorers)
                {
                    parser.CompilationUnit.AcceptVisitor(item, null);
                    if (item.Changed)
                    {
                        changed = true;
                    }
                }
                var finalFilename = file.SystemPath;
                foreach (var item in Refactorers.OfType<TypeRenameRefactorer>())
                {
                    var fileName = Path.GetFileNameWithoutExtension(file.SystemPath);
                    Debug.Log(string.Format("Filename Before: {0}", fileName));
                    if (fileName == item.Old)
                    {
                        finalFilename = file.SystemPath.Replace(item.Old + ".cs", item.New + ".cs");
                        Debug.Log(string.Format("Filename After: {0}", finalFilename));
                        File.Move(file.SystemPath, finalFilename);
                    }
                }
                if (!changed) continue;
                CSharpOutputVisitor outputVisitor = new CSharpOutputVisitor();
                parser.CompilationUnit.AcceptVisitor(outputVisitor, null);
                yield return new Refactor()
                {
                    Filename = finalFilename,
                    BeforeText = text,
                    AfterText = outputVisitor.Text,

                };

                InvertApplication.Log(outputVisitor.Text);
            }

        }

        public void ProcessAndApply(INodeRepository repository)
        {
            foreach (var item in Process(repository))
            {
                item.Apply();
            }
            Refactorers.Clear();
        }
    }

    public class Refactor
    {
        public string Filename { get; set; }
        public string BeforeText { get; set; }
        public string AfterText { get; set; }

        public void Apply()
        {
            File.WriteAllText(Filename, AfterText);
        }
    }

    public class Refactorer : AbstractAstTransformer
    {
        public bool Changed { get; set; }
        public IDiagramNodeItem Item { get; set; }

        public virtual bool IsValid
        {
            get { return true; }
        }
    }

    public class RenameRefactorer : Refactorer
    {
        public string Old { get; set; }
        public string New { get; set; }
        public string Format { get; set; }
        public override bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(Old)) return false;
                if (string.IsNullOrEmpty(New)) return false;
                return Old != New;
            }
        }

    }
    
    public class TypeRenameRefactorer : RenameRefactorer
    {
        
        public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
        {
            if (typeDeclaration.Name == Old)
            {
                typeDeclaration.Name = New;

                Changed = true;
            }
            return base.VisitTypeDeclaration(typeDeclaration, data);
        }

        public override string ToString()
        {
            return string.Format("Rename {0} to {1}", Old, New);
        }


    }
    public class IdentifierRenameRefactorer : RenameRefactorer
    {

        public override object VisitIdentifierExpression(IdentifierExpression identifierExpression, object data)
        {
            if (identifierExpression.Identifier == Old)
            {
                identifierExpression.Identifier = New;
                Changed = true;
            }
            return base.VisitIdentifierExpression(identifierExpression, data);
        }
    }
    public class MethodParameterRenameRefactorer : RenameRefactorer
    {

        public override object VisitParameterDeclarationExpression(ParameterDeclarationExpression parameterDeclarationExpression, object data)
        {
            if (parameterDeclarationExpression.ParameterName == Old)
            {
                parameterDeclarationExpression.ParameterName = New;
                Changed = true;
            }
            return base.VisitParameterDeclarationExpression(parameterDeclarationExpression, data);
        }
    }
    public class MethodRenameRefactorer : RenameRefactorer
    {

        public override object VisitMethodDeclaration(MethodDeclaration methodDeclaration, object data)
        {
            if (methodDeclaration.Name == Old)
            {
                methodDeclaration.Name = New;
                Changed = true;
            }
            return base.VisitMethodDeclaration(methodDeclaration, data);
        }
    }
}
