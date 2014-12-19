using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;

public class StateMachineStateData : DiagramNode
{
    private List<StateTransitionData> _transitions = new List<StateTransitionData>();


    public List<StateTransitionData> Transitions
    {
        get { return _transitions; }
        set { _transitions = value; }
    }

    public override string Label
    {
        get { return this.Name; }
    }

    public override IEnumerable<IDiagramNodeItem> PersistedItems
    {
        get { return Transitions.Cast<IDiagramNodeItem>(); }
        set { Transitions = value.OfType<StateTransitionData>().ToList(); }
    }
    
    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Project.RemoveNode(this);
    }
    public StateMachineNodeData Machine
    {
        get
        {
            return Project.NodeItems.OfType<StateMachineNodeData>().FirstOrDefault(p => p.States.Contains(this));
        }
    }

    public override void NodeItemRemoved(IDiagramNodeItem item)
    {
        Transitions.Remove(item as StateTransitionData);
        Transitions.RemoveAll(p => p.TransitionIdentifier == item.Identifier);

    }

    public override void NodeRemoved(IDiagramNode nodeData)
    {
        base.NodeRemoved(nodeData);

        Transitions.RemoveAll(
            p => p.TransitionIdentifier == nodeData.Identifier || p.TransitionToIdentifier == nodeData.Identifier);


    }
}

public class StateTransitionData : DiagramNodeItem
{
    public override string FullLabel
    {
        get { return Name; }
    }

    public override void Remove(IDiagramNode diagramNode)
    {
        var state = this.Node as StateMachineStateData;
        if (state != null)
            state.Transitions.Remove(this);
    }

    public override string Label
    {
        get { return Name; }
    }

    public override string Name
    {
        get
        {
            var transition = Transition;
            if (transition == null)
            {
                Remove(Node);
                return null;
            }

            return transition.Name;
        }
    }

    public string TransitionIdentifier { get; set; }

    public string TransitionToIdentifier { get; set; }

    public StateMachineStateData TransitionTo
    {
        get
        {
            return this.Machine.States.FirstOrDefault(p => TransitionToIdentifier == p.Identifier);
        }
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        if (TransitionToIdentifier != null)
        cls.Add("TransitionToIdentifier",new JSONData(TransitionToIdentifier));
        if (TransitionIdentifier != null)
            cls.Add("TransitionIdentifier", new JSONData(TransitionIdentifier));
    }

    public StateMachineStateData State
    {
        get { return Node as StateMachineStateData; }
    }

    public StateMachineNodeData Machine
    {
        get { return State.Machine; }
    }

    public StateMachineTransition Transition
    {
        get { return Machine.Transitions.FirstOrDefault(p => p.Identifier == TransitionIdentifier); }
    }
    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);

        if (cls["TransitionToIdentifier"] != null)
            TransitionToIdentifier = cls["TransitionToIdentifier"].Value;

        if (cls["TransitionIdentifier"] != null)
            TransitionIdentifier = cls["TransitionIdentifier"].Value;

    }
}