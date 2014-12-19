using System.Collections.Generic;
using Invert.Core.GraphDesigner;

public interface IShellConnectable : IDiagramNode, IShellNode
{
    [ReferenceSection("Connectable To", SectionVisibility.Always, false)]
    IEnumerable<ShellConnectableReferenceType> ConnectableTo { get; }

    bool AllowMultipleInputs { get; set; }

    bool AllowMultipleOutputs { get; set; }
}