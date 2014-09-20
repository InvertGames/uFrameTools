using System;
using System.CodeDom;
using Invert.uFrame.Editor;
using UnityEngine;

namespace Invert.uFrame.Code.Bindings
{
    public class PropertyBindingGenerator : BindingGenerator
    {
        public ViewModelPropertyData PropertyData
        {
            get { return Item as ViewModelPropertyData; }
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
                     new CodeSnippetExpression(string.Format("()=>{0}.{1}", Element.Name, Item.FieldName)));
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


            var setterMethod = CreateMethodSignature(null, new CodeParameterDeclarationExpression(
                Item.GetPropertyType(), "value"));

            if (GenerateDefaultImplementation)
            {
                if (RelatedElement == null)
                {
                    //foreach (var viewBindingExtender in BindingExtenders)
                    //{
                    //    viewBindingExtender.ExtendPropertyBinding(data, setterMethod.Statements, propertyData, null);
                    //}
                }
                else
                {
                    setterMethod.Statements.Add(
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
                    setterMethod.Statements.Add(prefabSetCondition);
                }

              
            }
            collection.Add(setterMethod);
        }
    }

    public class ComputedPropertyBindingGenerator : PropertyBindingGenerator
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
            base.CreateBindingStatement(collection,bindingCondition);
        }
    }
}