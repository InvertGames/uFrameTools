using Invert.Core.GraphDesigner;

public class ShellNodeInputsSlot : GenericReferenceItem<ShellSlotTypeNode>,IShellNodeItem
{
    public string ReferenceClassName
    {
        get { return SourceItem.ReferenceClassName; }
    }
}