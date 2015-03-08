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
    public class URefactor : DiagramPlugin, INodeItemEvents, ICommandEvents, IDesignerWindowEvents
    {
        public Refactoring Refactoring = new Refactoring();

        public override void Initialize(uFrameContainer container)
        {
            InvertApplication.ListenFor<INodeItemEvents>(this);
            InvertApplication.ListenFor<ICommandEvents>(this);
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

        public void CommandExecuting(ICommandHandler handler, IEditorCommand command, object arg)
        {
            
        }

        public void CommandExecuted(ICommandHandler handler, IEditorCommand command, object arg)
        {
            if (command is SaveCommand)
            {
                Refactoring = new Refactoring();
            }
        }

        public void ProcessInput()
        {
            EditorGUI.HelpBox(new Rect(20f,20f,200,50),string.Format("You have {0} refactors pending.", Refactoring.Refactorers.Count),MessageType.Info );
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
            var files = InvertGraphEditor.GetAllFileGenerators(null, repository, false, null);
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
                if (!changed) continue;
                CSharpOutputVisitor outputVisitor = new CSharpOutputVisitor();
                parser.CompilationUnit.AcceptVisitor(outputVisitor, null);
                yield return new Refactor()
                {
                    Filename = file.SystemPath,
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
            get { return Old != New; }
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
