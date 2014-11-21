using System;
using System.CodeDom;
using System.Collections.Generic;
using Invert.Core;

namespace Invert.uFrame.Editor
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

    public class GenericEnumCodeGenerator<TData, TItem> : CodeGenerator
        where TData : IDiagramNodeItem
        where TItem : IDiagramNodeItem
    {
        public TData Data { get; set; }
        public Func<TData, IEnumerable<TItem>> Selector { get; set; }
        public override void Initialize(CodeFileGenerator fileGenerator)
        {
            base.Initialize(fileGenerator);
            //if (IsDesignerFile)
            //{
            UnityEngine.Debug.Log("HERE");
                var enumDecleration = new CodeTypeDeclaration(Data.Name) { IsEnum = true };
                foreach (var item in Selector(Data))
                {
                    enumDecleration.Members.Add(new CodeMemberField(enumDecleration.Name, item.Name));
                }
                Namespace.Types.Add(enumDecleration);
            //}
            
        }
    }
}