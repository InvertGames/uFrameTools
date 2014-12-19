using Invert.Core.GraphDesigner;

public class ShellNodeOutputsSlot : GenericReferenceItem<ShellSlotTypeNode>, IShellNodeItem
{
    [JsonProperty, InspectorProperty]
    public int Order { get; set; }

    public string ReferenceClassName
    {
        get { return SourceItem.ReferenceClassName; }
    }
}