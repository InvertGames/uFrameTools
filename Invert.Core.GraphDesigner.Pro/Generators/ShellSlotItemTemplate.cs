using System.CodeDom;
using System.IO;
using Invert.Core.GraphDesigner;

[TemplateClass(MemberGeneratorLocation.Both, ClassNameFormat = "{0}")]
public class ShellSlotItemTemplate : GenericSlot, IClassTemplate<IShellSlotType>
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

    public string OutputPath
    {
        get { return Path.Combine("Editor", "Slots"); }
    }

    public bool CanGenerate
    {
        get { return true; }
    }

    public void TemplateSetup()
    {
        Ctx.TryAddNamespace("Invert.Core.GraphDesigner");
        var i = new CodeTypeDeclaration(Ctx.Data.ReferenceClassName)
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

    public TemplateContext<IShellSlotType> Ctx { get; set; }
}