using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

public class ShellNodeTypeNode : GenericInheritableNode, IShellReferenceType
{
    public IShellReferenceType ReferenceType
    {
        get { return GetConnectionReference<ReferenceItemType>().InputFrom<IShellReferenceType>(); }
    }

    public bool IsCustom { get { return this["Custom"]; }}

    public IEnumerable<ShellNodeTypeNode> SubNodes
    {
        get
        {
            return GetConnectionReference<ShellNodeSubNodesSlot>().OutputsTo<ShellNodeTypeNode>();
        }
    }
    public IEnumerable<ShellGeneratorTypeNode> Generators
    {
        get
        {
            return GetConnectionReference<ShellNodeGeneratorsSlot>().OutputsTo<ShellGeneratorTypeNode>();
        }
    }

    public ShellNodeSectionsSlot ShellNodeSectionsSlot
    {
        get { return GetConnectionReference<ShellNodeSectionsSlot>(); }
    }

    public IEnumerable<ShellNodeSectionsSlot> Sections
    {
        get { return ChildItems.OfType<ShellNodeSectionsSlot>(); }
    }

    public IEnumerable<ShellNodeTypeReferenceSection> ReferenceSections
    {
        get { return Sections.Select(p => p.SourceItem).OfType<ShellNodeTypeReferenceSection>(); }
    }
    public IEnumerable<ShellNodeInputsSlot> InputSlots
    {
        get
        {
            return ChildItems.OfType<ShellNodeInputsSlot>();
        }
    }
    public IEnumerable<ShellNodeOutputsSlot> OutputSlots
    {
        get
        {
            return ChildItems.OfType<ShellNodeOutputsSlot>();
        }
    }

    public virtual string ClassName
    {
        get { return Name + "Node"; }
    }
}