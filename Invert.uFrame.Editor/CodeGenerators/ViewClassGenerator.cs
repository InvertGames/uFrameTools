using Invert.uFrame.Editor;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class ViewClassGenerator : CodeGenerator
{
    private Dictionary<string, CodeConditionStatement> _bindingConditionStatements;

    public Dictionary<string, CodeConditionStatement> BindingConditionStatements
    {
        get { return _bindingConditionStatements ?? (_bindingConditionStatements = new Dictionary<string, CodeConditionStatement>()); }
        set { _bindingConditionStatements = value; }
    }

    public List<ViewBindingExtender> BindingExtenders { get; set; }

    public CodeMemberMethod CreateModelMethod { get; set; }

    public CodeTypeDeclaration Decleration { get; set; }

    public INodeRepository DiagramData
    {
        get;
        set;
    }

    public CodeMemberProperty ViewModelProperty { get; set; }

    public CodeMemberProperty ViewModelTypeProperty { get; set; }

    public CodeConditionStatement AddBindingCondition(CodeTypeDeclaration decl, CodeStatementCollection statements, ITypedItem item, ElementDataBase relatedElement)
    {
        var bindField = new CodeMemberField
        {
            Name = "_Bind" + item.Name,
            Type = new CodeTypeReference(typeof(bool)),
            Attributes = MemberAttributes.Public,
            InitExpression = new CodePrimitiveExpression(true)
        };

        bindField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.UFrameTypes.UFToggleGroup),
            new CodeAttributeArgument(new CodePrimitiveExpression(item.Name))));
        bindField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));

        var prop = item as ViewModelPropertyData;
        var coll = item as ViewModelCollectionData;
        if (prop != null)
        {
            if (relatedElement == null)
            {
                bindField.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.UFrameTypes.UFRequireInstanceMethod),
                        new CodeAttributeArgument(new CodePrimitiveExpression(prop.NameAsChangedMethod))));
            }
        }
        else if (coll != null)
        {
            //bindField.CustomAttributes.Add(
            //  new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFRequireInstanceMethod)),
            //      new CodeAttributeArgument(new CodePrimitiveExpression(relatedElement == null ? coll.NameAsAddHandler : coll.NameAsCreateHandler))));
        }

        decl.Members.Add(bindField);
        var conditionStatement = new CodeConditionStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), bindField.Name));
        statements.Add(conditionStatement);
        return conditionStatement;
    }

    public void AddCreateModelMethod(ElementData data)
    {
        CreateModelMethod = new CodeMemberMethod()
        {
            Name = "CreateModel",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            ReturnType = new CodeTypeReference(uFrameEditor.UFrameTypes.ViewModel)
        };

        //if (data.IsMultiInstance)
        //{
        if (Settings.GenerateControllers)
        {
            CreateModelMethod.Statements.Add(
            new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                "RequestViewModel",
                new CodeSnippetExpression(string.Format("GameManager.Container.Resolve<{0}>()",
                    data.NameAsController)))));
        }
        else
        {
            CreateModelMethod.Statements.Add(
            new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                "RequestViewModel",
                new CodeSnippetExpression("null"))));
        }

        Decleration.Members.Add(CreateModelMethod);
    }


    public CodeMemberField AddPropertyBindingField(CodeTypeDeclaration decl, string typeFullName, string propertyName, string name, bool keepHidden = false)
    {
        var memberField =
            new CodeMemberField(
                typeFullName,
                "_" + propertyName + name) { Attributes = MemberAttributes.Public };
        if (!keepHidden)
        {
            memberField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.UFrameTypes.UFGroup),
                new CodeAttributeArgument(new CodePrimitiveExpression(propertyName))));
        }

        memberField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));

        decl.Members.Add(memberField);
        return memberField;
    }

    public void AddViewBase(ElementData data, string className = null, string baseClassName = null)
    {
        Decleration = new CodeTypeDeclaration(className ?? data.NameAsViewBase)
        {
            TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public
        };

        if (data.IsDerived)
        {
            try
            {
                var baseType = DiagramData.GetAllElements().First(p => p.Name == data.BaseTypeName);
                Decleration.BaseTypes.Add(new CodeTypeReference(baseClassName ?? baseType.NameAsViewBase));
            }
            catch (Exception ex)
            {
                data.BaseIdentifier = null;
                Decleration.BaseTypes.Add(new CodeTypeReference(uFrameEditor.UFrameTypes.ViewBase));
            }
        }
        else
        {
            Decleration.BaseTypes.Add(new CodeTypeReference(uFrameEditor.UFrameTypes.ViewBase));
        }

        if (IsDesignerFile)
        {
            Decleration.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.UFrameTypes.DiagramInfoAttribute),
                new CodeAttributeArgument(new CodePrimitiveExpression(DiagramData.Name))));

            AddDefaultIdentifierProperty(data);
        }

        AddViewModelTypeProperty(data);

        

        //AddMultiInstanceProperty(data);

        AddViewModelProperty(data);

        AddCreateModelMethod(data);

        AddInitializeViewModelMethod(data);

        AddExecuteMethods(data, Decleration);

        Namespace.Types.Add(Decleration);
    }

    public void AddViewModelProperty(ElementData data)
    {



        ViewModelProperty = new CodeMemberProperty
         {
             Name = data.Name,
             Attributes = MemberAttributes.Public | MemberAttributes.Final,
             Type = new CodeTypeReference(data.NameAsViewModel)
         };
        ViewModelProperty.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeCastExpression(new CodeTypeReference(data.NameAsViewModel),
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ViewModelObject"))));
        ViewModelProperty.SetStatements.Add(
            new CodeAssignStatement(
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ViewModelObject"),
                new CodePropertySetValueReferenceExpression()));

        Decleration.Members.Add(ViewModelProperty);
    }

    public CodeMemberMethod CreateUpdateMethod(ViewData view, CodeTypeDeclaration decl)
    {
        var updateMethod = new CodeMemberMethod()
        {
            Attributes = MemberAttributes.Family | MemberAttributes.Override,
            Name = "Apply"
        };
        updateMethod.Statements.Add(new CodeSnippetExpression("base.Apply()"));

        var element = view.ViewForElement;
        var dirtyCondition =
            new CodeConditionStatement(new CodeSnippetExpression(string.Format("{0}.Dirty", element.Name)));

        updateMethod.Statements.Add(dirtyCondition);

        decl.Members.Add(updateMethod);
        return updateMethod;
    }

    public bool HasField(CodeTypeMemberCollection collection, string name)
    {
        return collection.OfType<CodeMemberField>().Any(item => item.Name == name);
    }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        BindingExtenders = uFrameEditor.Container.ResolveAll<ViewBindingExtender>().Where(p => p.Initialize(this)).ToList();
        Namespace.Imports.Add(new CodeNamespaceImport("UnityEngine"));
        Namespace.Imports.Add(new CodeNamespaceImport("UniRx"));
    }



    protected void AddDefaultIdentifierProperty(ElementData data)
    {
        var defaultInstance = data.RegisteredInstances.FirstOrDefault();
        if (defaultInstance == null) return;

        var defaultIdentifierProperty = new CodeMemberProperty()
        {
            Name = "DefaultIdentifier",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            Type = new CodeTypeReference(typeof(string))
        };

        defaultIdentifierProperty.GetStatements.Add(
            new CodeMethodReturnStatement(new CodePrimitiveExpression(defaultInstance.Name)));

        Decleration.Members.Add(defaultIdentifierProperty);

    }

    protected void AddExecuteMethods(ElementData data, CodeTypeDeclaration decl, bool useViewReference = false)
    {
        foreach (var viewModelCommandData in data.Commands)
        {
            var executeMethod = CreateExecuteMethod(data, useViewReference, viewModelCommandData);
            decl.Members.Add(executeMethod);
        }
    }

    protected void AddInitializeViewModelMethod(ElementData data)
    {
        var initializeViewModelMethod = new CodeMemberMethod
        {
            Name = "InitializeViewModel",
            Attributes = MemberAttributes.Override | MemberAttributes.Family
        };

        initializeViewModelMethod.Parameters.Add(
            new CodeParameterDeclarationExpression(new CodeTypeReference(uFrameEditor.UFrameTypes.ViewModel), "viewModel"));

        if (data.IsDerived)
        {
            var baseCall = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(),
                initializeViewModelMethod.Name);
            baseCall.Parameters.Add(new CodeVariableReferenceExpression("viewModel"));
            initializeViewModelMethod.Statements.Add(baseCall);
        }

        if (data.Properties.Count > 0)
            initializeViewModelMethod.Statements.Add(
                new CodeVariableDeclarationStatement(
                    new CodeTypeReference(data.NameAsViewModel), data.NameAsVariable, new CodeCastExpression(
                        new CodeTypeReference(data.NameAsViewModel),
                        new CodeVariableReferenceExpression("viewModel"))));
        Decleration.Members.Add(initializeViewModelMethod);

        foreach (var property in data.Properties)
        {
            if (property.ViewIdentifier != null) continue;

            var relatedNode = property.RelatedNode();
            var relatedViewModel = relatedNode as ElementData;

            if (relatedViewModel == null) // Non ViewModel Properties
            {
                if (relatedNode != null && !(relatedNode is IViewPropertyType)) continue;
               

                var field = new CodeMemberField(new CodeTypeReference(property.RelatedTypeName),
                    property.ViewFieldName) { Attributes = MemberAttributes.Public };

                field.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.UFrameTypes.UFGroup),
                        new CodeAttributeArgument(new CodePrimitiveExpression("View Model Properties"))));
                field.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));
                Decleration.Members.Add(field);

                initializeViewModelMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(
                    new CodeVariableReferenceExpression(data.NameAsVariable), property.Name),
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), property.ViewFieldName)));
            }
            else
            {
                var field = new CodeMemberField(new CodeTypeReference(uFrameEditor.UFrameTypes.ViewBase),
                    property.ViewFieldName) { Attributes = MemberAttributes.Public };

                field.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.UFrameTypes.UFGroup),
                        new CodeAttributeArgument(new CodePrimitiveExpression("View Model Properties"))));
                field.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));
                Decleration.Members.Add(field);
                initializeViewModelMethod.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(
                    new CodeVariableReferenceExpression(data.NameAsVariable), property.Name),
                    new CodeSnippetExpression(string.Format("this.{0} == null ? null : this.{0}.ViewModelObject as {1}",
                        property.ViewFieldName, relatedViewModel.NameAsViewModel))));
            }
        }
    }

    protected void AddViewModelTypeProperty(ElementData data)
    {
        ViewModelTypeProperty = new CodeMemberProperty
        {
            Attributes = MemberAttributes.Override | MemberAttributes.Public,
            Type = new CodeTypeReference(typeof(Type)),
            Name = "ViewModelType"
        };
        ViewModelTypeProperty.HasSet = false;
        ViewModelTypeProperty.HasGet = true;
        ViewModelTypeProperty.GetStatements.Add(
            new CodeSnippetExpression(string.Format("return typeof({0})", data.NameAsViewModel)));
        Decleration.Members.Add(ViewModelTypeProperty);
    }

    protected CodeMemberMethod CreateExecuteMethod(ElementData data, bool useViewReference,
        ViewModelCommandData viewModelCommandData)
    {
        ElementDataBase relatedElement;
        return CreateExecuteMethod(data, useViewReference, viewModelCommandData, out relatedElement);
    }

    protected CodeMemberMethod CreateExecuteMethod(ElementData data, bool useViewReference,
        ViewModelCommandData viewModelCommandData, out ElementDataBase relatedElement)
    {
        var executeMethod = new CodeMemberMethod
        {
            Name = viewModelCommandData.NameAsExecuteMethod,
            Attributes = MemberAttributes.Public
        };

        CodeExpression executeCommandReference = new CodeThisReferenceExpression();
        if (useViewReference)
            executeCommandReference = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "View");

        relatedElement = viewModelCommandData.RelatedNode() as ElementData;

        if (relatedElement == null)
        {
            if (!string.IsNullOrEmpty(viewModelCommandData.RelatedType))
            {
                executeMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                    viewModelCommandData.RelatedTypeName, "arg"));

                executeMethod.Statements.Add(new CodeMethodInvokeExpression(
                    executeCommandReference, "ExecuteCommand",
                    new CodeSnippetExpression(string.Format("{0}.{1}", data.Name, viewModelCommandData.Name)),
                    new CodeVariableReferenceExpression("arg")
                    ));
            }
            else
            {
                executeMethod.Statements.Add(new CodeMethodInvokeExpression(
                    executeCommandReference, "ExecuteCommand",
                    new CodeSnippetExpression(string.Format("{0}.{1}", data.Name, viewModelCommandData.Name))
                    ));
            }
        }
        else
        {
            executeMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                relatedElement.NameAsViewModel, relatedElement.NameAsVariable));

            executeMethod.Statements.Add(new CodeMethodInvokeExpression(
                executeCommandReference, "ExecuteCommand",
                new CodeSnippetExpression(string.Format("{0}.{1}", data.Name, viewModelCommandData.Name)),
                new CodeVariableReferenceExpression(relatedElement.NameAsVariable)
                ));
        }
        return executeMethod;
    }


    protected CodeMemberMethod GenerateBindMethod(CodeTypeDeclaration decl, ViewData data)
    {
        var preBindMethod = new CodeMemberMethod
        {
            Name = "Bind",
            Attributes = MemberAttributes.Public | MemberAttributes.Override
        };

        preBindMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Bind"));


        foreach (var viewProperty in data.SceneProperties)
        {
            var resetMethod = new CodeMemberMethod()
            {
                Name = string.Format("Reset{0}", viewProperty.Name),
                Attributes = MemberAttributes.Public
            };
            Decleration.Members.Add(resetMethod);
            var disposeField = new CodeMemberField()
            {
                Name = string.Format("_{0}Disposable", viewProperty.Name),
                Attributes = MemberAttributes.Private,
                Type = new CodeTypeReference("IDisposable")

            };
            Decleration.Members.Add(disposeField);

            resetMethod.Statements.Add(
                  new CodeSnippetExpression(string.Format("if ({0} != null) {0}.Dispose()", disposeField.Name)));

           
            var getMethod = new CodeMemberMethod()
            {
                Name = string.Format("Calculate{0}", viewProperty.Name),
                ReturnType = viewProperty.GetPropertyType(),
                Attributes = MemberAttributes.Family,

            };

            getMethod.Statements.Add(
                new CodeSnippetExpression(string.Format("return default({0})", viewProperty.RelatedTypeName)));

            Decleration.Members.Add(getMethod);

            var returnType = new CodeTypeReference(uFrameEditor.UFrameTypes.IObservable);
            returnType.TypeArguments.Add(getMethod.ReturnType);
            var createObservableMethod = new CodeMemberMethod()
            {
                Name = string.Format("Get{0}Observable", viewProperty.Name),
                Attributes = MemberAttributes.Family,
                ReturnType = returnType
            };


            createObservableMethod.Statements.Add(
                new CodeSnippetExpression(string.Format("return this.UpdateAsObservable().Select(p => {0}())", getMethod.Name)));

            Decleration.Members.Add(createObservableMethod);

            
            resetMethod.Statements.Add(
                new CodeSnippetExpression(string.Format("{3} = {0}().Subscribe({1}.{2}).DisposeWith(this)", createObservableMethod.Name,
                    data.ViewForElement.Name, viewProperty.FieldName,disposeField.Name)));
            preBindMethod.Statements.Add(new CodeSnippetExpression(string.Format("{0}()", resetMethod.Name)));
        }

        var bindingGenerators = uFrameEditor.GetBindingGeneratorsForView(data);

        foreach (var bindingGenerator in bindingGenerators)
        {
            bindingGenerator.IsOverride = !IsDesignerFile;
            CodeConditionStatement bindingCondition = null;
            if (this.BindingConditionStatements.ContainsKey(bindingGenerator.BindingConditionFieldName))
            {
                bindingCondition = this.BindingConditionStatements[bindingGenerator.BindingConditionFieldName];
            }
            else
            {
                bindingCondition = AddBindingCondition(decl, preBindMethod.Statements, bindingGenerator.Item,
                   bindingGenerator.Item.RelatedNode() as ElementData);


                BindingConditionStatements.Add(bindingGenerator.BindingConditionFieldName, bindingCondition);
            }
            //if (HasField(Decleration.Members, bindingGenerator.BindingConditionFieldName)) continue;

            bindingGenerator.CreateBindingStatement(decl.Members, bindingCondition);
        }
        decl.Members.Add(preBindMethod);
        return preBindMethod;
        //foreach (var property in data.Properties)
        //{
        //    var relatedElement = DiagramData.GetAllElements().FirstOrDefault(p => p.Name == property.RelatedTypeName);
        //    var bindingCondition = AddBindingCondition(decl, bindMethod.Statements, property, relatedElement);

        //    //var twoWayField = AddPropertyBindingField(decl, typeof(bool).FullName, property.Name, "IsTwoWay");
        //    //twoWayField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(UFRequireInstanceMethod)),
        //    //    new CodeAttributeArgument(new CodePrimitiveExpression(property.NameAsTwoWayMethod))));

        //    //var twoWayCondition =
        //    //    new CodeConditionStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),
        //    //        twoWayField.Name));

        //    //bindingCondition.TrueStatements.Add(twoWayCondition);

        //    //AddPropertyBinding(decl, data.Name, bindingCondition.FalseStatements, property, false, relatedElement);
        //    AddPropertyBinding(data, decl, bindingCondition.TrueStatements, property, false, relatedElement);
        //}
        //foreach (var collectionProperty in data.Collections)
        //{
        //    var relatedElement = DiagramData.GetAllElements().FirstOrDefault(p => p.Name == collectionProperty.RelatedTypeName);
        //    var bindingCondition = AddBindingCondition(decl, bindMethod.Statements, collectionProperty, relatedElement);

        //    if (relatedElement == null)
        //    {
        //        AddCollectionBinding(decl, data.Name, bindingCondition.TrueStatements, collectionProperty, relatedElement);
        //    }
        //    else
        //    {
        //        AddCollectionBinding(decl, data.Name, bindingCondition.TrueStatements, collectionProperty, relatedElement);
        //    }
        //}
    }


}