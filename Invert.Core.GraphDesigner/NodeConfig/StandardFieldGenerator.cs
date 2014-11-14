using System.CodeDom;

namespace Invert.uFrame.Editor
{
    public class StandardFieldGenerator : MemberGenerator<ITypedItem>
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
                Type = Data.GetPropertyType(),
                CustomAttributes = CustomAttributes,
                Name = string.Format(FormatString, Data.Name)
            };
            return field;
        }
    }
}