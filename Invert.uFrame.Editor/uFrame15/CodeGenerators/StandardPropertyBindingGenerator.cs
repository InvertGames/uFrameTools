using System;
using System.CodeDom;
using Invert.uFrame.Editor;
using UnityEngine;

namespace Invert.uFrame.Code.Bindings
{
    public class StandardPropertyBindingGenerator : BindingGenerator
    {
        public ViewModelPropertyData PropertyData
        {
            get { return Item as ViewModelPropertyData; }
        }

        public override string Title
        {
            get { return "Standard Property Binding"; }
        }

        public override string Description
        {
            get { return "Subscribes to the property and is notified anytime the value changes."; }
        }

        public override string MethodName
        {
            get { return string.Format("{0}Changed", Item.Name); }
        }
        public string NameAsPrefabField
        {
            get { return string.Format("_{0}Prefab", Item.Name); }
        }
        public override bool IsApplicable
        {
            get { return PropertyData != null; }
        }

        public override void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition)
        {
            var memberInvoke = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "BindProperty");

            memberInvoke.Parameters.Add(
                     new CodeSnippetExpression(string.Format("{0}.{1}", Element.Name, Item.FieldName)));

            memberInvoke.Parameters.Add(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), Item.NameAsChangedMethod));

            if (RelatedElement != null && GenerateDefaultImplementation)
            {
                var viewPrefabField = CreateBindingField(typeof(GameObject).FullName, Item.Name,
                   "Prefab");
                collection.Add(viewPrefabField);
            }

            bindingCondition.TrueStatements.Add(memberInvoke);
        }

        public override void CreateMembers(CodeTypeMemberCollection collection)
        {
            //if (!asTwoWay)
            //{


            //SetterMethod = CreateMethodSignature(null, new CodeParameterDeclarationExpression(
            //    Item.GetPropertyType(), "value"));

            if (GenerateDefaultImplementation)
            {



            }
            collection.Add(SetterMethod);
        }

        public CodeMemberMethod SetterMethod { get; set; }
    }

    public class ComputedPropertyBindingGenerator : StandardPropertyBindingGenerator
    {
        public ComputedPropertyData ComputedProperty
        {
            get { return Item as ComputedPropertyData; }
        }

        public override bool IsApplicable
        {
            get { return ComputedProperty != null; }
        }

        public override void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition)
        {
            base.CreateBindingStatement(collection, bindingCondition);

        }
    }

    public class InstantiateViewPropertyBindingGenerator : StandardPropertyBindingGenerator
    {
        public override string Title
        {
            get { return "Instantiate View Property Binding"; }
        }

        public override bool IsApplicable
        {
            get { return RelatedElement != null && base.IsApplicable; }
        }
        
        public override void CreateMembers(CodeTypeMemberCollection collection)
        {
            base.CreateMembers(collection);
            if (GenerateDefaultImplementation)
            {
                collection.Add(this.CreateBindingField(typeof(GameObject).FullName, PropertyData.NameAsPrefabBindingOption));
                SetterMethod.Statements.Add(new CodeSnippetExpression(string.Format("this.ReplaceView({0}, value, {1})",PropertyData.ViewFieldName, PropertyData.NameAsPrefabBindingOption)));
            }

        }
    }

    public class CommandExecutedBindingGenerator : BindingGenerator
    {
        public override string Title
        {
            get { return "Command Executed Binding"; }
        }

        public override string Description
        {
            get { return string.Format("Invokes {0} when the {1} command is executed.", MethodName,Item.Name); }
        }

        public override string MethodName
        {
            get { return string.Format("{0}Executed", Item.Name); }
        }

        public override bool IsApplicable
        {
            get { return Item is ViewModelCommandData; }
        }

        public override void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition)
        {
            bindingCondition.TrueStatements.Add(
                new CodeSnippetExpression(string.Format("this.BindCommandExecuted({0}.{1}, {2})", Element.Name,
                    Item.Name, MethodName)));
        }

        public override void CreateMembers(CodeTypeMemberCollection collection)
        {
            base.CreateMembers(collection);

            collection.Add(CreateMethodSignature(null));
        }
    }

    public class StateMachinePropertyBindingGenerator : StandardPropertyBindingGenerator
    {
        public override string Title
        {
            get { return "State Machine Property Binding"; }
        }

        public override string Description
        {
            get { return "Subscribes to the state machine property and executes a method for each state."; }
        }

        public override bool IsApplicable
        {
            get { return StateMachine != null; }
        }

        public StateMachineNodeData StateMachine
        {
            get
            {
                return RelatedNode as StateMachineNodeData;
            }
        }
        public override void CreateMembers(CodeTypeMemberCollection collection)
        {
            base.CreateMembers(collection);
            foreach (var state in StateMachine.States)
            {
                var method = new CodeMemberMethod()
                {
                   Name = "On" + state.Name,
                   Attributes = MemberAttributes.Public
                };
                

                if (IsOverride)
                {
                    method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
                    method.Statements.Add(new CodeSnippetExpression(string.Format("base.{0}()", method.Name)));

                }
                else
                {
                    var conditionStatement =
                    new CodeConditionStatement(
                        new CodeSnippetExpression(string.Format("value is {0}", state.Name)));
                    conditionStatement.TrueStatements.Add(
                        new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), method.Name));

                    SetterMethod.Statements.Add(conditionStatement);
                }


                collection.Add(method);

            }
        }
    }
}