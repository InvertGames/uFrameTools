using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using JSONData = Invert.uFrame.Editor.JSONData;

public class StateMachineNodeData : DiagramNode,IDesignerType
{
    private List<StateMachineTransition> _globalTransitions;
    //private List<StateMachineVariableData> _variables;

    public override string Label
    {
        get { return Name; }
    }


    public List<StateMachineTransition> Transitions
    {
        get { return _globalTransitions ?? (_globalTransitions = new List<StateMachineTransition>()); }
        set { _globalTransitions = value; }
    }

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get { return Transitions.Cast<IDiagramNodeItem>(); }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { return Transitions.Cast<IDiagramNodeItem>(); }
        set
        {
            Transitions = value.OfType<StateMachineTransition>().ToList();
        }
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Project.RemoveNode(this);
    }

    public override void NodeRemoved(IDiagramNode nodeData)
    {
        base.NodeRemoved(nodeData);
        if (StartStateIdentifier == nodeData.Identifier)
            StartStateIdentifier = null;
    }

    public IEnumerable<StateMachineStateData> States
    {
        get { return this.GetContainingNodes(Diagram).OfType<StateMachineStateData>(); }
    }

    public override CodeTypeReference GetFieldType(ITypedItem itemData)
    {
        return new CodeTypeReference(Name);
    }

    public override void NodeItemRemoved(IDiagramNodeItem item)
    {
        Transitions.Remove(item as StateMachineTransition);
    }

    public override CodeTypeReference GetPropertyType(ITypedItem itemData)
    {
        return new CodeTypeReference(uFrameEditor.UFrameTypes.State);
    }

    public ElementData Element
    {
        get { return Project.NodeItems.OfType<ElementData>().FirstOrDefault(p => p.Identifier == ElementIdentifier); }
    }
    public string ElementIdentifier { get; set; }
    public string StatePropertyIdentifier { get; set; }

    public StateMachineStateData StartState
    {
        get { return States.FirstOrDefault(p => p.Identifier == StartStateIdentifier); }
    }

    public string StartStateIdentifier { get; set; }
    public override void Serialize(Invert.uFrame.Editor.JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("StartStateIdentifier",new Invert.uFrame.Editor.JSONData(StartStateIdentifier ?? string.Empty));
    }

    public override void Deserialize(Invert.uFrame.Editor.JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["StartStateIdentifier"] != null)
        {
            StartStateIdentifier = cls["StartStateIdentifier"].Value;
        }
    }
}