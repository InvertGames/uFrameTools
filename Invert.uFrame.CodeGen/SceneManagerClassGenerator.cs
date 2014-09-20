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
        get;
        set;
    }
    public INodeRepository DiagramData
    {
        get;
        set;
    }

    //public CodeTypeDeclaration AddTypeEnum(string name, IEnumerable<RegisteredInstanceData> instances)
    //{
    //    var enumDecleration = new CodeTypeDeclaration(name) { IsEnum = true };
    //    //enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, name));
    //    foreach (var item in instances)
    //    {
    //        enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, item.Name));
    //    }
    //    Namespace.Types.Add(enumDecleration);

    //    return enumDecleration;
    //}

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
                ReturnType = new CodeTypeReference(typeof(IEnumerator)),
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
            foreach (var sceneManagerTransition in sceneManager.Transitions)
            {
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


            var rootElements = new List<ElementData>();
            var instancesRegistered = new List<string>();
            foreach (var instanceGroup in Data.SubSystem.AllInstances.GroupBy(p=>p.Name))
            {
                foreach (var instance in instanceGroup)
                {
                    if (!instancesRegistered.Contains(instanceGroup.Key))
                    {
                        var element = instance.RelatedNode() as ElementData;
                        if (element == null) continue;

                        var instanceField = new CodeMemberField(element.NameAsViewModel, "_" + instance.Name);
                        var instanceProperty = instanceField.EncapsulateField(instance.Name, new CodeSnippetExpression(
                            string.Format("SetupViewModel<{0}>({1}, \"{2}\")",
                                        element.NameAsViewModel, element.NameAsController, instance.Name)
                            ), null, true);
                        instanceProperty.CustomAttributes.Add(
                            new CodeAttributeDeclaration(new CodeTypeReference("Inject"),
                                new CodeAttributeArgument(new CodePrimitiveExpression(instance.Name))));

                        decl.Members.Add(instanceField);
                        decl.Members.Add(instanceProperty);
                    }   
                    instancesRegistered.Add(instanceGroup.Key);
               

                    setupMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1},\"{1}\")",instance.RelatedTypeName, instanceGroup.Key, instance.Name)));
                }
            }
            

            foreach (var element in Data.SubSystem.GetIncludedElements())
            {
                var controllerField = new CodeMemberField(element.NameAsController, "_" + element.NameAsController);
                var controllerProperty = controllerField.EncapsulateField(element.NameAsController,
                    new CodeSnippetExpression(string.Format("new {0}() {{ Container = Container, Context = Context }}",
                        element.NameAsController)),null,true);
                controllerProperty.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference("Inject")));
                decl.Members.Add(controllerField);
                decl.Members.Add(controllerProperty);

                setupMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format("Container.RegisterInstance({0},false)",
                        element.NameAsController)));
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

    public virtual void AddSceneManagerSettings(SceneManagerData sceneManagerData, List<ElementData> rootElements)
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