using System.Collections.Generic;

public interface IInhertable : IDiagramNode
{
    string BaseIdentifier { get; }

    void SetBaseType(GenericInheritableNode baseItem);

    GenericInheritableNode BaseNode { get; }

    IEnumerable<GenericInheritableNode> BaseNodes { get; }
    IEnumerable<GenericInheritableNode> DerivedNodes { get; }
}