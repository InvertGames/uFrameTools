using System.CodeDom;
using System.Collections;
using System.Linq;
using Invert.uFrame.Editor;

public class ElementCodeGenerator : CodeGenerator
{
    public ElementData ElementData
    {
        get;
        set;
    }

    public INodeRepository DiagramData
    {
        get;
        set;
    }

    protected void AddComputedPropertyMethods(ElementData data, CodeTypeDeclaration tDecleration)
    {
        foreach (var computedProperty in data.ComputedProperties)
        {
            var computeMethod = new CodeMemberMethod()
            {
                Name = computedProperty.NameAsComputeMethod,
                ReturnType = new CodeTypeReference(computedProperty.RelatedTypeNameOrViewModel)
            };

            if (Settings.GenerateControllers)
            {
                computeMethod.Parameters.Add(new CodeParameterDeclarationExpression(data.NameAsViewModel, "vm"));
            }
            
            if (IsDesignerFile)
            {
                computeMethod.Attributes = MemberAttributes.Public;
            }
            else
            {
                computeMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            }

            computeMethod.Statements.Add(
                new CodeSnippetExpression(string.Format("return default({0})", computedProperty.RelatedTypeNameOrViewModel)));

            tDecleration.Members.Add(computeMethod);
        }
    }

    protected void AddCommandMethods(ElementData data, CodeTypeReference viewModelTypeReference,
        CodeTypeDeclaration tDecleration)
    {
        foreach (var command in data.Commands)
        {
            var commandMethod = new CodeMemberMethod
            {
                Name = command.Name,
            };
            if (!Settings.GenerateControllers)
            {
                commandMethod.Name = "On" + command.Name;
            }
            if (command.IsYield)
            {
                commandMethod.ReturnType = new CodeTypeReference(typeof(IEnumerator));
            }
            var transition =
                DiagramData.GetSceneManagers().SelectMany(p => p.Transitions)
                    .FirstOrDefault(p => p.CommandIdentifier == command.Identifier && !string.IsNullOrEmpty(p.ToIdentifier));

            var hasTransition = transition != null && DiagramData.GetSceneManagers().Any(p => p.Identifier == transition.ToIdentifier);
            if (IsDesignerFile)
            {
                commandMethod.Attributes = MemberAttributes.Public;
            }
            else
            {
                commandMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            }

            if (Settings.GenerateControllers)
            {
                commandMethod.Parameters.Add(new CodeParameterDeclarationExpression(viewModelTypeReference,
                    data.NameAsVariable));
            }

            // TODO this should propably be injection but for now whatever
            // Add transition code
            if (IsDesignerFile && hasTransition)
            {
                commandMethod.Attributes = MemberAttributes.Public;

                commandMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                    "GameEvent", new CodePrimitiveExpression(transition.Name)));
            }
            if (IsDesignerFile && command.IsYield)
            {
                commandMethod.Statements.Add(new CodeSnippetExpression("yield break"));
            }
            //if (!IsDesignerFile)
            //{
            //    commandMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(),commandMethod.Name,))
            //}
            if (!string.IsNullOrEmpty(command.RelatedType))
            {
                var relatedViewModel = command.RelatedNode() as ElementData;
                if (relatedViewModel == null)
                {
                    commandMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(new CodeTypeReference(command.RelatedTypeName), "arg"));
                    if (!Settings.GenerateControllers && !IsDesignerFile)
                    commandMethod.Statements.Insert(0,
                        new CodeSnippetStatement(string.Format("base.{0}(arg);", commandMethod.Name)));
                }
                else
                {
                    commandMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(new CodeTypeReference(relatedViewModel.NameAsViewModel),
                            "arg"));
                    if (!Settings.GenerateControllers && !IsDesignerFile)
                    commandMethod.Statements.Insert(0,
                        new CodeSnippetStatement(string.Format("base.{0}(arg);", commandMethod.Name)));
                }
            }
            else
            {
                if (!Settings.GenerateControllers && !IsDesignerFile)
                commandMethod.Statements.Insert(0, new CodeSnippetStatement(string.Format("base.{0}();", commandMethod.Name)));
            }
            tDecleration.Members.Add(commandMethod);
        }
    }
}