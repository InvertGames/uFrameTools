using System.CodeDom;

namespace Invert.Core.GraphDesigner
{
    public abstract class ShellMemberGeneratorNode<TGeneratorFor> : ShellMemberGeneratorNode
    {
        public override CodeTypeMember Create(CodeTypeDeclaration decleration, object data, bool isDesignerFile)
        {
            return Create(decleration, (TGeneratorFor)data, isDesignerFile);
        }

        public abstract CodeTypeMember Create(CodeTypeDeclaration decleration, TGeneratorFor data, bool isDesignerFile);
    }

    public class MethodGenerator : ShellMemberGeneratorNode<IDiagramNodeItem>
    {
        private string _methodNameFormat = "{0}";

        [JsonProperty, NodeProperty]
        public string MethodNameFormat
        {
            get { return _methodNameFormat ?? "{0}"; }
            set { _methodNameFormat = value; }
        }

        [JsonProperty, NodeProperty]
        public bool AlwaysOverride { get; set; }

        public override CodeTypeMember Create(CodeTypeDeclaration decleration, IDiagramNodeItem data, bool isDesignerFile)
        {
            var method = new CodeMemberMethod()
            {
                Name = string.Format(MethodNameFormat, data.Name),
                
            };
            if (AlwaysOverride || (!isDesignerFile && MemberLocation == MemberGeneratorLocation.Both))
            {
                method.Attributes |= MemberAttributes.Override;
            }

            return method;
        }
    }
}