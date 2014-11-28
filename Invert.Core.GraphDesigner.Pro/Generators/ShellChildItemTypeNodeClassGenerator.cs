using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;

public class ShellChildItemTypeNodeClassGenerator : GenericNodeGenerator<ShellChildItemTypeNode>
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        if (IsDesignerFile)
        {
            foreach (var item in Data.IncludedInSections)
            {
                Decleration.BaseTypes.Add(item.ReferenceClassName);
            }
        }
    }
}