using System.CodeDom;

namespace Invert.uFrame.Editor
{
    public class EncapsulatedFieldGenerator : MemberItemGenerator<ITypedItem>
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

        public string BackingFieldFormatString
        {
            get { return _backingFieldFormatString; }
            set { _backingFieldFormatString = value; }
        }

        protected virtual CodeTypeReference GetFieldType()
        {
            return Item.GetPropertyType();
        }
        protected virtual CodeTypeReference GetPropertyType()
        {
            return Item.GetPropertyType();
        }
        public override CodeTypeMember Create(bool isDesignerFile)
        {
            var field = new CodeMemberField()
            {
                Attributes = Attributes,
                Type = Item.GetPropertyType(),
                CustomAttributes = CustomAttributes,
                Name = string.Format(BackingFieldFormatString, Item.Name)
            };
            Decleration.Members.Add(field);
            var property = new CodeMemberProperty()
            {
                Attributes = MemberAttributes.Public,
                Type = Item.GetPropertyType(),
                Name = Item.Name
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