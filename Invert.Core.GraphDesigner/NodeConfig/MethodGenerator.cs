using System.CodeDom;

namespace Invert.Core.GraphDesigner
{
    public abstract class ShellMemberGeneratorNode<TGeneratorFor> : ShellMemberGeneratorNode
    {
        public override CodeTypeMember Create(CodeTypeDeclaration decleration, object data, bool isDesignerFile)
        {
            return Create(decleration, (TGeneratorFor) data, isDesignerFile);
        }

        public abstract CodeTypeMember Create(CodeTypeDeclaration d, TGeneratorFor data, bool isDesignerFile);
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

        public override CodeTypeMember Create(CodeTypeDeclaration decleration, IDiagramNodeItem data,
            bool isDesignerFile)
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

    public class ViewModelPropertyGenerator : ShellMemberGeneratorNode<ITypedItem>
    {
        private string _genericTypeFormat = "P<{0}>";
        private string _privateFieldFormat = "_{0}Property";
        private string _publicFormat = "{0}Property";
        private string _valuePropertyFormat = "{0}";

        [JsonProperty, NodeProperty]
        public string ValuePropertyFormat
        {
            get { return _valuePropertyFormat; }
            set { _valuePropertyFormat = value; }
        }

        [JsonProperty, NodeProperty]
        public string PublicFormat
        {
            get { return _publicFormat; }
            set { _publicFormat = value; }
        }

        [JsonProperty, NodeProperty]
        public string PrivateFieldFormat
        {
            get { return _privateFieldFormat; }
            set { _privateFieldFormat = value; }
        }
           [JsonProperty, NodeProperty]
        public string GenericTypeFormat
        {
            get { return _genericTypeFormat; }
            set { _genericTypeFormat = value; }
        }

        public override CodeTypeMember Create(CodeTypeDeclaration d, ITypedItem data, bool isDesignerFile)
        {
            var propertyType = string.Format(GenericTypeFormat, data.RelatedTypeName);

            // Field
            var field = 
                d._private_(propertyType, PrivateFieldFormat, data.Name);

            // Property
            d.public_(data.RelatedType, PublicFormat, data.Name)
                ._get( "return {0}", field.Name);

            d.public_(propertyType, data.Name)
                ._get("return {0}.Value", field.Name)
                ._set("{0}.Value = value", field.Name);

            return null;
        }
    }

    
}