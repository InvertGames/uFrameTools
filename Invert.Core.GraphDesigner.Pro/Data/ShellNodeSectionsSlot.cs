using Invert.Core.GraphDesigner;

public class ShellNodeSectionsSlot : GenericReferenceItem<ShellNodeTypeSection>, IShellNodeItem
{
    [JsonProperty, InspectorProperty]
    public int Order { get; set; }


    public string ReferenceClassName
    {
        get { return SourceItem.ReferenceClassName; }
    }

    public override void NodeRemoved(IDiagramNode nodeData)
    {
        base.NodeRemoved(nodeData);
        if (SourceItem == nodeData)
        {
            this.Node.Project.RemoveItem(this);
        }
    }
}