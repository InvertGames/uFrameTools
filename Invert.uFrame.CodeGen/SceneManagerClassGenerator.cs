using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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

    public virtual void AddSceneManager(SceneManagerData sceneManager)
    {
        // Grab the root items
        var subSystem = sceneManager.SubSystem;
        if (subSystem == null)
        {
            Debug.LogWarning(string.Format("Scene Manager {0} doesn't have an associated SubSystem.  To create the type please associate one.", sceneManager.Name));
            return;
        }

        Declaration = new CodeTypeDeclaration(IsDesignerFile ? sceneManager.NameAsSceneManagerBase : sceneManager.NameAsSceneManager);
        Declaration.Comments.AddRange(
            UFrameGeneratorComments.SceneManagerDeclaration(sceneManager).ToArray());
        if (IsDesignerFile)
        {
            Declaration.BaseTypes.Add(new CodeTypeReference(uFrameEditor.UFrameTypes.SceneManager));

        }
        else
        {
            Declaration.BaseTypes.Add(new CodeTypeReference(sceneManager.NameAsSceneManagerBase));

            LoadMethod = new CodeMemberMethod
            {
                Name = "Load",
                ReturnType = new CodeTypeReference(typeof(IEnumerator)),
                Attributes = MemberAttributes.Override | MemberAttributes.Public
            };
            LoadMethod.Comments.AddRange(UFrameGeneratorComments.SceneManagerLoadMethod(sceneManager).ToArray());
            LoadMethod.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.UpdateProgressDelegate, "progress"));
            LoadMethod.Statements.Add(new CodeCommentStatement("Use the controllers to create the game."));
            LoadMethod.Statements.Add(new CodeSnippetExpression("yield break"));

            OnLoadedMethod = new CodeMemberMethod()
            {
                Name = "OnLoaded",
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            OnLoadedMethod.Statements.Add(new CodeSnippetExpression("base.OnLoaded()"));
            OnLoadedMethod.Comments.AddRange(UFrameGeneratorComments.OnLoadedMethod(sceneManager).ToArray());

            OnLoadingMethod = new CodeMemberMethod()
            {
                Name = "OnLoading",
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            OnLoadingMethod.Statements.Add(new CodeSnippetExpression("base.OnLoading()"));
            OnLoadingMethod.Comments.AddRange(UFrameGeneratorComments.OnLoadingMethod(sceneManager).ToArray());


            UnloadMethod = new CodeMemberMethod()
            {
                Name = "Unload",
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            UnloadMethod.Statements.Add(new CodeSnippetExpression("base.Unload()"));
            UnloadMethod.Comments.AddRange(UFrameGeneratorComments.OnUnloadMethod(sceneManager).ToArray());

            Declaration.Members.Add(OnLoadingMethod);
            Declaration.Members.Add(LoadMethod);
            Declaration.Members.Add(OnLoadedMethod);
            Declaration.Members.Add(UnloadMethod);

        }

        SetupMethod = new CodeMemberMethod()
        {
            Name = "Setup"
        };
        SetupMethod.Comments.AddRange(
            UFrameGeneratorComments.SceneManagerSetupMethod(sceneManager).ToArray());

        SetupMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
        SetupMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "Setup"));
        Declaration.Members.Add(SetupMethod);
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

                Declaration.Members.Add(settingsField);

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
                Declaration.Members.Add(transitionMethod);
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

                        Declaration.Members.Add(instanceField);
                        Declaration.Members.Add(instanceProperty);
                    }   
                    instancesRegistered.Add(instanceGroup.Key);
               

                    SetupMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format("Container.RegisterInstance<{0}>({1},\"{1}\")",instance.RelatedTypeName, instanceGroup.Key, instance.Name)));
                }
            }

            
            foreach (var element in Data.AllImportedElements.Distinct())
            {
                var controllerField = new CodeMemberField(element.NameAsController, "_" + element.NameAsController);
                var controllerProperty = controllerField.EncapsulateField(element.NameAsController,
                    new CodeSnippetExpression(string.Format("new {0}() {{ Container = Container, Context = Context }}",
                        element.NameAsController)),null,true);
                controllerProperty.CustomAttributes.Add(
                    new CodeAttributeDeclaration(new CodeTypeReference("Inject")));
                Declaration.Members.Add(controllerField);
                Declaration.Members.Add(controllerProperty);

                SetupMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format("Container.RegisterInstance({0},false)",
                        element.NameAsController)));
            }

            SetupMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Container"),
                    "InjectAll"));

            SettingsField = new CodeMemberField(sceneManager.NameAsSettings, sceneManager.NameAsSettingsField)
            {
                Attributes = MemberAttributes.Public,
                InitExpression = new CodeObjectCreateExpression(sceneManager.NameAsSettings)
            };
            Declaration.Members.Add(SettingsField);
            AddSceneManagerSettings(sceneManager, rootElements);
        }
        ProcessModifiers(Declaration);
        Namespace.Types.Add(Declaration);
    }

    public CodeMemberMethod UnloadMethod { get; set; }

    public CodeMemberMethod OnLoadingMethod { get; set; }

    public CodeMemberMethod OnLoadedMethod { get; set; }

    public CodeMemberField SettingsField { get; set; }

    public CodeMemberMethod LoadMethod { get; set; }

    public CodeMemberMethod SetupMethod { get; set; }

    public CodeTypeDeclaration Declaration { get; set; }

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

