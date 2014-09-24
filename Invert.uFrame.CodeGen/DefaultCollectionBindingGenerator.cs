using System.CodeDom;

namespace Invert.uFrame.Code.Bindings
{
    public class DefaultCollectionBindingGenerator : CollectionBindingGenerator
    {
        public override string Title
        {
            get { return "Standard Collection Binding"; }
        }

        public override string Description
        {
            get
            {
                return "Subscribes to collection modifications.  Add & Remove methods are invoked for each modification.";
            }
        }

        public override string MethodName
        {
            get { return AddMethodName; }
        }

        public override bool IsApplicable
        {
            get { return Item is ViewModelCollectionData; }
        }

        public override string ParameterTypeName
        {
            get
            {
                
                if (RelatedElement != null)
                {
                    return RelatedElement.NameAsViewModel;
                }
                return Item.RelatedTypeName;
            }
        }

        public override void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition)
        {
            base.CreateBindingStatement(collection, bindingCondition);

            bindingCondition.TrueStatements.Add(
                 new CodeSnippetExpression(string.Format("this.BindCollection({0}.{1}, {2}, {3})", Element.Name,
                     CollectionProperty.FieldName,this.AddMethodName,this.RemovedMethodName)));


        }

        public override void CreateMembers(CodeTypeMemberCollection collection)
        {
            //base.CreateMembers(collection);
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
            var removeHandlerMethod = CreateMethodSignatureWithName(RemovedMethodName, new CodeParameterDeclarationExpression(ParameterTypeName, VarName));
            collection.Add(removeHandlerMethod);
        }
    }
}