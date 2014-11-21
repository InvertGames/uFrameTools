using System.Collections.Generic;

public interface IInhertable : IDiagramNode
{

    GenericInheritableNode BaseNode { get; }

    IEnumerable<GenericInheritableNode> BaseNodes { get; }
    IEnumerable<GenericInheritableNode> DerivedNodes { get; }
}