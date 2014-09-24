using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using UnityEngine;

namespace Invert.uFrame.Code.Bindings
{
    public abstract class CollectionBindingGenerator : BindingGenerator
    {
        

        public string NameAsListField
        {
            get { return string.Format("_{0}List", Item.Name); }
        }
        public string NameAsSceneFirstField
        {
            get { return string.Format("_{0}SceneFirst", Item.Name); }
        }
        public string NameAsContainerField
        {
            get { return string.Format("_{0}Container", Item.Name); }
        }


        public override bool IsApplicable
        {
            get { return CollectionProperty != null; }
        }

        public bool IsViewModelBinding { get; set; }

        public ViewModelCollectionData CollectionProperty
        {
            get
            {
                return Item as ViewModelCollectionData;
            }
        }

        public bool HasField(CodeTypeMemberCollection collection, string name)
        {
            return collection.OfType<CodeMemberField>().Any(item => item.Name == name);
        }
        public override void CreateMembers(CodeTypeMemberCollection collection)
        {
            base.CreateMembers(collection);
        
        }

        public override void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition)
        {

            
        }

        public string VarTypeName
        {
            get { return RelatedElement == null ? CollectionProperty.RelatedTypeName : RelatedElement.NameAsViewModel; }
        }

        public virtual string ParameterTypeName
        {
            get { return RelatedElement == null ? CollectionProperty.RelatedTypeName : "ViewBase"; }
        }

        public virtual string VarName
        {
            get { return "item"; }
        }

        public string AddMethodName
        {
            get { return string.Format("{0}Added", Item.Name); }
        }

        public string RemovedMethodName
        {
            get { return string.Format("{0}Removed", Item.Name); }
        }

        public abstract void CreateAddMembers(CodeTypeMemberCollection collection);

        public abstract void CreateRemoveMembers(CodeTypeMemberCollection collection);
    }
}