public static class UFrameGeneratorComments
{
    public static IEnumerable<CodeCommentStatement> SceneManagerDeclaration(SceneManagerData sceneManager)
    {
        return CodeComment("The responsibility of this class is to manage the scenes Initialization, Loading, Transitioning, and Unloading.");
    }

    public static IEnumerable<CodeCommentStatement> SceneManagerSetupMethod(SceneManagerData sceneManager)
    {
        return CodeComment("This method is the first method to be invoked when the scene first loads. Anything registered " +
                               "here with 'Container' will effectively be injected on controllers, and instances defined on a subsystem." +
                               "And example of this would be Container.RegisterInstance<IDataRepository>(new CodeRepository()). " +
                               "Then any property with the 'Inject' attribute on any controller or view-model will automatically be set by uFrame. ");
    }

    public static IEnumerable<CodeCommentStatement> SceneManagerLoadMethod(SceneManagerData sceneManager)
    {
        return CodeComment("This method loads the scene instantiating any views or regular prefabs required by the scene.  " +
                               "This method is invoked after Setup() and Initialize().  Note:" +
                               " be sure to report progress to the delgate supplied in the first parameter.  It will update " +
                               "the loading screen to display a nice status message.");
    }

    public static IEnumerable<CodeCommentStatement> OnLoadedMethod(SceneManagerData sceneManager)
    {
        return CodeComment("This method is invoked after uFrame has completed its scene boot and the game is ready to begin.  " +
                               "Here would be a good place to use the generated Controller properties on this class to invoke some gameplay logic initialization");
    }

    public static IEnumerable<CodeCommentStatement> OnLoadingMethod(SceneManagerData sceneManager)
    {
        return CodeComment("This method is invoked exactly right before the Load method is invoked.");
    }

    public static IEnumerable<CodeCommentStatement> OnUnloadMethod(SceneManagerData sceneManager)
    {
        return CodeComment("This method is invoked when transitioning to another scene.  This could be used to destory a scene or effectively log a user off.");
    }

    public static IEnumerable<CodeCommentStatement> CodeComment(string comment)
    {
        if (!uFrameEditor.CurrentProject.GeneratorSettings.GenerateComments) yield break;
        var sb = new StringBuilder();
        yield return new CodeCommentStatement("<summary>");
        var whitespace = 0;
        foreach (var c in comment)
        {
            sb.Append(c);
            if (Char.IsWhiteSpace(c))
                whitespace++;
            if (whitespace > 20)
            {
                yield return new CodeCommentStatement(sb.ToString());
                sb = new StringBuilder();
                whitespace = 0;
            }
        }
        yield return new CodeCommentStatement(sb.ToString());
        yield return new CodeCommentStatement("</summary>");

    }
}