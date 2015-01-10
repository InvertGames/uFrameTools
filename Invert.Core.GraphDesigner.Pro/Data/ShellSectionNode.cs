using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellSectionNode : ShellNodeTypeSection, IShellConnectable
{
    [InputSlot("Reference Type")]
    public ShellSectionReferenceSlot ReferenceSlot { get; set; }

    public IShellNode ReferenceType
    {
        get
        {
            if (ReferenceSlot == null) return null;
            return ReferenceSlot.Item;
        }
    }
    private bool _allowMultipleInputs = true;
    private bool _allowMultipleOutputs = true;

    [JsonProperty, InspectorProperty]
    public bool AllowMultipleInputs
    {
        get { return _allowMultipleInputs; }
        set { _allowMultipleInputs = value; }
    }

    [JsonProperty, InspectorProperty]
    public bool AllowMultipleOutputs
    {
        get { return _allowMultipleOutputs; }
        set { _allowMultipleOutputs = value; }
    }
    public override string ReferenceClassName
    {
        get
        {
            if (ReferenceType == null) return null;
            return ReferenceType.ClassName;
        }
    }

    string IClassTypeNode.ClassName
    {
        get { return ReferenceClassName; }
    }

    public override string ClassName
    {
        get { return ReferenceClassName; }
    }

    [ReferenceSection("Connectable To", SectionVisibility.Always, false)]
    public IEnumerable<ShellConnectableReferenceType> ConnectableTo
    {
        get { return ChildItems.OfType<ShellConnectableReferenceType>(); }
    }

    public IEnumerable<IShellNode> PossibleConnectableTo
    {
        get { return Project.NodeItems.OfType<IShellNode>(); }
    }

}