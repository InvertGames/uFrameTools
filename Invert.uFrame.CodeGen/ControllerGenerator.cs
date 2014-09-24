using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Invert.uFrame;
using Invert.uFrame.Editor;

public class ControllerGenerator : ElementCodeGenerator
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddController(ElementData);
        Namespace.Imports.Add(new CodeNamespaceImport("UniRx"));
        
    }

    public CodeTypeReference GetCommandTypeReference(ViewModelCommandData itemData, CodeTypeReference senderType, ElementData element)
    {
        if (!itemData.IsYield)
        {
            if (string.IsNullOrEmpty(itemData.RelatedTypeName))
            {
                //if (element.IsMultiInstance)
                //{
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.CommandWithSenderT);
                    commandWithType.TypeArguments.Add(senderType);
                    return commandWithType;
                //}
                //else
                //{
                //    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.Command);
                //    return commandWithType;
                //}

            }
            else
            {
                //if (element.IsMultiInstance)
               // {
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.CommandWithSenderAndArgument);
                    commandWithType.TypeArguments.Add(senderType);
                    var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
                    if (typeViewModel == null)
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
                    }
                    else
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
                    }

                    return commandWithType;
               // }
                //else
                //{
                //    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.CommandWith);

                //    var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
                //    if (typeViewModel == null)
                //    {
                //        commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
                //    }
                //    else
                //    {
                //        commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
                //    }

                //    return commandWithType;

                //}

            }
        }
        else
        {
            if (string.IsNullOrEmpty(itemData.RelatedTypeName))
            {
                //if (element.IsMultiInstance)
                //{
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommandWithSenderT);
                    commandWithType.TypeArguments.Add(senderType);
                    return commandWithType;
                //}
                //else
                //{
                //    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommand);

                //    return commandWithType;
                //}

            }
            else
            {
                //if (element.IsMultiInstance)
                //{
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommandWithSenderAndArgument);
                    commandWithType.TypeArguments.Add(senderType);
                    var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
                    if (typeViewModel == null)
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
                    }
                    else
                    {
                        commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
                    }
                    return commandWithType;
                //}
                //else
                //{
                //    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommandWith);
                //    var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
                //    if (typeViewModel == null)
                //    {
                //        commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
                //    }
                //    else
                //    {
                //        commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
                //    }
                //    return commandWithType;
                //}

            }
        }


    }

    public virtual void AddController(ElementData data)
    {
        var viewModelTypeReference = new CodeTypeReference(data.NameAsViewModel);
        Declaration = new CodeTypeDeclaration();

        if (IsDesignerFile)
        {
            if (!data.IsDerived)
            {
                var itemsAdded = new List<string>();
                foreach (var item in DiagramData.GetAllRegisteredElements())
                {
                    if (itemsAdded.Contains(item.Name)) continue;

                    var element = item.RelatedNode() as ElementData;
                    if (element == null) continue;

                    Declaration.Members.Add(
                        new CodeSnippetTypeMember(string.Format("[Inject(\"{1}\")] public {0} {1} {{ get; set; }}",element.NameAsViewModel, item.Name)));
                    itemsAdded.Add(item.Name);
                }
            }
            

            AddDependencyControllers(Declaration, data);
            Declaration.Name = data.NameAsControllerBase;
            Declaration.TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public;
            if (data.IsDerived)
            {
                Declaration.BaseTypes.Add( data.BaseElement.NameAsController);
            }
            else
            {
                Declaration.BaseTypes.Add(new CodeTypeReference(uFrameEditor.UFrameTypes.Controller));
            }
        }
        else
        {
            Declaration.TypeAttributes = TypeAttributes.Public;
            Declaration.Name = data.ControllerName;
            Declaration.BaseTypes.Add(new CodeTypeReference(data.NameAsControllerBase));
        }

        InitializeMethod = new CodeMemberMethod { Name = string.Format("Initialize{0}", data.Name) };
        Declaration.Members.Add(InitializeMethod);
        InitializeMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(data.NameAsViewModel), data.NameAsVariable));
        if (IsDesignerFile)
        {
            InitializeMethod.Attributes = MemberAttributes.Abstract | MemberAttributes.Public;
            AddCreateMethod(data, viewModelTypeReference, InitializeMethod, Declaration);
        }
        else
        {
            InitializeMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
        }
        if (IsDesignerFile)
        {
            InitializeDesignerMethod = new CodeMemberMethod()
            {
                Name = "Initialize",
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            Declaration.Members.Add(InitializeDesignerMethod);
            InitializeDesignerMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(uFrameEditor.UFrameTypes.ViewModel), "viewModel"));

            if (data.BaseElement != null)
            {
                InitializeDesignerMethod.Statements.Add(new CodeSnippetExpression("base.Initialize(viewModel)"));
            }
            InitializeDesignerMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), InitializeMethod.Name,
                new CodeCastExpression(new CodeTypeReference(data.NameAsViewModel), new CodeVariableReferenceExpression("viewModel"))));

        }

        if (IsDesignerFile)
        {
            AddCommandMethods(data, viewModelTypeReference, Declaration);
            AddComputedPropertyMethods(data, Declaration);
        }
       
        ProcessModifiers(Declaration);
        Namespace.Types.Add(Declaration);
    }

    public CodeMemberMethod InitializeDesignerMethod { get; set; }

    public CodeMemberMethod InitializeMethod { get; set; }

    public CodeTypeDeclaration Declaration { get; set; }

    private void AddCreateMethod(ElementData data, CodeTypeReference viewModelTypeReference,
        CodeMemberMethod initializeMethod, CodeTypeDeclaration tDecleration)
    {
        var createMethod = new CodeMemberMethod
        {
            Name = string.Format("Create{0}", data.Name),
            Attributes = MemberAttributes.Public,
            ReturnType = viewModelTypeReference
        };
        createMethod.Statements.Add(
            new CodeMethodReturnStatement(new CodeCastExpression(data.NameAsViewModel,
                new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Create"))));


        if (!ElementData.IsTemplate)
        {


            var createEmptyMethod = new CodeMemberMethod
            {
                Name = "CreateEmpty",
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
                ReturnType = new CodeTypeReference(uFrameEditor.UFrameTypes.ViewModel)
            };

            createEmptyMethod.Statements.Add(
                new CodeMethodReturnStatement(new CodeObjectCreateExpression(data.NameAsViewModel)));

            tDecleration.Members.Add(createEmptyMethod);
        }
        tDecleration.Members.Add(createMethod);
    }

    private void AddDependencyControllers(CodeTypeDeclaration tDecleration, ElementData data)
    {
        var diagramItems = DiagramData.GetElements().ToArray();

        var controllers = GetDependencyControllers(data, diagramItems);

        var baseControllers = data.AllBaseTypes.SelectMany(p => GetDependencyControllers(p as ElementData, diagramItems)).ToArray();

        foreach (var controller in controllers.Distinct())
        {
            if (baseControllers.Contains(controller)) continue;
            tDecleration.Members.Add(new CodeSnippetTypeMember(string.Format("[Inject] public {0} {0} {{get;set;}}", controller)));
        }
    }

    private List<string> GetDependencyControllers(ElementData data, ElementData[] diagramItems)
    {
        var controllers = new List<string>();

        foreach (var elementDataBase in diagramItems)
        {
            foreach (var item in elementDataBase.ViewModelItems)
            {
                if (item.RelatedType == data.Identifier)
                {
                    controllers.Add(elementDataBase.NameAsController);
                }
            }
        }
        foreach (var item in data.ViewModelItems)
        {
            var relatedNode = item.RelatedNode() as ElementData;
            if (relatedNode != null)
                controllers.Add(relatedNode.NameAsController);
        }
        return controllers;
    }
}
