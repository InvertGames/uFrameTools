using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellChildItemTypeNode : GenericInheritableNode, IShellReferenceType {
    public IShellReferenceType ReferenceType
    {
        get { return GetConnectionReference<ReferenceItemType>().InputFrom<IShellReferenceType>(); }
    }

    public bool IsCustom { get { return this["Custom"]; } }

    public string ClassName
    {
        get { return this.Name + "ChildItem"; }
    }
    public IEnumerable<IReferenceNode> IncludedInSections
    {
        get
        {
            return Project.NodeItems.OfType<IReferenceNode>().Where(p => p.AcceptableTypes.Any(x => x.SourceItem == this));
        }
    }
}