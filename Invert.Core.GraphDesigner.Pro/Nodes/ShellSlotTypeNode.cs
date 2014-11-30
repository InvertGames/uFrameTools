using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public interface IReferenceNode : IShellNode
{
    string ReferenceClassName { get; }
    [ReferenceSection("Acceptable Types", SectionVisibility.Always, false)]
    IEnumerable<ShellAcceptableReferenceType> AcceptableTypes { get; }
    IEnumerable<IShellNode> PossibleAcceptableTypes { get; }

}
public class ShellSlotTypeNode : ShellNode, IReferenceNode
{
    public override string ClassName
    {
        get
        {
            return this.Name;
        }
    }
    [JsonProperty, InspectorProperty]
    public SectionVisibility Visibility { get; set; }

    [ReferenceSection("Acceptable Types",SectionVisibility.Always, false)]
    public IEnumerable<ShellAcceptableReferenceType> AcceptableTypes
    {
        get { return ChildItems.OfType<ShellAcceptableReferenceType>(); }
    }

    public IEnumerable<IShellNode> PossibleAcceptableTypes
    {
        get { return Project.NodeItems.OfType<IShellNode>(); }
    }

    public IShellNode ReferenceType
    {
        get { return AcceptableTypes.Select(p => p.SourceItem).FirstOrDefault(); }
    }

    public IEnumerable<IReferenceNode> IncludedInSections
    {
        get
        {
            return Project.NodeItems.OfType<IReferenceNode>().Where(p => p.AcceptableTypes.Any(x => x.SourceItem == this));
        }
    }


    public string ReferenceClassName
    {
        get
        {
            return "I" + Name;
        }
    }

    [InspectorProperty]
    public bool AllowMultiple
    {
        get { return this["Multiple"]; }
        set { this["Multiple"] = value; }
    }

    [InspectorProperty]
    public bool IsOutput
    {
        get { return this["Output"]; }
        set { this["Output"] = value; }
    }
}

public class ShellAcceptableReferenceType : GenericReferenceItem<IShellNode>
{
    
}

public class ShellConnectableReferenceType : GenericReferenceItem<IShellNode>
{

}