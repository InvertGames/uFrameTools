using System.CodeDom;
using Invert.uFrame.Editor;
using UnityEngine;

namespace Invert.uFrame.Code.Bindings
{
    public class ViewCollectionBindingGenerator : CollectionBindingGenerator
    {
        public override bool IsApplicable
        {
            get { return base.IsApplicable && RelatedElement != null; }
        }

        public override string Title
        {
            get { return "View Collection Binding"; }
        }

        public override string Description
        {
            get { return "This binding will add or remove views based on an element/viewmodel collection."; }
        }

        public override string MethodName
        {
            get { return string.Format("Create{0}View", Item.Name); }
        }

        public override string ParameterTypeName
        {
            get { return base.ParameterTypeName; }
        }

        public override string VarName
        {
            get { return base.VarName; }
        }

        public override void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition)
        {
            base.CreateBindingStatement(collection, bindingCondition);

            // Create the needed fields

            var sceneFirstField = CreateBindingField(typeof(bool).FullName, CollectionProperty.Name,
                "SceneFirst");
            collection.Add(sceneFirstField);


            var containerField = CreateBindingField(typeof(Transform).FullName, CollectionProperty.Name,
                "Container");
            collection.Add(containerField);

            bindingCondition.TrueStatements.Add(
                new CodeSnippetExpression(string.Format("this.BindToViewCollection( {0}.{1}, {2}, {3}, {4}, {5}, {6})",
                    Element.Name, CollectionProperty.FieldName,
                    string.Format("viewModel=>{{ return {0}(viewModel as {1}); }}", MethodName, RelatedElement.NameAsViewModel),
                    AddMethodName,
                    RemovedMethodName,
                    containerField.Name,
                    sceneFirstField.Name
                )));
        }

        public override void CreateMembers(CodeTypeMemberCollection collection)
        {
            base.CreateMembers(collection);
            var createHandlerMethod = CreateMethodSignature(new CodeTypeReference(uFrameEditor.UFrameTypes.ViewBase),
                new CodeParameterDeclarationExpression(RelatedElement.NameAsViewModel, VarName));

            
            if (!IsOverride)
            {
                createHandlerMethod.Statements.Clear();
                createHandlerMethod.Statements.Add(
                    new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                        "InstantiateView", new CodeVariableReferenceExpression(VarName))));
             
            }
            else
            {
                //createHandlerMethod.Statements.Add(
                //   new CodeMethodReturnStatement(new CodeSnippetExpression(string.Format("base.{0}({1})", createHandlerMethod.Name, VarName))));
                
            }
            collection.Add(createHandlerMethod);

            CreateAddMembers(collection);
            CreateRemoveMembers(collection);
        }

        public override void CreateAddMembers(CodeTypeMemberCollection collection)
        {
           
            var addHandlerMethod = CreateMethodSignatureWithName(AddMethodName, new CodeParameterDeclarationExpression(ParameterTypeName, VarName));

            collection.Add(addHandlerMethod);
        }

        public override void CreateRemoveMembers(CodeTypeMemberCollection collection)
        {
            //decl.Members.Add(addHandlerMethod);
            var removeHandlerMethod = CreateMethodSignatureWithName(RemovedMethodName, new CodeParameterDeclarationExpression(ParameterTypeName, VarName));


            collection.Add(removeHandlerMethod);
        }
    }
}