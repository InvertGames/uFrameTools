using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellNodeChildTypeNode : GenericNode
{
    public IEnumerable<IReferenceNode> IncludedInSections
    {
        get
        {
            return Project.NodeItems.OfType<IReferenceNode>().Where(p => p.AcceptableTypes.Any(x => x.SourceItem == this));
        }
    }

}