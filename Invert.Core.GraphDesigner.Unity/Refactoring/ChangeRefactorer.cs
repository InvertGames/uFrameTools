using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.Visitors;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{

    public interface IRefactorer : IAstVisitor
    {
        
    }

    public class RenameTypeRefactorer : AbstractAstVisitor, IRefactorer
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
    }
  
}