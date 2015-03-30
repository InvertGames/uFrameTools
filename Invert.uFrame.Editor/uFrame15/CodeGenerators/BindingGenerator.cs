using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Microsoft.CSharp;
using UnityEngine;

namespace Invert.uFrame.Code.Bindings
{
    public abstract class BindingGenerator : IBindingGenerator
    {

        public bool CallBase { get; set; }

        public virtual string BindingConditionFieldName
        {
            get { return "_Bind" + Item.Name; }
        }

        public IBindableTypedItem Item { get; set; }

        public IDiagramNode RelatedNode
        {
            get { return Item.RelatedNode(); }
        }

        public ElementData RelatedElement
        {
            get { return RelatedNode as ElementData; }
        }

  

        public INodeRepository DiagramData
        {
            get { return RelatedElement.Project; }
        }
        public bool GenerateDefaultImplementation { get; set; }
        public ElementData Element { get; set; }
        public bool IsBase { get; set; }

        public CodeMemberField CreateBindingField(string typeFullName, string propertyName, string name="", bool keepHidden = false)
        {
            var memberField =
                new CodeMemberField(
                    typeFullName,
                    "_" + propertyName + name) { Attributes = MemberAttributes.Public };
            if (!keepHidden)
            {
                memberField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.UFrameTypes.UFGroup),
                    new CodeAttributeArgument(new CodePrimitiveExpression(propertyName))));
            }

            memberField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));
            return memberField;
        }

        public virtual string GetMethodName(ITypedItem itemData)
        {
            return string.Empty;
        }

        public abstract string Title { get; }
        public abstract string Description { get;  }
        public abstract string MethodName { get; }

        public abstract bool IsApplicable { get; }

        public bool IsOverride { get; set; }

        public virtual void CreateMembers(CodeTypeMemberCollection collection)
        {

        }

        public abstract void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition);

        public override string ToString()
        {
            var collection = new CodeTypeMemberCollection();
            CreateMembers(collection);
            
            return CodeDomHelpers.GenerateCodeFromMembers(collection);
        }


        public virtual CodeMemberMethod CreateMethodSignature(CodeTypeReference returnType = null, params CodeParameterDeclarationExpression[] vars)
        {
            return CreateMethodSignature(returnType, true, vars);
        }

        public virtual CodeMemberMethod CreateMethodSignature(CodeTypeReference returnType = null, bool callBase = true, params CodeParameterDeclarationExpression[] vars)
        {
            return CreateMethodSignature(returnType, MemberAttributes.Public, callBase, vars);
        }

        public virtual CodeMemberMethod CreateMethodSignature(CodeTypeReference returnType = null, MemberAttributes attribute = MemberAttributes.Public, bool callBase = true, params CodeParameterDeclarationExpression[] vars)
        {
            return CreateMethodSignatureWithName(MethodName, returnType, attribute, callBase, vars);
        }

        public virtual CodeMemberMethod CreateMethodSignatureWithName(string name, params CodeParameterDeclarationExpression[] vars)
        {
            return CreateMethodSignatureWithName(name, false, vars);
        }

        public virtual CodeMemberMethod CreateMethodSignatureWithName(string name, bool callBase = true, params CodeParameterDeclarationExpression[] vars)
        {
            return CreateMethodSignatureWithName(name, null, callBase, vars);
        }

        public virtual CodeMemberMethod CreateMethodSignatureWithName(string name, CodeTypeReference returnType = null, bool callBase = true, params CodeParameterDeclarationExpression[] vars)
        {
            return CreateMethodSignatureWithName(name, returnType, (MemberAttributes) 0, callBase, vars);
        }

        public virtual CodeMemberMethod CreateMethodSignatureWithName(string name, CodeTypeReference returnType = null, MemberAttributes attribute = MemberAttributes.Public, bool callBase = true, params CodeParameterDeclarationExpression[] vars)
        {
            var createHandlerMethod = new CodeMemberMethod()
            {
                Attributes = MemberAttributes.Public,
                Name = name,
                ReturnType = returnType ?? new CodeTypeReference(typeof(void))
            };
            createHandlerMethod.Comments.Add(new CodeCommentStatement(Description, true));
            foreach (var item in vars)
            {
                createHandlerMethod.Parameters.Add(item);
            }
            if (IsOverride)
            {
                createHandlerMethod.Attributes |= MemberAttributes.Override;
                var baseInvoker = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(),
                    createHandlerMethod.Name);
                foreach (var item in vars)
                {
                    baseInvoker.Parameters.Add(new CodeVariableReferenceExpression(item.Name));
                }
                if (returnType != null)
                {
                    createHandlerMethod.Statements.Add(new CodeMethodReturnStatement(baseInvoker));
                }
                else
                {
                    createHandlerMethod.Statements.Add(baseInvoker);
                }
            }
            return createHandlerMethod;
        }
    }
}