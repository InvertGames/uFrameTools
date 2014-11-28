using System;
using System.CodeDom;
using System.Linq;
using Invert.Core.GraphDesigner;
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
     
        AddOverrideFillMethods(method);

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

            var reference = generatorNode.CreateGeneratorExpression(Decleration, method.Statements);
            method.Add("AddMembers(_=>_.{0}, {1})", itemReference.Name, reference.VariableName);

        }

        foreach (var item in Data.OutputsTo<ShellMemberGeneratorInputSlot>().Select(p=>p.Node as ShellMemberGeneratorNode))
        {
            var reference = item.CreateGeneratorExpression(Decleration, method.Statements);
            method.Add("AddMember({0})", reference.VariableName);
        }

        Decleration.Members.Add(method);
    }

    protected override void InitializeEditableFile()
    {
        base.InitializeEditableFile();
        AddOverrideFillMethods(null);
    }

    private void AddOverrideFillMethods(CodeMemberMethod method)
    {
        foreach (var item in Data.ChildItems.OfType<TemplatePropertyReference>())
        {
            //MethodFromTypeMethod

            var fillMethod = new CodeMemberMethod()
            {
                Name = "Fill" + item.Name,
                Attributes = MemberAttributes.Family,
            };
            fillMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof (CodeMemberMethod), "method"));
          
            if (!IsDesignerFile)
            {
                fillMethod.Attributes |= MemberAttributes.Override;
                fillMethod.Add("base.{0}(method)", fillMethod.Name);
            } else
            {
                method.Add("var {1}Method = typeof({0}).MethodFromTypeMethod(\"{1}\")", Data.BaseType.FullName, item.Name);
                method.Add("{0}({1}Method)", fillMethod.Name, item.Name);
                method.Add("Decleration.Members.Add({0}Method)", item.Name);

            }
            Decleration.Members.Add(fillMethod);
        }
    }
}