using System.Globalization;
using System.Linq;
using Invert.MVVM;
using Invert.uFrame.Editor;
using System;
using System.CodeDom;
using System.Collections.Generic;
using UnityEngine;

public class RxTypePostProcessor : TypeGeneratorPostProcessor<ViewModelGenerator>
{
    
    public override void Apply()
    {
        var elementData = CodeGenerator.Data;
        foreach (var item in elementData.Properties)
        {
            if (item["IsRxProperty"])
            {
                foreach (var member in Declaration.Members.OfType<CodeMemberField>())
                {
                    if (member.Name == item.FieldName)
                    {
                        var typeArgument = member.Type.TypeArguments[0];

                        member.Type = new CodeTypeReference(typeof(P<>));

                        member.Type.TypeArguments.Add(typeArgument);
                    }
                }
            }
        }
        
    }
}
public class ViewModelGenerator : CodeGenerator
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

    public ElementData Data
    {
        get;
        set;
    }

    public CodeTypeDeclaration Decleration { get; set; }

    public INodeRepository DiagramData
    {
        get;
        set;
    }

    public ViewModelGenerator(bool isDesignerFile, ElementData data)
    {
        IsDesignerFile = isDesignerFile;
        Data = data;
    }

    public virtual void AddViewModel(ElementData data)
    {
        Decleration = new CodeTypeDeclaration(data.NameAsViewModel);
        
        if (IsDesignerFile)
        {
            Decleration.BaseTypes.Add(data.BaseTypeName);
            Decleration.CustomAttributes.Add(
                new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.UFrameTypes.DiagramInfoAttribute),
                    new CodeAttributeArgument(new CodePrimitiveExpression(DiagramData.Name))));
            CreateViewProperties(data);
            AddWireCommandsMethod(data, Decleration, new CodeTypeReference(data.NameAsViewModel));
        }

        Decleration.IsPartial = true;
        if (IsDesignerFile)
        {
            var constructor = new CodeConstructor()
                {
                    Name = Decleration.Name,
                    Attributes = MemberAttributes.Public,
                };
            constructor.BaseConstructorArgs.Add(new CodeSnippetExpression(""));
            Decleration.Members.Add(constructor);
            var constructorWithController = new CodeConstructor()
            {
                Name = Decleration.Name,
                Attributes = MemberAttributes.Public
            };
            constructorWithController.ChainedConstructorArgs.Add(new CodeSnippetExpression(""));

            constructorWithController.Parameters.Add(new CodeParameterDeclarationExpression(Data.NameAsControllerBase, "controller"));
            constructorWithController.Statements.Add(
                new CodeAssignStatement(
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Controller"),
                    new CodeSnippetExpression("controller")));

            Decleration.Members.Add(constructorWithController);

            var unBindMethod = new CodeMemberMethod()
            {
                Name = "Unbind",
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            unBindMethod.Statements.Add(new CodeSnippetExpression("base.Unbind()"));
            Decleration.Members.Add(unBindMethod);
            var fillPropertiesMethod = new CodeMemberMethod()
            {
                Name = "FillProperties",
                Attributes = MemberAttributes.Family | MemberAttributes.Override

            };
            fillPropertiesMethod.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference("List<ViewModelPropertyInfo>"), "list"));
            fillPropertiesMethod.Statements.Add(new CodeSnippetExpression("base.FillProperties(list);"));
            Decleration.Members.Add(fillPropertiesMethod);

            var fillCommandsMethod = new CodeMemberMethod()
            {
                Name = "FillCommands",
                Attributes = MemberAttributes.Family | MemberAttributes.Override

            };
            fillCommandsMethod.Parameters.Add(
                new CodeParameterDeclarationExpression(new CodeTypeReference("List<ViewModelCommandInfo>"), "list"));
            fillCommandsMethod.Statements.Add(new CodeSnippetExpression("base.FillCommands(list);"));

            Decleration.Members.Add(fillCommandsMethod);
            // Now Generator code here
            foreach (var viewModelPropertyData in data.Properties.Where(p => !p.IsComputed))
            {
                Decleration.Members.Add(ToCodeMemberField(viewModelPropertyData, constructor));
                Decleration.Members.Add(ToCodeMemberProperty(viewModelPropertyData));

                if (viewModelPropertyData.RelatedNode() is ElementData)
                {
                    fillPropertiesMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelPropertyInfo({0}, true, false, false))", viewModelPropertyData.FieldName)));
                }
                else if (viewModelPropertyData.RelatedNode() is EnumData)
                {
                    fillPropertiesMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelPropertyInfo({0}, false, false, true))", viewModelPropertyData.FieldName)));
                }
                else
                {
                    fillPropertiesMethod.Statements.Add( 
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelPropertyInfo({0}, false, false, false))", viewModelPropertyData.FieldName)));
                }
            }
            foreach (var viewModelPropertyData in data.Properties.Where(p=>p.IsComputed))
            {
                Decleration.Members.Add(ToCodeMemberField(viewModelPropertyData, constructor));
                Decleration.Members.Add(ToCodeMemberProperty(viewModelPropertyData));

                if (viewModelPropertyData.RelatedNode() is ElementData)
                {
                    fillPropertiesMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelPropertyInfo({0}, true, false, false, true))", viewModelPropertyData.FieldName)));
                }
                else if (viewModelPropertyData.RelatedNode() is EnumData)
                {
                    fillPropertiesMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelPropertyInfo({0}, false, false, true, true))", viewModelPropertyData.FieldName)));
                }
                else
                {
                    fillPropertiesMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelPropertyInfo({0}, false, false, false, true))", viewModelPropertyData.FieldName)));
                }
            }

            
            foreach (var viewModelPropertyData in data.Collections)
            {
                Decleration.Members.Add(ToCollectionCodeMemberField(viewModelPropertyData, constructor,unBindMethod));
                Decleration.Members.Add(ToCollectionCodeMemberProperty(viewModelPropertyData));
                var relatedElement = viewModelPropertyData.RelatedNode() as ElementData;

                if (relatedElement != null)
                {


                    constructor.Statements.Add(
                        new CodeSnippetExpression(string.Format("{0}.CollectionChangedWith += {1}CollectionChanged",
                            viewModelPropertyData.FieldName, viewModelPropertyData.Name)));
                    unBindMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format("{0}.CollectionChangedWith -= {1}CollectionChanged",
                            viewModelPropertyData.FieldName, viewModelPropertyData.Name)));
        

                         var collectionChangedMethod = new CodeMemberMethod()
                        {
                            Name = string.Format("{0}CollectionChanged",viewModelPropertyData.Name),
                            Attributes = MemberAttributes.Private,
                    
                        };

                    collectionChangedMethod.Parameters.Add(
                        new CodeParameterDeclarationExpression(
                            string.Format("ModelCollectionChangeEventWith<{0}>", relatedElement.NameAsViewModel), "args"));

                    collectionChangedMethod.Statements.Add(
                       new CodeExpressionStatement(new CodeSnippetExpression(string.Format("foreach (var item in args.OldItemsOfT) item.Parent{0} = null;", data.Name))));
                    collectionChangedMethod.Statements.Add(
                        new CodeExpressionStatement(new CodeSnippetExpression(string.Format("foreach (var item in args.NewItemsOfT) item.Parent{0} = this;", data.Name))));

                   
                    Decleration.Members.Add(collectionChangedMethod);
                }

                if (relatedElement != null)
                {
                    fillPropertiesMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelPropertyInfo({0}, true, true, false))", viewModelPropertyData.FieldName)));
                }
                else if (viewModelPropertyData.RelatedNode() is EnumData)
                {
                    fillPropertiesMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelPropertyInfo({0}, false, true, true))", viewModelPropertyData.FieldName)));
                }
                else
                {
                    fillPropertiesMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelPropertyInfo({0}, false, true, false))", viewModelPropertyData.FieldName)));
                }
           
            }
            foreach (var viewModelPropertyData in data.Commands)
            {
                Decleration.Members.Add(ToCommandCodeMemberField(viewModelPropertyData));
                Decleration.Members.Add(ToCommandCodeMemberProperty(viewModelPropertyData));

                fillCommandsMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format(
                            "list.Add(new ViewModelCommandInfo(\"{0}\", {1}))", viewModelPropertyData.Name, viewModelPropertyData.Name)));
            }

            foreach (var parentElement in data.ParentElements)
            {
                var parentField = new CodeMemberField(parentElement.NameAsViewModel, "_Parent" + parentElement.Name);
                var property = parentField.EncapsulateField("Parent" + parentElement.Name);
                Decleration.Members.Add(parentField);
                Decleration.Members.Add(property);

            }
            AddWriteMethod(data);
        }
        ProcessModifiers(Decleration);
        Namespace.Types.Add(Decleration);
    }

    public virtual void AddSetParentMethod(ElementData data)
    {
        var setParentMethod = new CodeMemberMethod()
        {
            Name = "SetParent",
            Attributes = MemberAttributes.Override | MemberAttributes.Public

        };
        setParentMethod.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.ViewModel, "viewModel"));

        var baseInvoker = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(),
            setParentMethod.Name);
        baseInvoker.Parameters.Add(new CodeVariableReferenceExpression("viewModel"));
        setParentMethod.Statements.Add(baseInvoker);
    }
    public virtual void AddWriteMethod(ElementData data)
    {
        var readMethod = new CodeMemberMethod()
        {
            Name = "Read",
            Attributes = MemberAttributes.Override | MemberAttributes.Public
        };
        readMethod.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.ISerializerStream, "stream"));

        var writeMethod = new CodeMemberMethod()
        {
            Name = "Write",
            Attributes = MemberAttributes.Override | MemberAttributes.Public
        };
        writeMethod.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.ISerializerStream, "stream"));
        writeMethod.Statements.Add(new CodeSnippetStatement("\t\tbase.Write(stream);"));
        readMethod.Statements.Add(new CodeSnippetStatement("\t\tbase.Read(stream);"));

        foreach (var viewModelPropertyData in data.SerializedProperties)
        {
            var relatedNode = viewModelPropertyData.TypeNode();
            if (relatedNode is EnumData)
            {
                var statement = new CodeSnippetStatement(string.Format("\t\tstream.SerializeInt(\"{0}\", (int)this.{0});", viewModelPropertyData.Name));
                writeMethod.Statements.Add(statement);

                var dstatement = new CodeSnippetStatement(string.Format("\t\tthis.{0} = ({1})stream.DeserializeInt(\"{0}\");", viewModelPropertyData.Name, viewModelPropertyData.RelatedTypeName));
                readMethod.Statements.Add(dstatement);
            }
            else if (relatedNode is ElementData)
            {
                var elementNode = relatedNode as ElementData;
                var statement = new CodeSnippetStatement(string.Format("\t\tstream.SerializeObject(\"{0}\", this.{0});", viewModelPropertyData.Name));
                writeMethod.Statements.Add(statement);

                var dstatement = new CodeSnippetStatement(string.Format("\t\tthis.{0} = stream.DeserializeObject<{1}>(\"{0}\");", viewModelPropertyData.Name, elementNode.NameAsViewModel));
                readMethod.Statements.Add(dstatement);
            }
            else
            {
                if (viewModelPropertyData.Type == null) continue;
                if (!AcceptableTypes.ContainsKey(viewModelPropertyData.Type)) continue;
                //viewModelPropertyData.IsEnum(data.OwnerData);
                var statement = new CodeSnippetStatement(string.Format("\t\tstream.Serialize{0}(\"{1}\", this.{1});", AcceptableTypes[viewModelPropertyData.Type], viewModelPropertyData.Name));
                writeMethod.Statements.Add(statement);

                var dstatement = new CodeSnippetStatement(string.Format("\t\tthis.{0} = stream.Deserialize{1}(\"{0}\");", viewModelPropertyData.Name, AcceptableTypes[viewModelPropertyData.Type]));
                readMethod.Statements.Add(dstatement);
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
                var statement = new CodeSnippetStatement(string.Format("\t\tstream.SerializeArray(\"{0}\", this.{0});", collection.Name));
                writeMethod.Statements.Add(statement);

                var dstatement = new CodeSnippetStatement(string.Format("\t\tthis.{0} = stream.DeserializeObjectArray<{1}>(\"{0}\").ToList();", collection.Name, elementNode.NameAsViewModel));
                readMethod.Statements.Add(dstatement);
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
        Decleration.Members.Add(writeMethod);
        Decleration.Members.Add(readMethod);
    }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        AddViewModel(Data);
    }

    public virtual CodeMemberField ToCodeMemberField(ViewModelPropertyData itemData, CodeConstructor constructor)
    {
        var field = new CodeMemberField { Name = itemData.FieldName };

        field.Attributes = MemberAttributes.Public;

        var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);
        var relatedType = typeViewModel == null ? itemData.RelatedTypeName : typeViewModel.NameAsViewModel;
        if (itemData.IsComputed)
        {
            field.Type = new CodeTypeReference(string.Format("readonly Computed<{0}>", relatedType));
        }
        else
        {
            field.Type = new CodeTypeReference(string.Format("readonly P<{0}>", relatedType));
        }

        var t = itemData.IsComputed ? new CodeTypeReference(uFrameEditor.UFrameTypes.Computed) : new CodeTypeReference(uFrameEditor.UFrameTypes.P);
        t.TypeArguments.Add(new CodeTypeReference(relatedType));
        var initExpr = new CodeObjectCreateExpression(t);
        //field.InitExpression = initExpr;
      
        initExpr.Parameters.Add(new CodeThisReferenceExpression());
        initExpr.Parameters.Add(new CodePrimitiveExpression(itemData.Name));
        if (itemData.IsComputed)
        {
            foreach (var dependantProperty in itemData.DependantProperties)
            {
                initExpr.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), dependantProperty.FieldName));
            }
        }
        constructor.Statements.Add(new CodeAssignStatement(new CodeSnippetExpression(field.Name), initExpr));

        return field;
    }

    public virtual CodeMemberProperty ToCodeMemberProperty(ViewModelPropertyData itemData)
    {
        var property = new CodeMemberProperty { Name = itemData.Name, Attributes = MemberAttributes.Public };

        var typeViewModel = itemData.RelatedNode() as ElementData;

        if (typeViewModel == null)
        {
            property.Type = new CodeTypeReference(itemData.RelatedTypeName);
        }
        else
        {
            property.Type = new CodeTypeReference(typeViewModel.NameAsViewModel);

        }
        property.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("{0}.Value", itemData.FieldName))));

        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0}.Value = value", itemData.FieldName)));

        if (typeViewModel != null)
        {
            property.SetStatements.Add(new CodeSnippetExpression(string.Format("if (value != null) value.Parent{0} = this", itemData.Node.Name)));
        }

        return property;
    }

    public virtual CodeMemberField ToCollectionCodeMemberField(ViewModelCollectionData itemData, CodeConstructor constructor, CodeMemberMethod unBindMethod)
    {
        var field = new CodeMemberField { Name = itemData.FieldName };

        field.Attributes = MemberAttributes.Public;
        var typeViewModel = DiagramData.GetViewModel(itemData.RelatedTypeName);

        var relatedType = typeViewModel == null ? itemData.RelatedTypeName : typeViewModel.NameAsViewModel;

        field.Type = new CodeTypeReference(string.Format("readonly ModelCollection<{0}>", relatedType));

        var t = new CodeTypeReference(uFrameEditor.UFrameTypes.ModelCollection);
        t.TypeArguments.Add(new CodeTypeReference(relatedType));
        var initExpr = new CodeObjectCreateExpression(t);
        initExpr.Parameters.Add(new CodeThisReferenceExpression());
        initExpr.Parameters.Add(new CodePrimitiveExpression(itemData.Name));
        constructor.Statements.Add(new CodeAssignStatement(new CodeSnippetExpression(field.Name), initExpr));
        return field;
    }

    public virtual CodeMemberProperty ToCollectionCodeMemberProperty(ViewModelCollectionData itemData)
    {
        var property = new CodeMemberProperty();
        property.Name = itemData.Name;
        property.Type = new CodeTypeReference(typeof(ICollection<>));
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

    public virtual CodeMemberField ToCommandCodeMemberField(ViewModelCommandData itemData)
    {
        var property = new CodeMemberField();
        property.Name = itemData.FieldName;
        property.Type = new CodeTypeReference(uFrameEditor.UFrameTypes.ICommand);
        return property;
    }

    public virtual CodeMemberProperty ToCommandCodeMemberProperty(ViewModelCommandData itemData)
    {
        var property = new CodeMemberProperty
        {
            Name = itemData.Name,
            Attributes = MemberAttributes.Public,
            Type = new CodeTypeReference(uFrameEditor.UFrameTypes.ICommand)
        };

        property.GetStatements.Add(
            new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("{0}", itemData.FieldName))));

        property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0} = value", itemData.FieldName)));
        return property;
    }

    private void AddWireCommandsMethod(ElementData data, CodeTypeDeclaration tDecleration,
       CodeTypeReference viewModelTypeReference)
    {
        var wireMethod = new CodeMemberMethod { Name = string.Format("WireCommands") };
        tDecleration.Members.Add(wireMethod);
        wireMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(uFrameEditor.UFrameTypes.Controller),
            "controller"));
        if (data.IsDerived)
        {
            wireMethod.Attributes = MemberAttributes.Family | MemberAttributes.Override;
            var callBase = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), wireMethod.Name,
                new CodeVariableReferenceExpression("controller"));
            wireMethod.Statements.Add(callBase);
        }
        else
        {
            wireMethod.Attributes = MemberAttributes.Family | MemberAttributes.Override;
        }
        if (data.Commands.Count > 0 || data.Properties.Any(p => p.IsComputed))
        {
            wireMethod.Statements.Add(
                new CodeSnippetExpression(string.Format("var {0} = controller as {1}", data.NameAsVariable, data.NameAsControllerBase)));
        }

        foreach (var command in data.Commands)
        {
            var assigner = new CodeAssignStatement
            {
                Left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), command.Name)
            };
            var commandWithType = GetCommandTypeReference(command, viewModelTypeReference, data);
            var commandWith = new CodeObjectCreateExpression(commandWithType);
            if (data.IsMultiInstance)
            {
                commandWith.Parameters.Add(new CodeThisReferenceExpression());
            }
            commandWith.Parameters.Add(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(data.NameAsVariable), command.Name));
            assigner.Right = commandWith;
            wireMethod.Statements.Add(assigner);
        }
        foreach (var computedProperty in data.Properties.Where(p=>p.IsComputed))
        {
            wireMethod.Statements.Add(
                new CodeSnippetExpression(string.Format("{0}.Calculator = (vm)=> {{ return {1}.{2}(vm as {3}); }}", computedProperty.FieldName,
                    data.NameAsVariable, computedProperty.NameAsComputeMethod,data.NameAsViewModel)));

            //if (itemData.IsComputed)
            //{
            //    initExpr.Parameters.Add(new CodeSnippetExpression(string.Format("(vm)=>{{ return {0}(vm as {1}); }}", itemData.NameAsComputeMethod, Data.Name)));
            //}
        }
    }

    public CodeTypeReference GetCommandTypeReference(ViewModelCommandData itemData, CodeTypeReference senderType, ElementData element)
    {
        if (!itemData.IsYield)
        {
            if (string.IsNullOrEmpty(itemData.RelatedTypeName))
            {
                if (element.IsMultiInstance)
                {
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.CommandWithSenderT);
                    commandWithType.TypeArguments.Add(senderType);
                    return commandWithType;
                }
                else
                {
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.Command);
                    return commandWithType;
                }

            }
            else
            {
                if (element.IsMultiInstance)
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
                else
                {
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.CommandWith);

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

            }
        }
        else
        {
            if (string.IsNullOrEmpty(itemData.RelatedTypeName))
            {
                if (element.IsMultiInstance)
                {
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommandWithSenderT);
                    commandWithType.TypeArguments.Add(senderType);
                    return commandWithType;
                }
                else
                {
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommand);

                    return commandWithType;
                }

            }
            else
            {
                if (element.IsMultiInstance)
                {
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
                }
                else
                {
                    var commandWithType = new CodeTypeReference(uFrameEditor.UFrameTypes.YieldCommandWith);
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
            }
        }


    }

    public void CreateViewProperties(ElementData data)
    {
        var viewProperties = data.ViewProperties;
        foreach (var viewProperty in viewProperties)
        {
            var name = viewProperty.NameAsProperty;

            if (data.Properties.FirstOrDefault(p => p.Name == name) != null)
            {
                Debug.LogError(string.Format("The name '{0}' already exists on the element '{1}'.", name, data.Name));
                continue;
            }

            var viewFieldDecleration = new CodeMemberField()
            {
                Type = new CodeTypeReference(viewProperty.MemberType),
                Name = viewProperty.NameAsField,
                Attributes = MemberAttributes.Private
            };

            var viewPropertyDecleration = viewFieldDecleration.EncapsulateField(name, null, null, true);//,
            //new CodeSnippetExpression(string.Format("this.GetComponent<{0}>()", viewProperty.MemberType)));

            viewPropertyDecleration.Attributes = MemberAttributes.Public;
            //viewPropertyDecleration.SetStatements.Add(new CodeSnippetExpression("Dirty = true"));
            Decleration.Members.Add(viewFieldDecleration);
            Decleration.Members.Add(viewPropertyDecleration);
        }
    }
}

