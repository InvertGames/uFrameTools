using System.CodeDom;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;

public class ShellNodeTypeSectionClassGenerator : GenericNodeGenerator<ShellNodeTypeReferenceSection>
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        var i = new CodeTypeDeclaration("I" + Data.Name)
        {
            IsInterface = true,
            Attributes = MemberAttributes.Public,
            IsPartial = true,
        };
        i.BaseTypes.Add(new CodeTypeReference(typeof(IDiagramNodeItem)));
        i.BaseTypes.Add(new CodeTypeReference(typeof(IConnectable)));
        Namespace.Types.Add(i);
        if (IsDesignerFile)
        {
            foreach (var item in Data.IncludedInSections)
            {
                Decleration.BaseTypes.Add(item.ReferenceClassName);
            }
        }
    }

    protected override void InitializeDesignerFile()
    {
        base.InitializeDesignerFile();
     
    }

}