using System.CodeDom;

namespace Invert.uFrame.Editor
{
    public class StandardFieldGenerator : MemberItemGenerator<IBindableTypedItem>
    {
        public StandardFieldGenerator(string formatString)
        {
            _formatString = formatString;
        }
        public StandardFieldGenerator(string formatString, MemberAttributes attributes)
        {
            _formatString = formatString;
            _attributes = attributes;
        }
        private string _formatString = "_{0}";

        public string FormatString
        {
            get { return _formatString; }
            set { _formatString = value; }
        }

        public override CodeTypeMember Create(bool isDesignerFile)
        {
            var field = new CodeMemberField()
            {
                Attributes = Attributes,
                Type = Item.GetPropertyType(),
                CustomAttributes = CustomAttributes,
                Name = string.Format(FormatString, Item.Name)
            };
            return field;
        }
    }
}