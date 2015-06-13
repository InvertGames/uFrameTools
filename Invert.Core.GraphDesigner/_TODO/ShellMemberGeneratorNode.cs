using System;
using System.CodeDom;

namespace Invert.Core.GraphDesigner
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SingleMemberGenerator : Attribute
    {
        
    }
    public class ShellMemberGeneratorInputSlot : GenericSlot
    {

    }
    public abstract class ShellMemberGeneratorNode : GenericNode, IMemberGenerator
    {
        private TemplateLocation _memberLocation;
        
        [NodeProperty]
        public TemplateLocation MemberLocation
        {
            get { return _memberLocation; }
            set { _memberLocation = value; }
        }

        public ShellMemberGeneratorInputSlot ForInputSlot
        {
            get { return GetConnectionReference<ShellMemberGeneratorInputSlot>(); }
        }
        
        public abstract CodeTypeMember Create(CodeTypeDeclaration decleration, object data, bool isDesignerFile);

        public virtual CodeVariableReferenceExpression CreateGeneratorExpression(CodeTypeDeclaration decleration, CodeStatementCollection collection)
        {
            var properties = NodeFor.GetPropertiesByAttribute<GeneratorProperty>();
            var expression = new CodeObjectCreateExpression(NodeFor);
            var variable = new CodeVariableDeclarationStatement(NodeFor, this.Name, expression);
            var variableReference = new CodeVariableReferenceExpression(variable.Name);
            collection.Add(variable);
            foreach (var property in properties)
            {
                if (property.PropertyType.IsEnum)
                {
                    var statement = new CodeAssignStatement(new CodePropertyReferenceExpression(variableReference, property.Name),
                        new CodeSnippetExpression(string.Format("{0}.{1}", property.PropertyType.Name, property.GetValue(this, null).ToString()))
                        );
                    collection.Add(statement);
                }
                else
                {
                    var statement = new CodeAssignStatement(new CodePropertyReferenceExpression(variableReference, property.Name),
                        new CodePrimitiveExpression(property.GetValue(this, null))
                        );
                    collection.Add(statement);
                }
            }
            return variableReference;
        }

        public virtual Type NodeFor
        {
            get { return this.GetType(); }
        }

        [JsonProperty]
        public string MemberLocationString
        {
            get { return MemberLocation.ToString(); }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    MemberLocation = TemplateLocation.DesignerFile;
                }
                MemberLocation = (TemplateLocation)Enum.Parse(typeof(TemplateLocation), value);
            }
        }
    }
}