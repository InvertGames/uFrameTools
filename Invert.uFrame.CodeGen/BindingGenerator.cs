using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
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
        public IViewModelItem Item { get; set; }

        public IDiagramNode RelatedNode
        {
            get { return Item.RelatedNode(); }
        }

        public ElementData RelatedElement
        {
            get { return RelatedNode as ElementData; }
        }
        public ElementData ElementData
        {
            get { return Item.Node as ElementData; }
        }

        public IElementDesignerData DiagramData
        {
            get { return RelatedElement.Data; }
        }
        public bool GenerateDefaultImplementation { get; set; }

        public CodeMemberField CreateBindingField(string typeFullName, string propertyName, string name, bool keepHidden = false)
        {
            var memberField =
                new CodeMemberField(
                    typeFullName,
                    "_" + propertyName + name) { Attributes = MemberAttributes.Public };
            if (!keepHidden)
            {
                memberField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(uFrameEditor.uFrameTypes.UFGroup),
                    new CodeAttributeArgument(new CodePrimitiveExpression(propertyName))));
            }

            memberField.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(HideInInspector))));
            return memberField;
        }

        public virtual string GetMethodName(IViewModelItem itemData)
        {
            return string.Empty;
        }

        public abstract string MethodName { get; }

        public abstract bool IsApplicable { get; }
        public bool IsOverride { get; set; }

        public virtual void CreateMembers(CodeTypeMemberCollection collection)
        {

        }

        public abstract void CreateBindingStatement(CodeTypeMemberCollection collection, CodeConditionStatement bindingCondition);

        public override string ToString()
        {
            var cp = new CSharpCodeProvider();
            var sb = new StringBuilder();
            var strWriter = new StringWriter(sb);
            var collection = new CodeTypeMemberCollection();
            CreateMembers(collection);

            // ------------------------------------------------------------------------------
            //  <autogenerated>
            //      This code was generated by a tool.
            //      Mono Runtime Version: 2.0.50727.1433
            // 
            //      Changes to this file may cause incorrect behavior and will be lost if 
            //      the code is regenerated.
            //  </autogenerated>
            // ------------------------------------------------------------------------------



            //public class DUMMY {

            //var ctm = member as CodeTypeMember;
            //if (ctm == null) continue;
            var type = new CodeTypeDeclaration("DUMMY");
            type.Members.AddRange(collection);
            var ccu = new CodeCompileUnit();
            var ns = new CodeNamespace();
            ns.Types.Add(type);
            ccu.Namespaces.Add(ns);

            cp.GenerateCodeFromCompileUnit(ccu, strWriter, new CodeGeneratorOptions());
            //cp.GenerateCodeFromMember(ctm,strWriter,new CodeGeneratorOptions()
            //{

            //});

            var adjusted = new[] { "\t\t// Comment out the base invoke to skip default bindings. "}.Concat(
                sb.ToString()
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                    .Skip(14)
                    .Reverse()
                    .Skip(2)
                    .Reverse()).ToArray();
            //if (!CallBase)
            //{
            //    adjusted[1] = "//" + adjusted[1];
            //}
            
            return string.Join("\r\n", adjusted);
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
            var createHandlerMethod = new CodeMemberMethod()
            {
                Attributes = MemberAttributes.Public,
                Name = MethodName,
                ReturnType = returnType ?? new CodeTypeReference(typeof(void))
            };
            foreach (var item in vars)
            {
                createHandlerMethod.Parameters.Add(item);
            }
            if (IsOverride)
            {
                createHandlerMethod.Attributes |= MemberAttributes.Override;

                //if (CallBase)
                //{
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
                //}
                
          

            }
            return createHandlerMethod;
        }
    }
}