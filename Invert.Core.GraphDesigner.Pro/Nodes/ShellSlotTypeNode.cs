using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public interface IReferenceNode : IShellReferenceType
{
    string ReferenceClassName { get; }
    [ReferenceSection("Acceptable Types", SectionVisibility.Always, false)]
    IEnumerable<ShellAcceptableReferenceType> AcceptableTypes { get; }

    IEnumerable<IShellReferenceType> PossibleAcceptableTypes { get; }
}
public class ShellSlotTypeNode : GenericInheritableNode, IShellReferenceType, IReferenceNode
{
    public string ClassName
    {
        get
        {
            return this.Name;
        }
    }
    [JsonProperty, NodeProperty]
    public SectionVisibility Visibility { get; set; }

    [ReferenceSection("Acceptable Types",SectionVisibility.Always, false)]
    public IEnumerable<ShellAcceptableReferenceType> AcceptableTypes
    {
        get { return ChildItems.OfType<ShellAcceptableReferenceType>(); }
    }

    public IEnumerable<IShellReferenceType> PossibleAcceptableTypes
    {
        get { return Project.NodeItems.OfType<IShellReferenceType>(); }
    }

    public IShellReferenceType ReferenceType
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

    public bool IsCustom
    {
        get { return this["Custom"]; }
        set { this["Custom"] = value; }
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

    [JsonProperty,InspectorProperty]
    public bool IsOutput
    {
        get { return this["Output"]; }
        set { this["Output"] = value; }
    }
}

public class ShellAcceptableReferenceType : GenericReferenceItem<IShellReferenceType>
{
    
}