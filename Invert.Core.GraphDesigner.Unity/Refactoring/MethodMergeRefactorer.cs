using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.Visitors;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{
    public class MethodMergeRefactorer : AbstractAstVisitor
    {
        public MethodDeclaration[] GeneratedMethods { get; set; }

        public List<MethodDeclaration> MethodsToAdd { get; set; }

        public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
        {

            return base.VisitTypeDeclaration(typeDeclaration, data);
        }

        public override object VisitMethodDeclaration(MethodDeclaration methodDeclaration, object data)
        {
            var generated = GeneratedMethods.FirstOrDefault(p => p.Name == methodDeclaration.Name);
            if (generated != null)
            {
                MethodsToAdd.Remove(generated);

            }
            return base.VisitMethodDeclaration(methodDeclaration, data);
        }
    }
}