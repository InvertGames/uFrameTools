using Invert.Core.GraphDesigner;

public class ShellNodeOutputsSlot : GenericReferenceItem<ShellSlotTypeNode>, IShellNodeItem
{
    public string ReferenceClassName
    {
        get { return SourceItem.ReferenceClassName; }
    }
}