using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Invert.uFrame.CodeGen.CodeDomExtensions;
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

            CreateDefaultMethods(sceneManager);
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
            CreateTransitionMethods(sceneManager);
            RegisterInstances();
            


            

            CreateSettingsField(sceneManager);
            AddSceneManagerSettings(sceneManager);
        }
        ProcessModifiers(Declaration);
        Namespace.Types.Add(Declaration);
    }

    private void CreateDefaultMethods(SceneManagerData sceneManager)
    {
        LoadMethod = new CodeMemberMethod
        {
            Name = "Load",
            ReturnType = new CodeTypeReference(typeof(IEnumerator)),
            Attributes = MemberAttributes.Override | MemberAttributes.Public
        };
        LoadMethod.Comments.AddRange(UFrameGeneratorComments.SceneManagerLoadMethod(sceneManager).ToArray());
        LoadMethod.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.UpdateProgressDelegate,
            "progress"));
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

    private void CreateSettingsField(SceneManagerData sceneManager)
    {
        SettingsField = new CodeMemberField(sceneManager.NameAsSettings, sceneManager.NameAsSettingsField)
        {
            Attributes = MemberAttributes.Public,
            InitExpression = new CodeObjectCreateExpression(sceneManager.NameAsSettings)
        };

        Declaration.Members.Add(SettingsField);
    }

    private void CreateTransitionMethods(SceneManagerData sceneManager)
    {
        foreach (var sceneManagerTransition in sceneManager.Transitions)
        {

            var command = sceneManagerTransition.Command;

            if (command == null) continue;
            if (string.IsNullOrEmpty(sceneManagerTransition.ToIdentifier)) continue;

            var transitionItem =
                DiagramData.GetSceneManagers().FirstOrDefault(p => p.Identifier == sceneManagerTransition.ToIdentifier);
            if (transitionItem == null || transitionItem.SubSystem == null) continue;

            var settingsField = new CodeMemberField(transitionItem.NameAsSettings,
                sceneManagerTransition.NameAsSettingsField)
            {
                Attributes = MemberAttributes.Public,
                InitExpression = new CodeObjectCreateExpression(transitionItem.NameAsSettings)
            };

            Declaration.Members.Add(settingsField);

            var transitionMethod = command.ToMethod(null, !IsDesignerFile, false);

            var switchGameAndLevelCall =
                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(uFrameEditor.UFrameTypes.GameManager),
                    String.Format("TransitionLevel<{0}>", transitionItem.NameAsSceneManager));

            var transitionCompleteMethod =
                command.ToMethod(string.Format("{0}TransitionComplete", sceneManagerTransition.Name), !IsDesignerFile);
            transitionCompleteMethod.Parameters.Clear();
            transitionCompleteMethod.Parameters.Insert(0, new CodeParameterDeclarationExpression(transitionItem.NameAsSceneManager, "sceneManager"));
            Declaration.Members.Add(transitionCompleteMethod);


            var getSceneMethod = command.ToMethod(string.Format("Get{0}Scenes", sceneManagerTransition.Name), !IsDesignerFile, false);
            getSceneMethod.ReturnType = new CodeTypeReference(typeof(IEnumerable<string>));

            switchGameAndLevelCall.Parameters.Add(
                new CodeSnippetExpression(string.Format("(container) =>{{container.{0} = {1}; {2}(container); }}",
                    transitionItem.NameAsSettingsField, settingsField.Name, transitionCompleteMethod.Name)));

            getSceneMethod.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), settingsField.Name), "_Scenes")));
            Declaration.Members.Add(getSceneMethod);
            switchGameAndLevelCall.Parameters.Add(new CodeMethodInvokeExpression(command.ToInvoke(getSceneMethod.Name), "ToArray"));

            transitionMethod.Statements.Add(switchGameAndLevelCall);
            Declaration.Members.Add(transitionMethod);
        }
    }

    protected virtual void RegisterInstances()
    {
        var initializeMethod = new CodeMemberMethod()
        {
            Name = "Initialize",
            Attributes = MemberAttributes.Override | MemberAttributes.Public
        };
        initializeMethod.Statements.Add(new CodeSnippetExpression("base.Initialize()"));
        Declaration.Members.Add(initializeMethod);

        var transitionCommands = Data.Transitions.Where(p => p.Command != null && !string.IsNullOrEmpty(p.ToIdentifier)).Select(p => p.Command).ToArray();
        var instancesRegistered = new List<string>();
        var initializeStatements = new CodeStatementCollection();
        foreach (var instanceGroup in Data.SubSystem.AllInstances.GroupBy(p => p.Name))
        {
            foreach (var instance in instanceGroup)
            {
                var element = instance.RelatedNode() as ElementData;
                if (element == null) continue;
                if (!instancesRegistered.Contains(instanceGroup.Key))
                {
                    var instanceField = new CodeMemberField(element.NameAsViewModel, "_" + instance.Name);
                    var instanceProperty = instanceField.EncapsulateField(instance.Name, new CodeSnippetExpression(
                        string.Format("CreateInstanceViewModel<{0}>({1}, \"{2}\")",
                            element.NameAsViewModel, element.NameAsController, instance.Name)
                        ), null, true);
                    instanceProperty.CustomAttributes.Add(
                        new CodeAttributeDeclaration(new CodeTypeReference("Inject"),
                            new CodeAttributeArgument(new CodePrimitiveExpression(instance.Name))));
                    initializeStatements.Add(
                        new CodeSnippetExpression(string.Format("{0}.Initialize({1})", element.NameAsController,
                            instanceProperty.Name)));
                    Declaration.Members.Add(instanceField);
                    Declaration.Members.Add(instanceProperty);
                }

                instancesRegistered.Add(instanceGroup.Key);

                foreach (var command in element.Commands)
                {


                    if (!transitionCommands.Contains(command)) continue;

                    if (!string.IsNullOrEmpty(command.RelatedTypeName))
                    {

                        initializeMethod.Statements.Add(
                            new CodeSnippetExpression(
                                string.Format(
                                    "{0}.{1}.Subscribe(_=> {1}(({2}){0}.{1}.Parameter)).DisposeWith(this.gameObject)",
                                    instance.Name, command.Name, command.RelatedTypeName)));
                    }
                    else
                    {
                        initializeMethod.Statements.Add(
                            new CodeSnippetExpression(
                                string.Format(
                                    "{0}.{1}.Subscribe(_=> {1}()).DisposeWith(this.gameObject)",
                                    instance.Name, command.Name)));
                    }
                }

                SetupMethod.Statements.Add(
                    new CodeSnippetExpression(string.Format("Container.RegisterViewModel<{0}>({1},\"{1}\")",
                        instance.RelatedTypeName, instanceGroup.Key)));
            }
        }
        RegisterControllers();

        SetupMethod.Statements.Add(
                new CodeMethodInvokeExpression(
                    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Container"),
                    "InjectAll"));
        SetupMethod.Statements.AddRange(initializeStatements);
    }

    private void RegisterControllers()
    {
        foreach (var element in Data.AllImportedElements.Distinct())
        {
            var controllerField = new CodeMemberField(element.NameAsController, "_" + element.NameAsController);
            var controllerProperty = controllerField.EncapsulateField(element.NameAsController,
                new CodeSnippetExpression(string.Format("new {0}() {{ Container = Container }}",
                    element.NameAsController)), null, true);
            controllerProperty.CustomAttributes.Add(
                new CodeAttributeDeclaration(new CodeTypeReference("Inject")));
            Declaration.Members.Add(controllerField);
            Declaration.Members.Add(controllerProperty);

            SetupMethod.Statements.Add(
                new CodeSnippetExpression(string.Format("Container.RegisterController<{0}>({0})",
                    element.NameAsController)));
        }
    }

    public CodeMemberMethod UnloadMethod { get; set; }

    public CodeMemberMethod OnLoadingMethod { get; set; }

    public CodeMemberMethod OnLoadedMethod { get; set; }

    public CodeMemberField SettingsField { get; set; }

    public CodeMemberMethod LoadMethod { get; set; }

    public CodeMemberMethod SetupMethod { get; set; }

    public CodeTypeDeclaration Declaration { get; set; }

    public virtual void AddSceneManagerSettings(SceneManagerData sceneManagerData)
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



            //foreach (var element in rootElements)
            //{
            //    var field = new CodeMemberField(sceneManagerData.NameAsSceneManager + element.NameAsTypeEnum,
            //        element.NameAsTypeEnum) { Attributes = MemberAttributes.Public };
            //    decl.Members.Add(field);
            //}


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