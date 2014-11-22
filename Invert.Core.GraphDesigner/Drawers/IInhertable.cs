using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public interface IInhertable : IDiagramNode
    {

        GenericInheritableNode BaseNode { get; }

        IEnumerable<GenericInheritableNode> BaseNodes { get; }
        IEnumerable<GenericInheritableNode> DerivedNodes { get; }
    }
}