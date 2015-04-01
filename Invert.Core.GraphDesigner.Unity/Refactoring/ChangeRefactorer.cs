using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.PrettyPrinter;
using ICSharpCode.NRefactory.Visitors;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{

    public interface IRefactorer : IAstVisitor
    {
        void OutputNodeVisited(INode node, CSharpOutputFormatter outputFormatter);
        void OutputNodeVisiting(INode node, CSharpOutputFormatter outputFormatter);
    }
    public class RenameTypeRefactorer : AbstractAstTransformer, IRefactorer
    {
        public string Old { get; set; }
        public string New { get; set; }

        public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
        {
            if (typeDeclaration.Name == Old)
                typeDeclaration.Name = New;

            return base.VisitTypeDeclaration(typeDeclaration, data);
        }

        public override object VisitTypeReference(TypeReference typeReference, object data)
        {
            if (typeReference.Type == Old)
                typeReference.Type = New;

            return base.VisitTypeReference(typeReference, data);
        }

        public void OutputNodeVisited(INode node, CSharpOutputFormatter outputFormatter)
        {

        }

        public void OutputNodeVisiting(INode node, CSharpOutputFormatter outputFormatter)
        {

        }
    }

    public class InsertTextAtBottomRefactorer : AbstractAstTransformer, IRefactorer
    {
        public string Text { get; set; }

        private INode _lastNode;
        public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
        {
            _lastNode = typeDeclaration.Children.Last();
            return base.VisitTypeDeclaration(typeDeclaration, data);
        }

        public void OutputNodeVisited(INode node, CSharpOutputFormatter outputFormatter)
        {
            if (node == _lastNode)
            {
                outputFormatter.PrintText(Text);
                outputFormatter.PrintText(Environment.NewLine);
            }
        }

        public void OutputNodeVisiting(INode node, CSharpOutputFormatter outputFormatter)
        {
          
        }
    }

}