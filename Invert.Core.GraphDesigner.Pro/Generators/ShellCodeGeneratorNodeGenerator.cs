using System;
using System.CodeDom;
using System.Linq;
using Invert.uFrame.Editor;

public class ShellCodeGeneratorNodeGenerator : GenericNodeGenerator<ShellGeneratorTypeNode>
{
    protected override void InitializeDesignerFile()
    {
        base.InitializeDesignerFile();

        var method = new CodeMemberMethod()
        {
            Name = "InitializeDesignerFile",
            Attributes = MemberAttributes.Override | MemberAttributes.Family
        };

        var memberGeneratorReferences = Data.ChildItems.OfType<ShellMemberGeneratorSectionReference>();
        foreach (var memberGeneratorReference in memberGeneratorReferences)
        {
            var itemReference = memberGeneratorReference.SourceItem;
            var itemNode = itemReference.SourceItemObject as IShellReferenceType;
            if (itemNode == null) continue;
            var generatorNode = memberGeneratorReference.OutputTo<ShellMemberGeneratorInputSlot>().Node as ShellMemberGeneratorNode;
            if (generatorNode == null)
            {
                throw new Exception(string.Format("{0} on {1} Node is not connected to a generator", memberGeneratorReference.Name, memberGeneratorReference.Node.Name));
            }

            var reference = generatorNode.CreateGeneratorExpression(method.Statements);
            method.Statements.Add(
                new CodeSnippetExpression(string.Format("AddMembers(_=>_.{0}, {1})", itemReference.Name,
                    reference.VariableName)));

        }
        Decleration.Members.Add(method);
    }
}