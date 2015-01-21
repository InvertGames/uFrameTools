using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using UnityEngine;

public class StateMachineClassGenerator : CodeGenerator
{
    public INodeRepository Repository { get; set; }
    public StateMachineNodeData Data { get; set; }
    public Type StateMachineType { get; set; }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        if (!Data.HasStartingState)
        {
            UnityEngine.Debug.Log("You don't have a starting state for ");
        }
        Namespace.Imports.Add(new CodeNamespaceImport("Invert.StateMachine"));
        BaseTypeDecleration = new CodeTypeDeclaration(Data.Name);

        Constructor = new CodeConstructor()
        {
            Name = BaseTypeDecleration.Name,
            Attributes = MemberAttributes.Public,
        };
        Constructor.Parameters.Add(new CodeParameterDeclarationExpression(uFrameEditor.UFrameTypes.ViewModel, "vm"));
        Constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "propertyName"));
        Constructor.BaseConstructorArgs.Add(new CodeSnippetExpression("vm"));
        Constructor.BaseConstructorArgs.Add(new CodeSnippetExpression("propertyName"));
        BaseTypeDecleration.Members.Add(Constructor);

        if (IsDesignerFile)
        {

            foreach (var item in Data.Transitions)
            {
                var field = new CodeMemberField("StateMachineTrigger", "_" + item.Name);
                var property = field.EncapsulateField(item.Name,
                    new CodeSnippetExpression(string.Format("new StateMachineTrigger(this, \"{0}\")", item.Name)));
                BaseTypeDecleration.Members.Add(field);
                BaseTypeDecleration.Members.Add(property);
            }

            BaseTypeDecleration.BaseTypes.Add(StateMachineType);
            BaseTypeDecleration.Name += "Base";


            var startState = Data.StartState ?? Data.States.FirstOrDefault();
            if (startState != null)
            {
                StartStateProperty = new CodeMemberProperty()
                {
                    Name = "StartState",
                    Attributes = MemberAttributes.Override | MemberAttributes.Public,
                    Type = new CodeTypeReference(uFrameEditor.UFrameTypes.State)
                };
                StartStateProperty.GetStatements.Add(new CodeMethodReturnStatement(
                   new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), Data.StartState.Name)
                ));
                BaseTypeDecleration.Members.Add(StartStateProperty);
            }


            ComposeMethod = new CodeMemberMethod()
            {
                Name = "Compose",
                Attributes = MemberAttributes.Override | MemberAttributes.Public
            };

            ComposeMethod.Parameters.Add(new CodeParameterDeclarationExpression("List<State>", "states"));
            ComposeMethod.Statements.Add(new CodeSnippetExpression("base.Compose(states)"));
            BaseTypeDecleration.Members.Add(ComposeMethod);

            foreach (var state in Data.States)
            {
                var field = new CodeMemberField() { Name = "_" + state.Name, Type = new CodeTypeReference(state.Name) };
                var property = field.EncapsulateField(state.Name,
                    new CodeObjectCreateExpression(state.Name));
                ComposeMethod.Statements.Add(new CodeSnippetExpression(string.Format("this.{0}.StateMachine = this", state.Name)));

                foreach (var transition in state.Transitions)
                {
                    var transitionTo = transition.TransitionTo;
                    if (transitionTo == null) continue; // TODO THROW AN ERROR HERE
                    //var createTransitionExpression = new CodeObjectCreateExpression(typeof (StateTransition),
                    //    new CodePropertyReferenceExpression(new CodeThisReferenceExpression(),
                    //        transition.TransitionTo.Name));

                    ComposeMethod.Statements.Add(
                        new CodeSnippetExpression(string.Format("{0}.{1} = new StateTransition(\"{1}\", {2},{3})", state.Name, transition.Name, state.Name, transitionTo.Name)));

                }
                foreach (var t in state.Transitions)
                {
                    if (t.Transition == null) continue;
                    ComposeMethod.Statements.Add(new CodeSnippetExpression(string.Format("{0}.AddTrigger({1}, {0}.{1})", property.Name, t.Transition.Name)));
                }
                ComposeMethod.Statements.Add(new CodeSnippetExpression(string.Format("states.Add({0})", state.Name)));
                BaseTypeDecleration.Members.Add(field);
                BaseTypeDecleration.Members.Add(property);
            }

            //foreach (var variable in Data.Variables)
            //{
            //    var field = new CodeMemberField(variable.RelatedTypeName, "_" + variable.Name)
            //    {
            //        Attributes = MemberAttributes.Private
            //    };
            //    field.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof (SerializeField))));
            //    var property = field.EncapsulateField(variable.Name);
            //    Decleration.Members.Add(field);
            //    Decleration.Members.Add(property);
            //}
        }
        else
        {
            BaseTypeDecleration.BaseTypes.Add(Data.Name + "Base");
        }
        Namespace.Types.Add(BaseTypeDecleration);
    }

    public CodeMemberProperty StartStateProperty { get; set; }

    public CodeMemberMethod ComposeMethod { get; set; }

    public CodeConstructor Constructor { get; set; }

    public CodeTypeDeclaration BaseTypeDecleration { get; set; }
}