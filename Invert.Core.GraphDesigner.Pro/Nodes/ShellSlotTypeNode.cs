using Invert.Core.GraphDesigner;

public class ShellSlotTypeNode : GenericInheritableNode, IShellReferenceType
{
    public string ClassName
    {
        get { return this.Name; }
    }
    public IShellReferenceType ReferenceType
    {
        get { return GetConnectionReference<ReferenceItemType>().InputFrom<IShellReferenceType>(); }
    }

    public bool IsCustom { get { return this["Custom"]; } }
}