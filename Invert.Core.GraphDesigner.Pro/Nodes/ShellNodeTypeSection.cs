using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellNodeTypeSection : ShellNode, IShellNode
{
    [JsonProperty]
    public bool AllowAdding { get; set; }



    public override string ClassName
    {
        get { return this.Name + "Reference"; }
    }
    [JsonProperty, InspectorProperty]
    public SectionVisibility Visibility { get; set; }

    public virtual string ReferenceClassName
    {
        get { return "I" + this.Name; }
    }
}
public class  ShellSectionReferenceSlot : SingleInputSlot<ShellChildItemTypeNode>
{
    public override bool Validate(IDiagramNodeItem a, IDiagramNodeItem b)
    {
        return true;
        return base.Validate(a, b);
    }

    public override bool ValidateInput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
    {
        return true;
        return base.ValidateInput(arg1, arg2);
    }

    public override bool ValidateOutput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
    {
        return true;
        return base.ValidateOutput(arg1, arg2);
    }
}
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