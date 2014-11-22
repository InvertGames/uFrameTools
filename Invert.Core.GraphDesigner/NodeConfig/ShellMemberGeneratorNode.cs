using System;
using System.CodeDom;

namespace Invert.Core.GraphDesigner
{
    public abstract class ShellMemberGeneratorNode : GenericNode, IMemberGenerator
    {
        private MemberGeneratorLocation _memberLocation;

        [NodeProperty]
        public MemberGeneratorLocation MemberLocation
        {
            get { return _memberLocation; }
            set { _memberLocation = value; }
        }

        public abstract CodeTypeMember Create(CodeTypeDeclaration decleration, object data, bool isDesignerFile);

        public CodeVariableReferenceExpression CreateGeneratorExpression(CodeStatementCollection collection)
        {
            var properties = NodeFor.GetPropertiesByAttribute<NodeProperty>();
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
                    MemberLocation = MemberGeneratorLocation.DesignerFile;
                }
                MemberLocation = (MemberGeneratorLocation)Enum.Parse(typeof(MemberGeneratorLocation), value);
            }
        }
    }
}