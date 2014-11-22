using System;
using System.CodeDom;
using System.Collections.Generic;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using UnityEngine;

public class StateMachineStateClassGenerator : CodeGenerator
{
    public StateMachineStateData Data { get; set; }
    public Type StateMachineType { get; set; }
    public Type StateType { get; set; }

    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        Decleration = new CodeTypeDeclaration(Data.Name);
        if (IsDesignerFile)
        {
            Decleration.BaseTypes.Add(StateType);
            var composeMethod = new CodeMemberMethod()
            {
                Name = "Compose",
                Attributes = MemberAttributes.Override | MemberAttributes.Public
            };
           // composeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(List<StateTransition>), "transitions"));
            composeMethod.Parameters.Add(new CodeParameterDeclarationExpression("List<StateTransition>", "transitions"));
            composeMethod.Statements.Add(new CodeSnippetExpression("base.Compose(transitions)"));

            //Decleration.Members.Add(composeMethod);
            foreach (var transition in Data.Transitions)
            {
                var transitionTo = transition.TransitionTo;
                if (transitionTo == null) continue;

                var field = new CodeMemberField("StateTransition", "_" + transition.Name)
                {
                    Attributes = MemberAttributes.Private
                };
                //field.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializeField))));
                
                var property = field.EncapsulateField(transition.Name);
                Decleration.Members.Add(field);
                Decleration.Members.Add(property);

                composeMethod.Statements.Add(new CodeSnippetExpression(string.Format("transitions.Add(this.{0});", transition.Name)));
                var transitionMethod = new CodeMemberMethod()
                {
                    Name = transition.Name + "Transition",
                    
                };
                transitionMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),"Transition",new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), transition.Name)))
                ;
                Decleration.Members.Add(transitionMethod);
            }
            //foreach (var transition in Data.Transitions)
            //{
            //    var field = new CodeMemberField(typeof(StateTransition), "_" + transition.Name)
            //    {
            //        Attributes = MemberAttributes.Private
            //    };
            //    field.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializeField))));
            //    var property = field.EncapsulateField(transition.Name);
            //    Decleration.Members.Add(field);
            //    Decleration.Members.Add(property);
            //}

            // Override the name property of the state class
            var nameProperty = new CodeMemberProperty()
            {
                Name = "Name",
                Type = new CodeTypeReference(typeof(string)),
                Attributes = MemberAttributes.Override | MemberAttributes.Public
            };

            nameProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(Data.Name)));
            Decleration.Members.Add(nameProperty);
        }
  
        //var updateMethod = new CodeMemberMethod() { Name = "Update", Attributes = MemberAttributes.Public | MemberAttributes.Override };
        //updateMethod.Statements.Add(new CodeSnippetExpression("base.Update()"));
        //Decleration.Members.Add(updateMethod);

        Namespace.Types.Add(Decleration);
    }

    public CodeTypeDeclaration Decleration { get; set; }
}