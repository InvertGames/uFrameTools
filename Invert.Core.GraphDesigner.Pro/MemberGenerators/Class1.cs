using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner.Pro
{
    [SingleMemberGenerator]
    public class MethodOverrideGenerator : ShellMemberGeneratorNode
    {

        [GeneratorProperty]
        public string TypeName { get; set; }

        [JsonProperty, NodeProperty]
        public bool InvokeBase { get; set; }

        public override CodeTypeMember Create(CodeTypeDeclaration decleration, object data, bool isDesignerFile)
        {
            var type = InvertApplication.FindTypeByName(TypeName);
            var method = type.MethodFromTypeMethod(this.Name);
            if (InvokeBase)
            {
                method.invoke_base();
            }
            decleration.Members.Add(method);
        
            return method;
        }

        public override CodeVariableReferenceExpression CreateGeneratorExpression(CodeTypeDeclaration decleration, CodeStatementCollection collection)
        {
            var result = base.CreateGeneratorExpression(decleration, collection);
            TypeName = this.ForInputSlot.InputFrom<ShellGeneratorTypeNode>().BaseType.Name;
            var fillMethod = decleration.public_virtual_func("void", "Fill" + this.Name, "CodeMemberMethod","method");
            collection._("this.{0}(method)",fillMethod.Name);
            return result;
        }
    }

    public class MemberSnippetGenerator : ShellMemberGeneratorNode
    {
        [JsonProperty,InspectorProperty(InspectorType.TextArea)]
        public string Snippet { get; set; }

        public override CodeTypeMember Create(CodeTypeDeclaration decleration, object data, bool isDesignerFile)
        {
            
            return new CodeSnippetTypeMember(Snippet);
        }
    }
}
