using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellNodeTypeReferenceSection : ShellNodeTypeSection,IReferenceNode
{
    [JsonProperty, NodeProperty]
    public bool IsAutomatic { get; set; }

    [JsonProperty,NodeProperty]
    public bool AllowDuplicates { get; set; }

    [ReferenceSection("Acceptable Types", SectionVisibility.Always, false)]
    public IEnumerable<ShellAcceptableReferenceType> AcceptableTypes
    {
        get { return ChildItems.OfType<ShellAcceptableReferenceType>(); }
    }

    public IEnumerable<IShellReferenceType> PossibleAcceptableTypes
    {
        get { return Project.NodeItems.OfType<IShellReferenceType>(); }
    }
    public IEnumerable<IReferenceNode> IncludedInSections
    {
        get
        {
            return Project.NodeItems.OfType<IReferenceNode>().Where(p => p.AcceptableTypes.Any(x => x.SourceItem == this));
        }
    }

}