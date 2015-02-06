using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using UnityEngine;

public class ShellNodeTypeReferenceSection : ShellNodeTypeSection,IReferenceNode, IShellConnectable
{
    [JsonProperty, InspectorProperty]
    public bool IsAutomatic { get; set; }

    [JsonProperty, InspectorProperty]
    public bool AllowDuplicates { get; set; }

    [JsonProperty, InspectorProperty]
    public bool IsEditable { get; set; }
    
    [JsonProperty, InspectorProperty]
    public bool HasPredefinedOptions { get; set; }

    private bool _allowMultipleInputs = true;
    private bool _allowMultipleOutputs = true;

    [JsonProperty, InspectorProperty]
    public bool MultipleInputs
    {
        get { return _allowMultipleInputs; }
        set { _allowMultipleInputs = value; }
    }

    [JsonProperty, InspectorProperty]
    public bool MultipleOutputs
    {
        get { return _allowMultipleOutputs; }
        set { _allowMultipleOutputs = value; }
    }


    [ReferenceSection("Acceptable Types", SectionVisibility.Always, false)]
    public IEnumerable<ShellAcceptableReferenceType> AcceptableTypes
    {
        get { return ChildItems.OfType<ShellAcceptableReferenceType>(); }
    }
    public IEnumerable<IShellNode> PossibleAcceptableTypes
    {
        get { return Project.NodeItems.OfType<IShellNode>(); }
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


  
    public IEnumerable<IReferenceNode> IncludedInSections
    {
        get
        {
            return Project.NodeItems.OfType<IReferenceNode>().Where(p => p.AcceptableTypes.Any(x => x.SourceItem == this));
        }
    }

}