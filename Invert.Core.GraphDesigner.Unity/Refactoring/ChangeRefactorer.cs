using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.Visitors;

namespace Invert.Core.GraphDesigner.Unity.Refactoring
{
    public class ChangeRefactorer : AbstractAstVisitor
    {
        public List<IChangeData> Changes { get; set; }

        public List<FilenameRefactor> FilenameChanges { get; set; }

        public bool HasChanged { get; set; }

        public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
        {
            foreach (var item in Changes.Where(p => p.Item is IClassRefactorable).OfType<NameChange>())
            {
                var refactorable = item.Item as IClassRefactorable;

                if (refactorable != null)
                    foreach (var format in refactorable.ClassNameFormats)
                    {
                        if (string.Format(format, item.Old) == typeDeclaration.Name)
                        {
                            typeDeclaration.Name = string.Format(format, item.New);
                            HasChanged = true;
                        }
                    }
            }
            return base.VisitTypeDeclaration(typeDeclaration, data);
        }

        public override object VisitMethodDeclaration(MethodDeclaration methodDeclaration, object data)
        {
            foreach (var item in Changes.Where(p => p.Item is IMethodRefactorable).OfType<NameChange>())
            {
                var refactorable = item.Item as IMethodRefactorable;

                if (refactorable != null)
                    foreach (var format in refactorable.MethodFormats)
                    {
                        if (string.Format(format, item.Old) == methodDeclaration.Name)
                        {
                            methodDeclaration.Name = string.Format(format, item.New);
                            HasChanged = true;
                        }
                    }
            }
            return base.VisitMethodDeclaration(methodDeclaration, data);
        }

        public override object VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration, object data)
        {
            foreach (var item in Changes.Where(p => p.Item is IPropertyRefactorable).OfType<NameChange>())
            {
                var refactorable = item.Item as IPropertyRefactorable;

                if (refactorable != null)
                    foreach (var format in refactorable.PropertyFormats)
                    {
                        if (string.Format(format, item.Old) == propertyDeclaration.Name)
                        {
                            propertyDeclaration.Name = string.Format(format, item.New);
                            HasChanged = true;
                        }
                    }
            }
            foreach (var item in Changes.Where(p => p.Item is ITypedItem).OfType<TypeChange>())
            {
                if (propertyDeclaration.TypeReference.Type == item.Old)
                {
                    propertyDeclaration.TypeReference.Type = item.New;
                }
            }
            return base.VisitPropertyDeclaration(propertyDeclaration, data);
        }

        public override object VisitParameterDeclarationExpression(ParameterDeclarationExpression parameterDeclarationExpression, object data)
        {
            return base.VisitParameterDeclarationExpression(parameterDeclarationExpression, data);
        }
    }
}