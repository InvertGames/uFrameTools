using System.CodeDom;
using System.Collections;
using System.Linq;
using Invert.uFrame.CodeGen.CodeDomExtensions;
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

    protected void AddCommandMethods(ElementData data, CodeTypeReference viewModelTypeReference,
        CodeTypeDeclaration tDecleration)
    {
        foreach (var command in data.Commands)
        {
            var commandMethod = command.ToMethod(null, !IsDesignerFile, true);
            tDecleration.Members.Add(commandMethod);

            //var commandMethod = new CodeMemberMethod
            //{
            //    Name = command.Name,
            //};
            //if (!Settings.GenerateControllers)
            //{
            //    commandMethod.Name = "On" + command.Name;
            //}
          
            //var transition =
            //    DiagramData.GetSceneManagers().SelectMany(p => p.Transitions)
            //        .FirstOrDefault(p => p.CommandIdentifier == command.Identifier && !string.IsNullOrEmpty(p.ToIdentifier));

            //var hasTransition = transition != null && DiagramData.GetSceneManagers().Any(p => p.Identifier == transition.ToIdentifier);
            //if (IsDesignerFile)
            //{
            //    commandMethod.Attributes = MemberAttributes.Public;
            //}
            //else
            //{
            //    commandMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
            //}

            //if (Settings.GenerateControllers)
            //{
            //    commandMethod.Parameters.Add(new CodeParameterDeclarationExpression(viewModelTypeReference,
            //        data.NameAsVariable));
            //}

            //if (!string.IsNullOrEmpty(command.RelatedType))
            //{
            //    var relatedViewModel = command.RelatedNode() as ElementData;
            //    if (relatedViewModel == null)
            //    {
            //        commandMethod.Parameters.Add(
            //            new CodeParameterDeclarationExpression(new CodeTypeReference(command.RelatedTypeName), "arg"));
            //        if (!Settings.GenerateControllers && !IsDesignerFile)
            //        commandMethod.Statements.Insert(0,
            //            new CodeSnippetStatement(string.Format("base.{0}(arg);", commandMethod.Name)));
            //    }
            //    else
            //    {
            //        commandMethod.Parameters.Add(
            //            new CodeParameterDeclarationExpression(new CodeTypeReference(relatedViewModel.NameAsViewModel),
            //                "arg"));
            //        if (!Settings.GenerateControllers && !IsDesignerFile)
            //        commandMethod.Statements.Insert(0,
            //            new CodeSnippetStatement(string.Format("base.{0}(arg);", commandMethod.Name)));
            //    }
            //}
            //else
            //{
            //    if (!Settings.GenerateControllers && !IsDesignerFile)
            //    commandMethod.Statements.Insert(0, new CodeSnippetStatement(string.Format("base.{0}();", commandMethod.Name)));
            //}
            
        }
    }
}