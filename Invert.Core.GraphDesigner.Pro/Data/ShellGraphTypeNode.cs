using Invert.Core.GraphDesigner;

public class ShellGraphTypeNode : ShellNode
{
    [OutputSlot("Root Node")]
    public SingleOutputSlot<ShellNodeTypeNode> RootNodeSlot { get; set; }

    public ShellNodeTypeNode RootNode
    {
        get
        {
            return RootNodeSlot.Item;
        }
    }

    public override string ClassName
    {
        get { return string.Format("{0}Graph", Name); }
    }
}