using System.CodeDom;
using Invert.Core.GraphDesigner;

[TemplateClass("Slots", MemberGeneratorLocation.Both, ClassNameFormat = "{0}", IsEditorExtension = true)]
public class ShellSlotItemTemplate : GenericSlot, IClassTemplate<ShellSlotTypeNode>
{
    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public override bool AllowMultipleInputs
    {
        get
        {
            Ctx._("return {0}", Ctx.Data.AllowMultipleInputs ? "true" : "false");
            return base.AllowMultipleInputs;
        }
    }
    [TemplateProperty(MemberGeneratorLocation.DesignerFile)]
    public override bool AllowMultipleOutputs
    {
        get
        {
            Ctx._("return {0}", Ctx.Data.AllowMultipleOutputs ? "true" : "false");
            return base.AllowMultipleOutputs;
        }
    }

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