using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;

public interface IShellNode : IDiagramNode, IItem, IClassTypeNode, IConnectable
{
    bool IsCustom { get; }
}

public interface IInputOutpuItem : IShellNode
{
}
public class ShellNode : GenericNode, IShellNode 
{
  
    [ InspectorProperty]
    public bool IsCustom
    {
        get { return this["Custom"]; }
        set { this["Custom"] = value; }
    }


    public virtual string ClassName
    {
        get { return string.Format("{0}", Name); }
    }
}

public class ShellInheritableNode : GenericInheritableNode, IShellNode
{

    [InspectorProperty]
    public bool IsCustom
    {
        get { return this["Custom"]; }
        set { this["Custom"] = value; }
    }


    public virtual string ClassName
    {
        get { return string.Format("{0}", Name); }
    }
}
public class ShellNodeTypeNode : ShellInheritableNode, IShellNode, IShellConnectable
{
    private string _classFormat = "{0}";
    private bool _allowMultipleOutputs;

    [JsonProperty, InspectorProperty]
    public bool AllowMultipleInputs { get; set; }

    [JsonProperty, InspectorProperty]
    public bool AllowMultipleOutputs
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

    [OutputSlot("Sub Nodes")]
    public MultiOutputSlot<ShellNodeTypeNode> SubNodesSlot { get; set; }

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

    public override string ClassName
    {
        get { return Name + "Node"; }
    }

    [ReferenceSection("Connectable To", SectionVisibility.WhenNodeIsFilter, false)]
    public IEnumerable<ShellConnectableReferenceType> ConnectableTo
    {
        get { return ChildItems.OfType<ShellConnectableReferenceType>(); }
    }

    public IEnumerable<IShellNode> PossibleConnectableTo
    {
        get { return Project.NodeItems.OfType<IShellNode>(); }
    }

}

public class ShellNodeTypeNodeViewModel
{

}

public interface IOutputCapable
{
    
}

public interface IInputCapable
{
    
}
public class ShellConnectionDefitionNode
{
    [InputSlot("Output")]
    public SingleInputSlot<IOutputCapable> OutputItem { get; set; }
    [OutputSlot("Input")]
    public SingleInputSlot<IInputCapable> InputItem { get; set; }
}