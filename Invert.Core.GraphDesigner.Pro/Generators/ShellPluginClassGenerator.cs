using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;

public class ShellPluginClassGenerator : GenericNodeGenerator<ShellPluginNode>
{
    public override void Initialize(CodeFileGenerator fileGenerator)
    {
        base.Initialize(fileGenerator);
        TryAddNamespace("System.IO");

    }
}