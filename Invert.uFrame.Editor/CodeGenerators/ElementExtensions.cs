using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.uFrame.CodeGen.CodeDomExtensions
{
    public static class ElementExtensions
    {
        public static CodeTypeDeclaration CreateViewModelProperty(this CodeTypeDeclaration decl, ElementData element)
        {
            var viewModelProperty = new CodeMemberProperty
            {
                Name = element.Name,
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(element.NameAsViewModel),
                HasGet = true,
                HasSet = false
            };

            viewModelProperty.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeCastExpression(viewModelProperty.Type,
                    new CodePropertyReferenceExpression(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "View"), "ViewModelObject"))
                ));
            decl.Members.Add(viewModelProperty);
            return decl;
        }
        public static CodeMemberMethod ToMethod(this ViewModelCommandData command, string commandName = null, bool isOverride = false, bool addViewModelParameter = false)
        {
            var name = commandName ?? command.Name;
            var element = command.Node as ElementData;

            var commandMethod = new CodeMemberMethod
            {
                Name = name,
            };
            var baseCall = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), commandMethod.Name);

            if (isOverride)
            {
                commandMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                commandMethod.Statements.Add(baseCall);
            }
            else
            {
                commandMethod.Attributes = MemberAttributes.Public;

            }

            if (addViewModelParameter && element != null)
            {
                commandMethod.Parameters.Add(new CodeParameterDeclarationExpression(element.NameAsViewModel,
                   element.NameAsVariable));

                baseCall.Parameters.Add(new CodeVariableReferenceExpression(element.NameAsVariable));
            }

            if (!string.IsNullOrEmpty(command.RelatedType))
            {
                var relatedElement = command.RelatedNode() as ElementData;
                if (relatedElement == null)
                {
                    commandMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(new CodeTypeReference(command.RelatedTypeName), "arg"));
                }
                else
                {
                    commandMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(new CodeTypeReference(relatedElement.NameAsViewModel),
                            "arg"));
                }
                baseCall.Parameters.Add(new CodeVariableReferenceExpression("arg"));
            }
            return commandMethod;
        }

        public static CodeMethodInvokeExpression ToInvoke(this ViewModelCommandData command, string methodName = null, CodeExpression targetObject = null, string viewModelReferenceName = null, string argReferenceName = "arg")
        {
            var invoke =
                new CodeMethodInvokeExpression(
                    targetObject ?? new CodeThisReferenceExpression(),
                    methodName ?? command.Name);

            if (!string.IsNullOrEmpty(command.RelatedType))
            {
                var relatedElement = command.RelatedNode() as ElementData;
                if (relatedElement == null)
                {
                    if (argReferenceName != null)
                        invoke.Parameters.Add(new CodeVariableReferenceExpression(argReferenceName));
                }
                else if (!string.IsNullOrEmpty(viewModelReferenceName))
                {
                    invoke.Parameters.Add(new CodeSnippetExpression(viewModelReferenceName));
                }
               
            }
            return invoke;
        }
    }
}
