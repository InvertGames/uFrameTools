public class ShellGeneratorTypeNode : GenericInheritableNode
{
    public ShellNodeGeneratorsSlot ShellNodeShellNodeGeneratorsSlot
    {
        get
        {
            return this.InputFrom<ShellNodeGeneratorsSlot>();
        }
    }
    public ShellNodeTypeNode GeneratorFor
    {
        get
        {
            var channel = ShellNodeShellNodeGeneratorsSlot;
            if (channel == null) return null;
            return channel.Node as ShellNodeTypeNode;
        }
    }
}