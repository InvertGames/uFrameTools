using System.CodeDom;
using Invert.Core.GraphDesigner;

[TemplateClass("Slots", MemberGeneratorLocation.Both, ClassNameFormat = "{0}", IsEditorExtension = true)]
public class ShellSlotItemTemplate : GenericSlot, IClassTemplate<ShellSlotTypeNode>
{
    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        var i = new CodeTypeDeclaration("I" + Ctx.Data.Name)
        {
            IsInterface = true,
            Attributes = MemberAttributes.Public,
            IsPartial = true,
        };
        i.BaseTypes.Add(new CodeTypeReference(typeof(IDiagramNodeItem)));
        i.BaseTypes.Add(new CodeTypeReference(typeof(IConnectable)));
        Ctx.Namespace.Types.Add(i);

        if (Ctx.IsDesignerFile)
        {
            if (Ctx.Data.IsOutput)
            {
                if (Ctx.Data.AllowMultiple)
                {
                    Ctx.SetBaseType("MultiOutputSlot<{0}>", Ctx.Data.ReferenceClassName);
                }
                else
                {
                    Ctx.SetBaseType("SingleOutputSlot<{0}>", Ctx.Data.ReferenceClassName);
                }
            }
            else
            {
                if (Ctx.Data.AllowMultiple)
                {
                    Ctx.SetBaseType("MultiInputSlot<{0}>", Ctx.Data.ReferenceClassName);
                }
                else
                {
                    Ctx.SetBaseType("SingleInputSlot<{0}>", Ctx.Data.ReferenceClassName);
                }
            }

            foreach (var item in Ctx.Data.IncludedInSections)
            {
                Ctx.AddInterface(item.ReferenceClassName);
            }
        }
      

    }

    public TemplateContext<ShellSlotTypeNode> Ctx { get; set; }
}