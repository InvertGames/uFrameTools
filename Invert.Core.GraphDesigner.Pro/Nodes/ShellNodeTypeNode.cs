using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellNodeTypeNode : GenericInheritableNode, IShellReferenceType
{
    private string _classFormat = "{0}";

    public IShellReferenceType ReferenceType
    {
        get { return GetConnectionReference<ReferenceItemType>().InputFrom<IShellReferenceType>(); }
    }

    [InspectorProperty]
    public bool IsCustom
    {
        get
        {
            return this["Custom"];
        }
        set { this["Custom"] = value; }
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

    [OutputSlot("Generators")]
    public MultiOutputSlot<ShellGeneratorTypeNode> GeneratorsSlot { get; set; }

    [OutputSlot("Sub Nodes")]
    public MultiOutputSlot<ShellNodeTypeNode> SubNodesSlot { get; set; }

    public IEnumerable<ShellNodeTypeNode> SubNodes
    {
        get { return SubNodesSlot.Items; }
    }

    public IEnumerable<ShellGeneratorTypeNode> Generators
    {
        get
        {
            return GeneratorsSlot.Items;
        }
    }


    [ReferenceSection("Sections", SectionVisibility.WhenNodeIsFilter, false)]
    public IEnumerable<ShellNodeSectionsSlot> Sections
    {
        get { return ChildItems.OfType<ShellNodeSectionsSlot>(); }
    }

    public IEnumerable<ShellNodeTypeSection> PossibleSections
    {
        get { return Project.NodeItems.OfType<ShellNodeTypeSection>(); }
    }

    public IEnumerable<IReferenceNode> IncludedInSections
    {
        get
        {
            return Project.NodeItems.OfType<IReferenceNode>().Where(p => p.AcceptableTypes.Any(x => x.SourceItem == this));
        }
    }

    public IEnumerable<ShellNodeTypeReferenceSection> ReferenceSections
    {
        get { return Sections.Select(p => p.SourceItem).OfType<ShellNodeTypeReferenceSection>(); }
    }

    [ReferenceSection("Inputs", SectionVisibility.WhenNodeIsFilter, true)]
    public IEnumerable<ShellNodeInputsSlot> InputSlots
    {
        get
        {
            return ChildItems.OfType<ShellNodeInputsSlot>();
        }
    }

    [ReferenceSection("Outputs", SectionVisibility.WhenNodeIsFilter, true)]
    public IEnumerable<ShellNodeOutputsSlot> OutputSlots
    {
        get
        {
            return ChildItems.OfType<ShellNodeOutputsSlot>();
        }
    }
    public IEnumerable<ShellSlotTypeNode> PossibleInputSlots
    {
        get { return Project.NodeItems.OfType<ShellSlotTypeNode>().Where(p => !p.IsOutput); }
    }
    public IEnumerable<ShellSlotTypeNode> PossibleOutputSlots
    {
        get { return Project.NodeItems.OfType<ShellSlotTypeNode>().Where(p => p.IsOutput); }
    }
        
        [Section("Custom Selectors", SectionVisibility.WhenNodeIsFilter)]
    public IEnumerable<ShellPropertySelectorItem> CustomSelectors
    {
        get
        {
            return ChildItems.OfType<ShellPropertySelectorItem>();
        }
    }

    public virtual string ClassName
    {
        get { return Name + "Node"; }
    }
}

public class ShellNodeTypeNodeViewModel
{

}