using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Invert.uFrame.Editor
{
    public class MethodGenerator<TType> : MemberGenerator<TType> where TType : IDiagramNodeItem
    {
        private string _methodNameFormat = "{0}";

        public string MethodNameFormat
        {
            get { return _methodNameFormat ?? "{0}"; }
            set { _methodNameFormat = value; }
        }

        public bool AlwaysOverride { get; set; }

        public Func<TType, CodeTypeReference> ReturnTypeSelector { get; set; }
        public Func<TType, IEnumerable<CodeParameterDeclarationExpression>> ParametersSelecter { get; set; }
        public Func<TType, IEnumerable<CodeExpression>> BodySelector { get; set; }

        public override CodeTypeMember Create(bool isDesignerFile)
        {
            var method = new CodeMemberMethod()
            {
                Name = string.Format(MethodNameFormat, Data.Name),
                CustomAttributes = CustomAttributes
                
            };
            if (ReturnTypeSelector != null)
            {
                method.ReturnType = ReturnTypeSelector(Data);
            }
            method.Attributes = Attributes;
            if (AlwaysOverride || (!isDesignerFile && Location == MemberGeneratorLocation.Both))
            {
                method.Attributes |= MemberAttributes.Override;
            }
            if (ParametersSelecter != null)
            {
                var parameters = ParametersSelecter(Data);
                foreach (var item in parameters)
                {
                    method.Parameters.Add(item);
                }
            }
            if (BodySelector != null)
            {
                var body = BodySelector(Data);
                foreach (var item in body)
                {
                    method.Statements.Add(item);
                }
            }
            return method;
        }
    }

    public class GenericEnumCodeGenerator<TData, TItem> : CodeGenerator
        where TData : IDiagramNodeItem
        where TItem : IDiagramNodeItem
    {
        public TData Data { get; set; }
        public Func<TData, IEnumerable<TItem>> Selector { get; set; }
        public override void Initialize(CodeFileGenerator fileGenerator)
        {
            base.Initialize(fileGenerator);
            //if (IsDesignerFile)
            //{
            UnityEngine.Debug.Log("HERE");
                var enumDecleration = new CodeTypeDeclaration(Data.Name) { IsEnum = true };
                foreach (var item in Selector(Data))
                {
                    enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, item.Name));
                }
                Namespace.Types.Add(enumDecleration);
            //}
            
        }
    }
}