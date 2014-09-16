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
        var tDecleration = new CodeTypeDeclaration();

        if (IsDesignerFile)
        {
            if (!data.IsDerived)
            {
                foreach (var item in DiagramData.GetAllRegisteredElements())
                {
                    UnityEngine.Debug.Log("Creating Property" + item.Name);
                    var element = item.RelatedNode() as ElementData;
                    if (element == null) continue;

                    tDecleration.Members.Add(
                        new CodeSnippetTypeMember(string.Format("[Inject(\"{1}\")] {0} {1} {{ get; set; }}",element.NameAsViewModel, item.Name)));

                }
            }
            

            AddDependencyControllers(tDecleration, data);
            tDecleration.Name = data.NameAsControllerBase;
            tDecleration.TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public;
            if (data.IsDerived)
            {
                tDecleration.BaseTypes.Add( data.BaseElement.NameAsController);
            }
            else
            {
                tDecleration.BaseTypes.Add(new CodeTypeReference(uFrameEditor.UFrameTypes.Controller));
            }

            //if (!data.IsMultiInstance)
            //{
            //    var property = new CodeMemberProperty
            //    {
            //        Name = data.Name,
            //        Type = new CodeTypeReference(data.NameAsViewModel),
            //        HasGet = true,
            //        HasSet = false,
            //        Attributes = MemberAttributes.Public
            //    };
            //    property.GetStatements.Add(
            //        new CodeMethodReturnStatement(
            //            new CodeSnippetExpression(string.Format("Container.Resolve<{0}>()", data.NameAsViewModel))));//,data.RootElement.Name))));

                
            //    tDecleration.Members.Add(property);
            //}
        }
        else
        {
            tDecleration.TypeAttributes = TypeAttributes.Public;
            tDecleration.Name = data.ControllerName;
            tDecleration.BaseTypes.Add(new CodeTypeReference(data.NameAsControllerBase));
        }

        var initializeTypedMethod = new CodeMemberMethod { Name = string.Format("Initialize{0}", data.Name) };
        tDecleration.Members.Add(initializeTypedMethod);
        initializeTypedMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(data.NameAsViewModel), data.NameAsVariable));
        if (IsDesignerFile)
        {
            initializeTypedMethod.Attributes = MemberAttributes.Abstract | MemberAttributes.Public;

            //AddWireCommandsMethod(data, tDecleration, viewModelTypeReference);
            AddCreateMethod(data, viewModelTypeReference, initializeTypedMethod, tDecleration);
        }
        else
        {
            initializeTypedMethod.Attributes = MemberAttributes.Public | MemberAttributes.Override;
        }
        if (IsDesignerFile)
        {
            var initializeOverrideMethod = new CodeMemberMethod()
            {
                Name = "Initialize",
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            tDecleration.Members.Add(initializeOverrideMethod);
            initializeOverrideMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(uFrameEditor.UFrameTypes.ViewModel), "viewModel"));

            if (data.BaseElement != null)
            {
                initializeOverrideMethod.Statements.Add(new CodeSnippetExpression("base.Initialize(viewModel)"));
            }
            initializeOverrideMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), initializeTypedMethod.Name,
                new CodeCastExpression(new CodeTypeReference(data.NameAsViewModel), new CodeVariableReferenceExpression("viewModel"))));

        }


        // Command functions
        if (IsDesignerFile)
        {
            AddCommandMethods(data, viewModelTypeReference, tDecleration);
            AddComputedPropertyMethods(data, tDecleration);
        }
       

        ProcessModifiers(tDecleration);

        Namespace.Types.Add(tDecleration);
    }

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
