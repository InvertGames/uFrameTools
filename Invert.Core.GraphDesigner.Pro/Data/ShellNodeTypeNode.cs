using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellNodeTypeNode : ShellInheritableNode, IShellNode, IShellConnectable
{
    private string _classFormat = "{0}";
    private bool _allowMultipleOutputs;

    [JsonProperty, InspectorProperty]
    public bool MultipleInputs { get; set; }

    [JsonProperty, InspectorProperty]
    public bool MultipleOutputs
    {
        get
        { 
            if (this["Inheritable"])
            {
                return true;
            }
            return _allowMultipleOutputs;
        }
        set { _allowMultipleOutputs = value; }
    }

    public override bool ValidateInput(IDiagramNodeItem a, IDiagramNodeItem b)
    {
        return true;
        return base.ValidateInput(a, b);
    }

    public override bool ValidateOutput(IDiagramNodeItem a, IDiagramNodeItem b)
    {
        return true;
    }
    [Browsable(false)]
    public IShellNode ReferenceType
    {
        get { return GetConnectionReference<ReferenceItemType>().InputFrom<IShellNode>(); }
    }
    [InspectorProperty]
    public bool Inheritable
    {
        get
        {
            return this["Inheritable"];
        }
        set { this["Inheritable"] = value; }

    }

    //[InputSlot("Base Class")]
    //public SingleInputSlot<ShellNodeTypeNode> BaseSlot { get; set; }

    //[OutputSlot("Generators")]
    //public MultiOutputSlot<ShellGeneratorTypeNode> GeneratorsSlot { get; set; }
    [Browsable(false)]
    [OutputSlot("Sub Nodes")]
    public MultiOutputSlot<ShellNodeTypeNode> SubNodesSlot { get; set; }
    [Browsable(false)]
    public IEnumerable<ShellNodeTypeNode> SubNodes
    {
        get { return SubNodesSlot.Items; }
    }

    //public IEnumerable<ShellGeneratorTypeNode> Generators
    //{
    //    get
    //    {
    //        return GeneratorsSlot.Items;
    //    }
    //}

    [Browsable(false)]
    [ReferenceSection("Sections", SectionVisibility.WhenNodeIsFilter, false)]
    public IEnumerable<ShellNodeSectionsSlot> Sections
    {
        get { return ChildItems.OfType<ShellNodeSectionsSlot>(); }
    }
    [Browsable(false)]
    public IEnumerable<ShellNodeTypeSection> PossibleSections
    {
        get { return Project.NodeItems.OfType<ShellNodeTypeSection>(); }
    }
    [Browsable(false)]
    public IEnumerable<IReferenceNode> IncludedInSections
    {
        get
        {
            return Project.NodeItems.OfType<IReferenceNode>().Where(p => p.AcceptableTypes.Any(x => x.SourceItem == this));
        }
    }
    [Browsable(false)]
    public IEnumerable<ShellNodeTypeReferenceSection> ReferenceSections
    {
        get { return Sections.Select(p => p.SourceItem).OfType<ShellNodeTypeReferenceSection>(); }
    }
    [Browsable(false)]
    [ReferenceSection("Inputs", SectionVisibility.WhenNodeIsFilter, true)]
    public IEnumerable<ShellNodeInputsSlot> InputSlots
    {
        get
        {
            return ChildItems.OfType<ShellNodeInputsSlot>();
        }
    }
    [Browsable(false)]
    [ReferenceSection("Outputs", SectionVisibility.WhenNodeIsFilter, true)]
    public IEnumerable<ShellNodeOutputsSlot> OutputSlots
    {
        get
        {
            return ChildItems.OfType<ShellNodeOutputsSlot>();
        }
    }
    [Browsable(false)]
    public IEnumerable<ShellSlotTypeNode> PossibleInputSlots
    {
        get { return Project.NodeItems.OfType<ShellSlotTypeNode>().Where(p => !p.IsOutput); }
    }
    [Browsable(false)]
    public IEnumerable<ShellSlotTypeNode> PossibleOutputSlots
    {
        get { return Project.NodeItems.OfType<ShellSlotTypeNode>().Where(p => p.IsOutput); }
    }
        
    //[Section("Custom Selectors", SectionVisibility.WhenNodeIsFilter)]
    [Browsable(false)]
    public IEnumerable<ShellPropertySelectorItem> CustomSelectors
    {
        get
        {
            return ChildItems.OfType<ShellPropertySelectorItem>();
        }
    }

    public override string ClassName
    {
        get { return Name + "Node"; }
    }
    [Browsable(false)]
    [ReferenceSection("Connectable To", SectionVisibility.WhenNodeIsFilter, false)]
    public IEnumerable<ShellConnectableReferenceType> ConnectableTo
    {
        get { return ChildItems.OfType<ShellConnectableReferenceType>(); }
    }

    [Browsable(false)]
    public IEnumerable<IShellNode> PossibleConnectableTo
    {
        get { return Project.NodeItems.OfType<IShellNode>(); }
    }

}