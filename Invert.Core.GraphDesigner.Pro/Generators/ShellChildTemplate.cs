using Invert.Core.GraphDesigner;

[TemplateClass("Sections", MemberGeneratorLocation.Both, ClassNameFormat = "{0}ChildItem", IsEditorExtension = true)]
public class ShellChildTemplate : GenericNodeChildItem,
    IClassTemplate<ShellChildItemTypeNode>
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.IsDesignerFile)
        {
            if (Ctx.Data["Typed"] && Ctx.Data.BaseNode == null)
                Ctx.SetBaseType(typeof(GenericTypedChildItem));

            foreach (var item in Ctx.Data.IncludedInSections)
            {
                Ctx.AddInterface(item.ReferenceClassName);
            }
        }
        
    }

    public TemplateContext<ShellChildItemTypeNode> Ctx { get; set; }
}