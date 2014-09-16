using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.uFrame.Editor;
using UnityEngine;

public abstract class SceneManagerClassGenerator : CodeGenerator
{
    public SceneManagerData Data
    {
        get; set;
    }
    public INodeRepository DiagramData
    {
        get; set;
    }
    
    public CodeTypeDeclaration AddTypeEnum(string name, IEnumerable<RegisteredInstanceData> instances)
    {
        var enumDecleration = new CodeTypeDeclaration(name) { IsEnum = true };
        //enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, name));
        foreach (var item in instances)
        {
            enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, item.Name));
        }
        Namespace.Types.Add(enumDecleration);
        
        return enumDecleration;
    }

    public virtual void AddSceneManager(SceneManagerData sceneManager)
    {
        // Grab the root items
        var subSystem = sceneManager.SubSystem;
        if (subSystem == null)
        {
            Debug.LogWarning(string.Format("Scene Manager {0} doesn't have an associated SubSystem.  To create the type please associate one.", sceneManager.Name));
            return;
        }

        var elements = subSystem.GetIncludedElements().ToArray();

        var decl = new CodeTypeDeclaration(IsDesignerFile ? sceneManager.NameAsSceneManagerBase : sceneManager.NameAsSceneManager);
        if (IsDesignerFile)
        {
            decl.BaseTypes.Add(new CodeTypeReference(uFrameEditor.UFrameTypes.SceneManager));

            //var singleInstanceElements = elements.Where(p => !p.IsMultiInstance).ToArray();

            //foreach (var element in singleInstanceElements)
            //{


            //    //Container.RegisterInstance(AICheckersGameController.CreateAICheckersGame());
            //    //progress("Loading CheckersGame", 100);
            //}

        }
        else
        {
            decl.BaseTypes.Add(new CodeTypeReference(sceneManager.NameAsSceneManagerBase));

            var loadMethod = new CodeMemberMethod
            {
                Name = "Load",
                ReturnType = new CodeTypeReference(typeof (IEnumerator)),
                Attributes = MemberAttributes.Override | MemberAttributes.Public
            };
            loadMethod.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.UpdateProgressDelegate, "progress"));
            loadMethod.Statements.Add(new CodeCommentStatement("Use the controllers to create the game."));
            loadMethod.Statements.Add(new CodeSnippetExpression("yield break"));

            decl.Members.Add(loadMethod);
        }

        var setupMethod = new CodeMemberMethod()
        {
            Name = "Setup"
        };
        setupMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
        setupMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Setup"));
        decl.Members.Add(setupMethod);
        if (IsDesignerFile)
        {
            //var commands = sceneManager.SubSystem.IncludedCommands.ToArray();
            foreach (var sceneManagerTransition in sceneManager.Transitions)
            {
                //commands.Where(p=>p.Identifier == SceneManagerTransition.Id)
                var transitionItem = DiagramData.GetSceneManagers().FirstOrDefault(p => p.Identifier == sceneManagerTransition.ToIdentifier);
                if (transitionItem == null || transitionItem.SubSystem == null) continue;

                var settingsField = new CodeMemberField(transitionItem.NameAsSettings, sceneManagerTransition.NameAsSettingsField)
                {
                    Attributes = MemberAttributes.Public,
                    InitExpression = new CodeObjectCreateExpression(transitionItem.NameAsSettings)
                };

                decl.Members.Add(settingsField);

                var transitionMethod = new CodeMemberMethod()
                {
                    Name = sceneManagerTransition.Name,
                    Attributes = MemberAttributes.Public
                };

                var switchGameAndLevelCall =
                    new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(uFrameEditor.UFrameTypes.GameManager),
                        String.Format("TransitionLevel<{0}>", transitionItem.NameAsSceneManager));

                switchGameAndLevelCall.Parameters.Add(
                    new CodeSnippetExpression(string.Format("(container) =>{{container.{0} = {1}; }}",
                        transitionItem.NameAsSettingsField, settingsField.Name)));

                switchGameAndLevelCall.Parameters.Add(new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), settingsField.Name), "_Scenes"));

                transitionMethod.Statements.Add(switchGameAndLevelCall);
                decl.Members.Add(transitionMethod);
                //transitionMethod.Statements.Add(new CodeSnippetExpression(
                //    string.Format(
                //        "GameManager.SwitchGameAndLevel<CheckersMenuManager>((checkersMenu) =>{checkersMenu._CheckersMenuSettings = _QuitTransition;},\"CheckersMenu\")")));
            }


            List<ElementData> rootElements = new List<ElementData>();
            //List<ElementData> baseElements = new List<ElementData>();

            foreach (var element in elements)
            {
                if (element.IsTemplate) continue;
                if (Settings.GenerateControllers)
                {
                    decl.Members.Add(
                        new CodeSnippetTypeMember(string.Format("public {0} {0} {{ get; set; }}",
                            element.NameAsController)));

                    var property = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(),
                       element.NameAsController);

                    var assignProperty =
                        new CodeAssignStatement(property,
                            new CodeObjectCreateExpression(element.NameAsController)
                            );

                    setupMethod.Statements.Add(new CodeSnippetExpression(string.Format("this.{0} = new {0}() {{ Container = Container, Context = Context }}", element.NameAsController)));

                    var registerInstance = new CodeMethodInvokeExpression(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Container"), "RegisterInstance");
                    registerInstance.Parameters.Add(
                        new CodePropertyReferenceExpression(
                            new CodeThisReferenceExpression(), element.NameAsController));

                    registerInstance.Parameters.Add(new CodeSnippetExpression("false"));
                    setupMethod.Statements.Add(registerInstance);
                }

               
             

                //if ((element.BaseElement == null || element.BaseElement.IsTemplate) && !element.IsTemplate)
                //{
                //    if (AddTypeEnum(element, sceneManager, elements) == null)
                //    {
                //        baseElements.Add(element);

                //    }
                //    else
                //    {
                //        rootElements.Add(element);
                //    }

                //}
            }
            
            // TODO Change this method to register items that have been added
            //setupMethod.Statements
            foreach (var instance in Data.Instances)
            {
                var element = instance.RelatedNode() as ElementData;
                if (element == null) continue;
                if (Settings.GenerateControllers)
                {
                    setupMethod.Statements.Add(
                        new CodeSnippetExpression(
                            string.Format("Container.RegisterInstance<{0}>(SetupViewModel<{0}>({1}, \"{2}\"))",
                                element.NameAsViewModel, element.NameAsController, element.RootElement.Name)));
                }
                else
                {
                    setupMethod.Statements.Add(
                        new CodeSnippetExpression(
                            string.Format("Container.RegisterInstance<{0}>(SetupViewModel<{0}>(null, \"{1}\"))",
                                element.NameAsViewModel, element.RootElement.Name)));
                }
                
            }

            foreach (var element in rootElements)
            {
                var derived = element.DerivedElements.Where(p => elements.Contains(p)).ToArray();

                foreach (var derivedElement in derived.Concat(new[] { element }))
                {
                    var condition =
                        new CodeConditionStatement(new CodeBinaryOperatorExpression(
                            new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), sceneManager.NameAsSettingsField), element.NameAsTypeEnum
                                ), CodeBinaryOperatorType.ValueEquality,
                            new CodeSnippetExpression(string.Format("{0}.{1}",
                                sceneManager.NameAsSceneManager + element.NameAsTypeEnum, derivedElement.Name))));


                    condition.TrueStatements.Add(
                        new CodeVariableDeclarationStatement(new CodeTypeReference(derivedElement.NameAsViewModel),
                            derivedElement.NameAsVariable, new CodeSnippetExpression(string.Format("SetupViewModel<{1}>({0},\"{2}\")", Settings.GenerateControllers ? derivedElement.NameAsController : "null", derivedElement.NameAsViewModel,derivedElement.RootElement.Name))));

                    condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", derivedElement.NameAsViewModel, derivedElement.NameAsVariable)));
                    // TODO add a while here for each base type
                    foreach (var baseType in derivedElement.AllBaseTypes)
                    {
                        //if (baseType == derivedElement) continue;
                        //condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", baseType.NameAsController, derivedElement.NameAsController)));
                        //condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", element.NameAsController, baseType.NameAsController)));
                        condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", baseType.NameAsViewModel, derivedElement.NameAsVariable)));
                        if (Settings.GenerateControllers)
                            condition.TrueStatements.Add(new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1}, false)", baseType.NameAsController, derivedElement.NameAsController)));
                    }



                    //if (!derivedElement.IsMultiInstance)
                    //{
                    //    condition.TrueStatements.Add(
                    //        new CodeMethodInvokeExpression(
                    //        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Container"),
                    //        string.Format("RegisterInstance<{0}>", element.NameAsController), new CodeSnippetExpression()));

                    //}


                    setupMethod.Statements.Add(condition);
                }
            }

            setupMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Container"),
                    "InjectAll"));

            var settingsField2 = new CodeMemberField(sceneManager.NameAsSettings, sceneManager.NameAsSettingsField)
            {
                Attributes = MemberAttributes.Public,
                InitExpression = new CodeObjectCreateExpression(sceneManager.NameAsSettings)
            };
            decl.Members.Add(settingsField2);
            AddSceneManagerSettings(sceneManager, rootElements);
        }
        ProcessModifiers(decl);
        Namespace.Types.Add(decl);
    }

    public virtual void AddSceneManagerSettings( SceneManagerData sceneManagerData, List<ElementData> rootElements)
    {
        var decl = new CodeTypeDeclaration
        {
            Name = sceneManagerData.NameAsSettings,
            TypeAttributes = TypeAttributes.Sealed | TypeAttributes.Public,
            IsPartial = true
        };
        if (IsDesignerFile)
        {
            decl.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializableAttribute))));
            decl.Members.Add(new CodeMemberField(typeof(string[]), "_Scenes") { Attributes = MemberAttributes.Public });



            foreach (var element in rootElements)
            {
                var field = new CodeMemberField(sceneManagerData.NameAsSceneManager + element.NameAsTypeEnum,
                    element.NameAsTypeEnum) { Attributes = MemberAttributes.Public };
                decl.Members.Add(field);
            }

            
        }



        ProcessModifiers(decl);
        Namespace.Types.Add(decl);
    }

}

public class TypeEnumGenerator : CodeGenerator
{
    

}