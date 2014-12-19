using Invert.Core.GraphDesigner;

[TemplateClass("ChildItems", MemberGeneratorLocation.Both, ClassNameFormat = "{0}ChildItem", IsEditorExtension = true)]
public class ShellChildItemTemplate : GenericNodeChildItem,
    IClassTemplate<ShellNodeChildTypeNode>
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        if (Ctx.IsDesignerFile)
        {
            if (Ctx.Data["Typed"])
                Ctx.SetBaseType(typeof(GenericTypedChildItem));

            foreach (var item in Ctx.Data.IncludedInSections)
            {
                Ctx.AddInterface(item.ReferenceClassName);
            }
        }

    }

    public TemplateContext<ShellNodeChildTypeNode> Ctx { get; set; }
}