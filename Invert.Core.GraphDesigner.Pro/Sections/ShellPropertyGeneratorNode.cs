using System.CodeDom;
using Invert.uFrame.Editor;
using UnityEngine;

public class ShellPropertyGeneratorNode : ShellMemberGeneratorNode<ITypedItem>
{
    private string _fieldFormat = "_{0}";
    private string _propertyFormat = "{0}";

    [JsonProperty, NodeProperty]
    public string FieldFormat
    {
        get { return _fieldFormat; }
        set { _fieldFormat = value; }
    }

    [JsonProperty, NodeProperty]
    public string PropertyFormat
    {
        get { return _propertyFormat; }
        set { _propertyFormat = value; }
    }

    [JsonProperty, NodeProperty]
    public bool HasGet { get; set; }

    [JsonProperty, NodeProperty]
    public bool HasSet { get; set; }

    [JsonProperty, NodeProperty]
    public bool Override { get; set; }

    [JsonProperty, NodeProperty]
    public bool SerializeAttribute { get; set; }

    public override CodeTypeMember Create(CodeTypeDeclaration decleration, ITypedItem data, bool isDesignerFile)
    {
        var field = new CodeMemberField()
        {
            Name = FieldFormat.Contains("{0}") ? string.Format(FieldFormat, data.Name) : FieldFormat,
            Type = data.GetPropertyType()
        };
        var member = new CodeMemberProperty()
        {
            Name = PropertyFormat.Contains("{0}") ? string.Format(PropertyFormat, data.Name) : PropertyFormat,
            Type = data.GetPropertyType(),
            HasGet = HasGet,
            HasSet = HasSet,
        };
        if (SerializeAttribute)
        {
            field.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializeField))));
        }
        if (HasGet)
        {
            member.GetStatements.Add(new CodeSnippetExpression(string.Format("return {0}", field.Name)));
        }
        if (HasSet)
        {
            member.SetStatements.Add(new CodeSnippetExpression(string.Format("{0} = value", field.Name)));
        }
        if (Override)
        {
            member.Attributes |= MemberAttributes.Override;
        }

        decleration.Members.Add(field);
        return member;
    }


}