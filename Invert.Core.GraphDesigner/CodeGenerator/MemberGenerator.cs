using System.CodeDom;

namespace Invert.uFrame.Editor
{
    public abstract class MemberGenerator<TNode> : IMemberGenerator
    {
        protected MemberAttributes _attributes = MemberAttributes.Private;
        private CodeAttributeDeclarationCollection _customAttributes;

        public CodeTypeDeclaration Decleration { get; set; }
        public MemberGeneratorLocation Location { get; set; }
        public object DataObject { get; set; }
        public abstract CodeTypeMember Create(bool isDesignerFile);

        public TNode Data
        {
            get { return (TNode) DataObject; }
        }

        public MemberAttributes Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }

        public CodeAttributeDeclarationCollection CustomAttributes
        {
            get { return _customAttributes ?? (_customAttributes = new CodeAttributeDeclarationCollection()); }
            set { _customAttributes = value; }
        }
    }
}