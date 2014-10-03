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


            SetterMethod = CreateMethodSignature(null, new CodeParameterDeclarationExpression(
                Item.GetPropertyType(), "value"));

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
                SetterMethod.Statements.Add(
               new CodeConditionStatement(
                   new CodeSnippetExpression(string.Format(
                       "value == null && {0} != null && {0}.gameObject != null", Item.ViewFieldName)),
                   new CodeExpressionStatement(
                       new CodeMethodInvokeExpression(null,
                           "Destroy",
                           new CodeSnippetExpression(string.Format("{0}.gameObject", Item.ViewFieldName))))));

                var prefabSetCondition =
                    new CodeConditionStatement(
                        new CodeSnippetExpression(String.Format((string)"{0} == null ", (object)NameAsPrefabField)));

                prefabSetCondition.TrueStatements.Add(new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), Item.ViewFieldName),
                    new CodeCastExpression(new CodeTypeReference(RelatedElement.NameAsViewBase),
                        new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InstantiateView",
                            new CodeVariableReferenceExpression("value"))
                        )));

                prefabSetCondition.FalseStatements.Add(new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), Item.ViewFieldName),
                    new CodeCastExpression(new CodeTypeReference(RelatedElement.NameAsViewBase),
                        new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InstantiateView",
                            new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), NameAsPrefabField),
                            new CodeVariableReferenceExpression("value"))
                        )));
                SetterMethod.Statements.Add(prefabSetCondition);
            }
           
        }
    }
}