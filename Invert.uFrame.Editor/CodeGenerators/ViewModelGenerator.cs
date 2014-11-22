using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ViewModelGenerator : ElementCodeGenerator
{
    public static Dictionary<Type, string> AcceptableTypes = new Dictionary<Type, string>
    {
        {typeof(int),"Int" },
        {typeof(Vector3),"Vector3" },
        {typeof(Vector2),"Vector2" },
        {typeof(string),"String" },
        {typeof(bool),"Bool" },
        {typeof(float),"Float" },
        {typeof(double),"Double" },
        {typeof(Quaternion),"Quaternion" },
    };

    public CodeConstructor ConstructorWithController { get; set; }

    public CodeTypeDeclaration BaseTypeDeclaration { get; set; }
    public CodeTypeDeclaration Declaration { get; set; }
    public CodeMemberMethod FillCommandsMethod { get; set; }

    public CodeMemberMethod FillPropertiesMethod { get; set; }

    public CodeMemberMethod ReadMethod { get; set; }

    public CodeMemberMethod SetParentMethod { get; set; }

    public CodeMemberMethod UnBindMethod { get; set; }

    public CodeMemberMethod BindMethod { get; set; }

    public CodeMemberMethod WireCommandsMethod { get; set; }

    public CodeMemberMethod WriteMethod { get; set; }

    public ViewModelGenerator(bool isDesignerFile, ElementData data)
    {
        IsDesignerFile = isDesignerFile;
        ElementData = data;
    }

    public virtual void AddSetParentMethod(ElementData data)
    {
        SetParentMethod = new CodeMemberMethod()
        {
            Name = "SetParent",
            Attributes = MemberAttributes.Override | MemberAttributes.Public
        };
        SetParentMethod.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.ViewModel, "viewModel"));

        var baseInvoker = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(),
            SetParentMethod.Name);
        baseInvoker.Parameters.Add(new CodeVariableReferenceExpression("viewModel"));
        SetParentMethod.Statements.Add(baseInvoker);
    }

    public virtual void AddViewModel(ElementData data)
    {
        BaseTypeDeclaration = new CodeTypeDeclaration(data.NameAsViewModel) { IsPartial = true };
        if (IsDesignerFile)
        {
            AddConstructors(BaseTypeDeclaration);
            CreateBindMethod();
            BaseTypeDeclaration.Name = data.NameAsViewModelBase;
            BaseTypeDeclaration.IsPartial = false;

            var baseType = data.BaseElement;
            if (baseType != null)
            {
                BaseTypeDeclaration.BaseTypes.Add(baseType.NameAsViewModel);
            }
            else
            {
                BaseTypeDeclaration.BaseTypes.Add(uFrameEditor.UFrameTypes.ViewModel);
            }

            BaseTypeDeclaration.CustomAttributes.Add(
                new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.UFrameTypes.DiagramInfoAttribute),
                    new CodeAttributeArgument(new CodePrimitiveExpression(DiagramData.Name))));

            
            AddComputedProperties(BaseTypeDeclaration, data);

            

        }
        ProcessModifiers(BaseTypeDeclaration);

        Namespace.Types.Add(BaseTypeDeclaration);
    }

    private void AddUnbindMethod(CodeTypeDeclaration decleration)
    {
        UnBindMethod.Statements.Add(new CodeSnippetExpression("base.Unbind()"));
        decleration.Members.Add(UnBindMethod);
    }

    private void AddConstructors(CodeTypeDeclaration decleration)
    {
        
        ConstructorWithController = new CodeConstructor()
        {
            Name = decleration.Name,
            Attributes = MemberAttributes.Public
        };
        
        ConstructorWithController.Parameters.Add(new CodeParameterDeclarationExpression(ElementData.NameAsControllerBase,
            "controller"));


        ConstructorWithController.Parameters.Add(new CodeParameterDeclarationExpression(typeof(bool),
            "initialize = true"));

        //ConstructorWithController.Statements.Add(
        //    new CodeAssignStatement(
        //        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Controller"),
        //        new CodeSnippetExpression("controller")));

        ConstructorWithController.BaseConstructorArgs.Add(new CodeSnippetExpression("controller"));
        ConstructorWithController.BaseConstructorArgs.Add(new CodeSnippetExpression("initialize"));

        decleration.Members.Add(ConstructorWithController);

        ConstructorWithOutController = new CodeConstructor()
        {
            Name = decleration.Name,
            Attributes = MemberAttributes.Public
        };
        ConstructorWithOutController.BaseConstructorArgs.Add(new CodeSnippetExpression(""));
        //ConstructorWithOutController.Statements.Add(
        //    new CodeAssignStatement(
        //        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Controller"),
        //        new CodeSnippetExpression("null")));

        decleration.Members.Add(ConstructorWithOutController);
    }

    private void CreateBindMethod()
    {

        BindMethod = new CodeMemberMethod()
        {
            Name = "Bind",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
        };
        BindMethod.Statements.Add(new CodeSnippetExpression("base.Bind()"));
//        ViewModelConstructor.BaseConstructorArgs.Add(new CodeSnippetExpression(""));
        BaseTypeDeclaration.Members.Add(BindMethod);
    }

    public CodeConstructor ConstructorWithOutController { get; set; }

    private void AddParentProperties(CodeTypeDeclaration decleration, ElementData data)
    {
        var baseElements = data.AllBaseTypes.SelectMany(p => p.ParentElements).ToArray();
        foreach (var parentElement in data.ParentElements)
        {
            if (baseElements.Contains(parentElement)) continue;

            var parentField = new CodeMemberField(parentElement.NameAsViewModel, "_Parent" + parentElement.Name);
            var property = parentField.EncapsulateField("Parent" + parentElement.Name);
            decleration.Members.Add(parentField);
            decleration.Members.Add(property);
        }
    }

    private void AddComputedProperties(CodeTypeDeclaration decl, ElementData data)
    {
        foreach (var computedProperty in data.ComputedProperties)
        {
            var resetMethod = new CodeMemberMethod()
            {
                Name = string.Format("Reset{0}", computedProperty.Name),
                Attributes = MemberAttributes.Public
            };
            var disposeField = new CodeMemberField()
            {
                Name = string.Format("_{0}Disposable", computedProperty.Name),
                Attributes = MemberAttributes.Private,
                Type = new CodeTypeReference("IDisposable")
            };
            decl.Members.Add(disposeField);

            resetMethod.Statements.Add(
                new CodeSnippetExpression(string.Format("if ({0} != null) {0}.Dispose()", disposeField.Name)));

            resetMethod.Statements.Add(
                new CodeSnippetExpression(
                    string.Format("{3} = {0}.ToComputed( {1}, this.Get{2}Dependents().ToArray() ).DisposeWith(this)",
                        computedProperty.FieldName,
                        computedProperty.NameAsComputeMethod,
                        computedProperty.Name,
                        disposeField.Name)));

           

            decl.Members.Add(resetMethod);
        }
    }

    public virtual void AddWireCommandsMethod(ElementData data, CodeTypeDeclaration tDecleration,
       CodeTypeReference viewModelTypeReference)
    {
        WireCommandsMethod = new CodeMemberMethod { Name = string.Format("WireCommands") };
        tDecleration.Members.Add(WireCommandsMethod);
        WireCommandsMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(uFrameEditor.UFrameTypes.Controller),
            "controller"));
        if (data.IsDerived)
        {
            WireCommandsMethod.Attributes = MemberAttributes.Family | MemberAttributes.Override;
            var callBase = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), WireCommandsMethod.Name,
                new CodeVariableReferenceExpression("controller"));
            WireCommandsMethod.Statements.Add(callBase);
        }
        else
        {
            WireCommandsMethod.Attributes = MemberAttributes.Family | MemberAttributes.Override;
        }
        if (data.Commands.Count > 0 || data.ComputedProperties.Any())
        {
            if (Settings.GenerateControllers)
                WireCommandsMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format("var {0} = controller as {1}", data.NameAsVariable, data.NameAsControllerBase)));
        }

        foreach (var command in data.Commands)
        {
            var commandName = Settings.GenerateControllers ? command.Name : "On" + command.Name;
            var assigner = new CodeAssignStatement
            {
                Left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), command.Name)
            };
            var commandWithType = GetCommandTypeReference(command, viewModelTypeReference, data);
            var commandWith = new CodeObjectCreateExpression(commandWithType);
            // if (data.IsMultiInstance)
            // {
            commandWith.Parameters.Add(new CodeThisReferenceExpression());
            // }
            if (Settings.GenerateControllers)
            {
                commandWith.Parameters.Add(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(data.NameAsVariable), commandName));
            }
            else
            {
                commandWith.Parameters.Add(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), commandName));
            }

            assigner.Right = commandWith;
            WireCommandsMethod.Statements.Add(assigner);
        }

        
    }

    private void BindTransitions()
    {
        var element = ElementData;
        var stateMachines =
            ElementData.Properties.Select(p => p.RelatedNode()).OfType<StateMachineNodeData>().ToArray();
        var properties = ElementData.ViewModelItems.ToArray();

        foreach (var stateMachine in stateMachines)
        {
            var stateMachineProperty =
                element.Properties.FirstOrDefault(p => p.RelatedType == stateMachine.Identifier);

            if (stateMachineProperty == null) continue;

            foreach (var transition in stateMachine.Transitions)
            {
                var transitionProperties =
                    properties.Where(p => transition[p.Identifier]).ToArray();

                foreach (var transitionProperty in transitionProperties)
                {

                    if (transitionProperty is ComputedPropertyData)
                    {
                        BindMethod.Statements.Add(
                            new CodeSnippetExpression(string.Format("this.{1}.{2}.AddComputer({0})",
                                transitionProperty.FieldName, stateMachineProperty.FieldName, transition.Name)));
                    }
                    else
                    {
                        BindMethod.Statements.Add(
                            new CodeSnippetExpression(string.Format("this.{0}.Subscribe({1}.{2})",
                                transitionProperty.FieldName, stateMachineProperty.FieldName, transition.Name)));
                    }
                  
                }
            }
        }
    }

    public virtual void AddWriteMethod(CodeTypeDeclaration decl, ElementData data)
    {
        ReadMethod = new CodeMemberMethod()
        {
            Name = "Read",
            Attributes = MemberAttributes.Override | MemberAttributes.Public
        };
        ReadMethod.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.ISerializerStream, "stream"));

        WriteMethod = new CodeMemberMethod()
        {
            Name = "Write",
            Attributes = MemberAttributes.Override | MemberAttributes.Public
        };
        WriteMethod.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.ISerializerStream, "stream"));
        WriteMethod.Statements.Add(new CodeSnippetStatement("\t\tbase.Write(stream);"));
        ReadMethod.Statements.Add(new CodeSnippetStatement("\t\tbase.Read(stream);"));

        foreach (var viewModelPropertyData in data.SerializedProperties)
        {
            
            var relatedNode = viewModelPropertyData.TypeNode();
            if (relatedNode is EnumData)
            {
                var statement = new CodeSnippetStatement(string.Format("\t\tstream.SerializeInt(\"{0}\", (int)this.{0});", viewModelPropertyData.Name));
                WriteMethod.Statements.Add(statement);

                var dstatement = new CodeSnippetStatement(string.Format("\t\tthis.{0} = ({1})stream.DeserializeInt(\"{0}\");", viewModelPropertyData.Name, viewModelPropertyData.RelatedTypeName));
                ReadMethod.Statements.Add(dstatement);
            }
            else if (relatedNode is ElementData)
            {
                var elementNode = relatedNode as ElementData;
                var statement = new CodeSnippetStatement(string.Format("\t\tif (stream.DeepSerialize) stream.SerializeObject(\"{0}\", this.{0});", viewModelPropertyData.Name));
                WriteMethod.Statements.Add(statement);

                var dstatement = new CodeSnippetStatement(string.Format("\t\tif (stream.DeepSerialize) this.{0} = stream.DeserializeObject<{1}>(\"{0}\");", viewModelPropertyData.Name, elementNode.NameAsViewModel));
                ReadMethod.Statements.Add(dstatement);
            }
            else if (relatedNode is StateMachineNodeData)
            {
                
                var statement = new CodeSnippetExpression(string.Format("stream.SerializeString(\"{0}\", this.{0}.Name);", viewModelPropertyData.Name));
                WriteMethod.Statements.Add(statement);

                var dstatement = new CodeSnippetExpression(string.Format("this.{0}.SetState(stream.DeserializeString(\"{1}\"))", viewModelPropertyData.FieldName, viewModelPropertyData.Name));
                ReadMethod.Statements.Add(dstatement);
            }
            else
            {
                if (viewModelPropertyData.Type == null)
                {
                    
                    UnityEngine.Debug.Log(viewModelPropertyData.Name + " has a null type");
                    continue;
                }
                if (!AcceptableTypes.ContainsKey(viewModelPropertyData.Type)) continue;
                //viewModelPropertyData.IsEnum(data.OwnerData);
                var statement = new CodeSnippetExpression(string.Format("stream.Serialize{0}(\"{1}\", this.{1})", AcceptableTypes[viewModelPropertyData.Type], viewModelPropertyData.Name));
                WriteMethod.Statements.Add(statement);

                var dstatement = new CodeSnippetExpression(string.Format("\t\tthis.{0} = stream.Deserialize{1}(\"{0}\");", viewModelPropertyData.Name, AcceptableTypes[viewModelPropertyData.Type]));
                ReadMethod.Statements.Add(dstatement);
            }
        }
        foreach (var collection in data.Collections)
        {
            var relatedNode = collection.RelatedNode();
            if (relatedNode is EnumData)
            {
                //var statement = new CodeSnippetStatement(string.Format("\t\tstream.SerializeInt(\"{0}\", (int)this.{0});", viewModelPropertyData.Name));
                //writeMethod.Statements.Add(statement);

                //var dstatement = new CodeSnippetStatement(string.Format("\t\tthis.{0} = ({1})stream.DeserializeInt(\"{0}\");", viewModelPropertyData.Name, viewModelPropertyData.RelatedTypeName));
                //readMethod.Statements.Add(dstatement);
            }
            else if (relatedNode is ElementData)
            {
                var elementNode = relatedNode as ElementData;
                var statement = new CodeSnippetExpression(string.Format("if (stream.DeepSerialize) stream.SerializeArray(\"{0}\", this.{0})", collection.Name));
                WriteMethod.Statements.Add(statement);
                ReadMethod.Statements.Add(new CodeSnippetStatement("if (stream.DeepSerialize) {"));
                ReadMethod.Statements.Add(new CodeSnippetExpression(string.Format("this.{0}.Clear()", collection.Name)));
                var dstatement = new CodeSnippetExpression(string.Format("this.{0}.AddRange(stream.DeserializeObjectArray<{1}>(\"{0}\"))", collection.Name, elementNode.NameAsViewModel));
                ReadMethod.Statements.Add(dstatement);
                ReadMethod.Statements.Add(new CodeSnippetStatement("}"));
            }
            else
            {
                //if (collection.Type == null) continue;
                //if (!AcceptableTypes.ContainsKey(viewModelPropertyData.Type)) continue;
                //viewModelPropertyData.IsEnum(data.OwnerData);
                //var statement = new CodeSnippetStatement(string.Format("\t\tstream.Serialize{0}(\"{1}\", this.{1});", AcceptableTypes[viewModelPropertyData.Type], viewModelPropertyData.Name));
                //writeMethod.Statements.Add(statement);

                //var dstatement = new CodeSnippetStatement(string.Format("\t\tthis.{0} = stream.Deserialize{1}(\"{0}\");", viewModelPropertyData.Name, AcceptableTypes[viewModelPropertyData.Type]));
                //readMethod.Statements.Add(dstatement);
            }
        }
        decl.Members.Add(WriteMethod);
        decl.Members.Add(ReadMethod);
    }

    public CodeTypeReference GetCommandTypeReference(ViewModelCommandData itemData, CodeTypeReference senderType, ElementData element)
    {
        //if (!itemData.IsYield)
        //{
        if (string.IsNullOrEmpty(itemData.RelatedTypeName))
        {
            var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.CommandWithSenderT);
            commandWithType.TypeArguments.Add(senderType);
            return commandWithType;
        }
        else
        {
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
        }
        //}
        ////else
        ////{
        //    if (string.IsNullOrEmpty(itemData.RelatedTypeName))
        //    {
        //        //if (element.IsMultiInstance)
        //        //{
        //            var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommandWithSenderT);
        //            commandWithType.TypeArguments.Add(senderType);
        //            return commandWithType;
        //        //}
        //        //else
        //        //{
        //        //    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommand);

        //        //    return commandWithType;
        //        //}
        //    }
        //    else
        //    {
        //        //if (element.IsMultiInstance)
        //        //{
        //            var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommandWithSenderAndArgument);
        //            commandWithType.TypeArguments.Add(senderType);
        //            var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
        //            if (typeViewModel == null)
        //            {
        //                commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
        //            }
        //            else
        //            {
        //                commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
        //            }
        //            return commandWithType;
        //        //}
        //        //else
        //        //{
        //        //    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommandWith);
        //        //    var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
        //        //    if (typeViewModel == null)
        //        //    {
        //        //        commandWithType.TypeArguments.Add(new CodeTypeReference(itemData.RelatedTypeName));
        //        //    }
        //        //    else
        //        //    {
        //        //        commandWithType.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
        //        //    }
        //        //    return commandWithType;
        //        //}
        //    }

    }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        Declaration = new CodeTypeDeclaration(ElementData.NameAsViewModel) { IsPartial = true };
        AddViewModel(ElementData);
        if (IsDesignerFile)
        {
        
            Declaration.BaseTypes.Add(ElementData.NameAsViewModelBase);
            AddWireCommandsMethod(ElementData, Declaration, new CodeTypeReference(ElementData.NameAsViewModel));
            Namespace.Types.Add(Declaration);
            
            AddConstructors(Declaration);
            
            UnBindMethod = new CodeMemberMethod()
            {
                Name = "Unbind",
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            AddWriteMethod(Declaration, ElementData);
            AddUnbindMethod(Declaration);
            GenerateMetaDataMethods(Declaration, ElementData, BindMethod, UnBindMethod);
            AddParentProperties(Declaration, ElementData);
            BindTransitions();
            AddComputedPropertyMethods(ElementData, BaseTypeDeclaration);
        }

        
        this.TryAddNamespace("UnityEngine");
    }

    public virtual CodeMemberField ToCodeMemberField(IBindableTypedItem itemData, CodeMemberMethod constructor)
    {
        var field = new CodeMemberField { Name = itemData.FieldName };

        field.Attributes = MemberAttributes.Public;
        var t = itemData.GetFieldType();
        field.Type = t;
        //if (itemData is ComputedPropertyData)
        //{
        //    field.Type = new CodeTypeReference(string.Format("readonly Computed<{0}>", fieldType.Name));
        //}
        //else
        //{
        //    field.Type = new CodeTypeReference(string.Format("readonly P<{0}>", relatedType));
        //}
        //var t = itemData.GetFieldType(relatedType);
        var initExpr = new CodeObjectCreateExpression(t);
        initExpr.Parameters.Add(new CodeThisReferenceExpression());
        initExpr.Parameters.Add(new CodePrimitiveExpression(itemData.Name));

        constructor.Statements.Add(new CodeAssignStatement(new CodeSnippetExpression(field.Name), initExpr));

        return field;
    }

    public virtual CodeMemberProperty ToCodeMemberProperty(IBindableTypedItem itemData)
    {
        var property = new CodeMemberProperty { Name = itemData.Name, Attributes = MemberAttributes.Public };

        var typeViewModel = itemData.RelatedNode();

        var t = itemData.GetPropertyType();
        property.Type = t;
        //if (typeViewModel == null)
        //{
        //    property.Type = new CodeTypeReference(itemData.RelatedTypeName);
        //}
        //else
        //{
        //    property.Type = new CodeTypeReference(typeViewModel.NameAsViewModel);
        //}
        property.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("{0}.Value", itemData.FieldName))));

        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0}.Value = value", itemData.FieldName)));

        if (typeViewModel is ElementData && itemData is ViewModelPropertyData)
        {
            property.SetStatements.Add(new CodeSnippetExpression(string.Format("if (value != null) value.Parent{0} = this", itemData.Node.Name)));
        }

        return property;
    }

    public virtual CodeMemberField ToCollectionCodeMemberField(ViewModelCollectionData itemData, CodeMemberMethod bindMethod, CodeMemberMethod unBindMethod)
    {
        var field = new CodeMemberField { Name = itemData.FieldName };

        field.Attributes = MemberAttributes.Public;
        var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);

        var relatedType = typeViewModel == null ? itemData.RelatedTypeName : typeViewModel.NameAsViewModel;

        field.Type = new CodeTypeReference(string.Format("ModelCollection<{0}>", relatedType));

        var t = new CodeTypeReference(uFrameEditor.UFrameTypes.ModelCollection);
        t.TypeArguments.Add(new CodeTypeReference(relatedType));
        var initExpr = new CodeObjectCreateExpression(t);
        initExpr.Parameters.Add(new CodeThisReferenceExpression());
        initExpr.Parameters.Add(new CodePrimitiveExpression(itemData.Name));
        bindMethod.Statements.Add(new CodeAssignStatement(new CodeSnippetExpression(field.Name), initExpr));
        return field;
    }

    public virtual CodeMemberProperty ToCollectionCodeMemberProperty(ViewModelCollectionData itemData)
    {
        var property = new CodeMemberProperty();
        property.Name = itemData.Name;
        property.Type = new CodeTypeReference(uFrameEditor.UFrameTypes.ModelCollection);
        property.Attributes = MemberAttributes.Public;
        var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
        if (typeViewModel == null)
        {
            property.Type.TypeArguments.Add(itemData.RelatedTypeName);
        }
        else
        {
            property.Type.TypeArguments.Add(new CodeTypeReference(typeViewModel.NameAsViewModel));
        }

        property.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("{0}", itemData.FieldName))));

        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0}.Clear()", itemData.FieldName)));
        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0}.AddRange(value)", itemData.FieldName)));

        return property;
    }

    public virtual CodeMemberField ToCommandCodeMemberField(ViewModelCommandData itemData, ElementData data)
    {
        var property = new CodeMemberField
        {
            Name = itemData.FieldName,
            Attributes = MemberAttributes.Family,
            Type = GetCommandTypeReference(itemData, new CodeTypeReference(data.NameAsViewModel), data)
        };
        return property;
    }

    public virtual CodeMemberProperty ToCommandCodeMemberProperty(ViewModelCommandData itemData, ElementData data)
    {
        var property = new CodeMemberProperty
        {
            Name = itemData.Name,
            Attributes = MemberAttributes.Public,
            Type = GetCommandTypeReference(itemData, new CodeTypeReference(data.NameAsViewModel), data)
        };

        property.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("{0}", itemData.FieldName))));

        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0} = value", itemData.FieldName)));
        return property;
    }

    private void GenerateMetaDataMethods(CodeTypeDeclaration decleration, ElementData data, CodeMemberMethod constructor, CodeMemberMethod unBindMethod)
    {
        FillPropertiesMethod = new CodeMemberMethod()
        {
            Name = "FillProperties",
            Attributes = MemberAttributes.Family | MemberAttributes.Override
        };
        FillPropertiesMethod.Parameters.Add(
            new CodeParameterDeclarationExpression(new CodeTypeReference("List<ViewModelPropertyInfo>"), "list"));
        FillPropertiesMethod.Statements.Add(new CodeSnippetExpression("base.FillProperties(list);"));
        decleration.Members.Add(FillPropertiesMethod);

        FillCommandsMethod = new CodeMemberMethod()
        {
            Name = "FillCommands",
            Attributes = MemberAttributes.Family | MemberAttributes.Override
        };
        FillCommandsMethod.Parameters.Add(
            new CodeParameterDeclarationExpression(new CodeTypeReference("List<ViewModelCommandInfo>"), "list"));
        FillCommandsMethod.Statements.Add(new CodeSnippetExpression("base.FillCommands(list);"));

        decleration.Members.Add(FillCommandsMethod);
        // Now Generator code here
        foreach (var viewModelPropertyData in data.Properties)
        {
            var field = ToCodeMemberField(viewModelPropertyData, constructor);
            var property = field.EncapsulateField(viewModelPropertyData.Name + "Property");
            property.HasSet = false;
            decleration.Members.Add(property);
            BaseTypeDeclaration.Members.Add(field);
            decleration.Members.Add(ToCodeMemberProperty(viewModelPropertyData));

            if (viewModelPropertyData.RelatedNode() is ElementData)
            {
                FillPropertiesMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format(
                        "list.Add(new ViewModelPropertyInfo({0}, true, false, false))", viewModelPropertyData.FieldName)));
            }
            else if (viewModelPropertyData.RelatedNode() is EnumData)
            {
                FillPropertiesMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format(
                        "list.Add(new ViewModelPropertyInfo({0}, false, false, true))", viewModelPropertyData.FieldName)));
            }
            else
            {
                FillPropertiesMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format(
                        "list.Add(new ViewModelPropertyInfo({0}, false, false, false))", viewModelPropertyData.FieldName)));
            }
        }
        foreach (var viewModelPropertyData in data.ComputedProperties)
        {
            var field = ToCodeMemberField(viewModelPropertyData, constructor);
            var property = field.EncapsulateField(viewModelPropertyData.Name + "Property", null, null, false);
            property.HasSet = false;
            BaseTypeDeclaration.Members.Add(field);
            decleration.Members.Add(property);
            decleration.Members.Add(ToCodeMemberProperty(viewModelPropertyData));

            if (viewModelPropertyData.RelatedNode() is ElementData)
            {
                FillPropertiesMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format(
                        "list.Add(new ViewModelPropertyInfo({0}, true, false, false, true))",
                        viewModelPropertyData.FieldName)));
            }
            else if (viewModelPropertyData.RelatedNode() is EnumData)
            {
                FillPropertiesMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format(
                        "list.Add(new ViewModelPropertyInfo({0}, false, false, true, true))",
                        viewModelPropertyData.FieldName)));
            }
            else
            {
                FillPropertiesMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format(
                        "list.Add(new ViewModelPropertyInfo({0}, false, false, false, true))",
                        viewModelPropertyData.FieldName)));
            }
        }

        foreach (var collectionData in data.Collections)
        {
            var field = ToCollectionCodeMemberField(collectionData, constructor, unBindMethod);
            var property = field.EncapsulateField(collectionData.Name);
            property.HasSet = false;
            property.Attributes = MemberAttributes.Public;

            decleration.Members.Add(property);
            BaseTypeDeclaration.Members.Add(field);


            var relatedElement = collectionData.RelatedNode() as ElementData;

            if (relatedElement != null)
            {
                constructor.Statements.Add(
                    new CodeSnippetExpression(string.Format("{0}.CollectionChanged += {1}CollectionChanged",
                        collectionData.FieldName, collectionData.Name)));

                unBindMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format("{0}.CollectionChanged -= {1}CollectionChanged",
                        collectionData.FieldName, collectionData.Name)));

                var collectionChangedMethod = new CodeMemberMethod()
                {
                    Name = string.Format("{0}CollectionChanged", collectionData.Name),
                    Attributes = MemberAttributes.Family | MemberAttributes.Override,
                };
                var baseCollectionChangedMethod = new CodeMemberMethod()
                {
                    Name = string.Format("{0}CollectionChanged", collectionData.Name),
                    Attributes = MemberAttributes.Family,
                };

                collectionChangedMethod.Parameters.Add(
                    new CodeParameterDeclarationExpression(
                        "System.Collections.Specialized.NotifyCollectionChangedEventArgs", "args"));
                baseCollectionChangedMethod.Parameters.Add(
                    new CodeParameterDeclarationExpression(
                        "System.Collections.Specialized.NotifyCollectionChangedEventArgs", "args"));

                collectionChangedMethod.Statements.Add(
                    new CodeExpressionStatement(
                        new CodeSnippetExpression(
                            string.Format("foreach (var item in args.OldItems.OfType<{0}>()) item.Parent{1} = null;", relatedElement.NameAsViewModel, data.Name))));
                collectionChangedMethod.Statements.Add(
                    new CodeExpressionStatement(
                        new CodeSnippetExpression(
                            string.Format("foreach (var item in args.NewItems.OfType<{0}>()) item.Parent{1} = this;", relatedElement.NameAsViewModel, data.Name))));

                BaseTypeDeclaration.Members.Add(baseCollectionChangedMethod);
                decleration.Members.Add(collectionChangedMethod);
            }

            if (relatedElement != null)
            {
                FillPropertiesMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format(
                        "list.Add(new ViewModelPropertyInfo({0}, true, true, false))", collectionData.FieldName)));
            }
            else if (collectionData.RelatedNode() is EnumData)
            {
                FillPropertiesMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format(
                        "list.Add(new ViewModelPropertyInfo({0}, false, true, true))", collectionData.FieldName)));
            }
            else
            {
                FillPropertiesMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format(
                        "list.Add(new ViewModelPropertyInfo({0}, false, true, false))", collectionData.FieldName)));
            }
        }
        foreach (var commandData in data.Commands)
        {
            BaseTypeDeclaration.Members.Add(ToCommandCodeMemberField(commandData, data));
            decleration.Members.Add(ToCommandCodeMemberProperty(commandData, data));

            var relatedNode = commandData.RelatedNode();
            var element = relatedNode as ElementData;
            var type = "void";
            if (element != null)
            {
                type = element.NameAsViewModel;
            }
            else if (relatedNode != null)
            {
                type = relatedNode.Name;
            } else if (!string.IsNullOrEmpty(commandData.RelatedTypeName))
            {
                type = commandData.RelatedTypeName;
            }

            FillCommandsMethod.Statements.Add(
                new CodeSnippetExpression(string.Format(
                    "list.Add(new ViewModelCommandInfo(\"{0}\", {1}) {{ ParameterType = typeof({2}) }})", commandData.Name,
                    commandData.Name, type)));
        }
        foreach (var computedProperty in ElementData.ComputedProperties)
        {
            // Call the reset method on the constructor
            BindMethod.Statements.Add(
                new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(),
                    string.Format("Reset{0}", computedProperty.Name))));
            //this.BindProperty(this._CurrentPlayerProperty,p=>ResetWaveIsComplete());
            
        }
    }

    protected void AddComputedPropertyMethods(ElementData data, CodeTypeDeclaration tDecleration)
    {
        foreach (var computedProperty in data.ComputedProperties)
        {
            var dependentsMethods = new CodeMemberMethod()
            {
                Attributes = MemberAttributes.Public,
                Name = string.Format("Get{0}Dependents",computedProperty.Name),
                ReturnType = new CodeTypeReference("IEnumerable<IObservableProperty>")
            };

            foreach (var dependent in computedProperty.DependantProperties)
            {
               

                var relatedElement = dependent.RelatedNode() as ElementData;
                if (relatedElement != null)
                {

                    BindMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format("this.BindProperty({0}, p=> Reset{1}())", dependent.FieldName,
                            computedProperty.Name)));

                    var condition =
                        new CodeConditionStatement(
                            new CodeSnippetExpression(string.Format("{0}.Value != null", dependent.FieldName)));


                    foreach (var item in relatedElement.AllProperties)
                    {
                        if (!computedProperty[item.Identifier]) continue;
                        condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("yield return {0}.Value.{1}", dependent.FieldName,item.FieldName)));

                    }
                    dependentsMethods.Statements.Add(condition);
                }
                else
                {
                    dependentsMethods.Statements.Add(new CodeSnippetExpression(string.Format("yield return {0}", dependent.FieldName)));
                }
                
            }
            dependentsMethods.Statements.Add(new CodeSnippetExpression("yield break"));

            var computeMethod = new CodeMemberMethod()
            {
                Attributes = MemberAttributes.Public,
                Name = computedProperty.NameAsComputeMethod,
                ReturnType = new CodeTypeReference(computedProperty.RelatedTypeNameOrViewModel)
            };

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
            tDecleration.Members.Add(dependentsMethods);
        }
    }
}