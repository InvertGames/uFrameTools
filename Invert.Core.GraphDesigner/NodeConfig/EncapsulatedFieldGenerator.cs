using System.CodeDom;

namespace Invert.uFrame.Editor
{
    public class EncapsulatedFieldGenerator : MemberGenerator<ITypedItem>
    {
        public EncapsulatedFieldGenerator(string backingFieldFormatString)
        {
            _backingFieldFormatString = backingFieldFormatString;
        }
        public EncapsulatedFieldGenerator(string backingFieldFormatString, MemberAttributes attributes)
        {
            _backingFieldFormatString = backingFieldFormatString;
            _attributes = attributes;
        }
        private string _backingFieldFormatString = "_{0}";
        private bool _allowSet = true;
        private string _propertyFormatString = "{0}";

        public string BackingFieldFormatString
        {
            get { return _backingFieldFormatString; }
            set { _backingFieldFormatString = value; }
        }

        public string PropertyFormatString
        {
            get { return _propertyFormatString; }
            set { _propertyFormatString = value; }
        }

        protected virtual CodeTypeReference GetFieldType()
        {
            return Data.GetPropertyType();
        }
        protected virtual CodeTypeReference GetPropertyType()
        {
            return Data.GetPropertyType();
        }
        public override CodeTypeMember Create(bool isDesignerFile)
        {
            var field = new CodeMemberField()
            {
                Attributes = Attributes,
                Type = GetFieldType(),
                CustomAttributes = CustomAttributes,
                Name = string.Format(BackingFieldFormatString, Data.Name)
            };
            Decleration.Members.Add(field);
            var property = new CodeMemberProperty()
            {
                Attributes = MemberAttributes.Public,
                Type = GetPropertyType(),
                Name = Data.Name
            };
            property.GetStatements.Add(new CodeSnippetExpression(string.Format("return {0}", field.Name)));
            if (AllowSet)
                property.SetStatements.Add(new CodeSnippetExpression(string.Format("{0} = value", field.Name)));
            return property;
        }

        public EncapsulatedFieldGenerator(string backingFieldFormatString, bool allowSet)
        {
            _backingFieldFormatString = backingFieldFormatString;
            AllowSet = allowSet;
        }

        public bool AllowSet
        {
            get { return _allowSet; }
            set { _allowSet = value; }
        }
    }
}