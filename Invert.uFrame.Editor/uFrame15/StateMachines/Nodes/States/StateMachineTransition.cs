using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.Json;

public class StateMachineTransition : DiagramNodeItem
{
    public override string FullLabel
    {
        get { return Name; }
    }
    
    public override void Remove(IDiagramNode diagramNode)
    {
        var node = diagramNode as StateMachineNodeData;
        if (node != null)
            node.Transitions.Remove(this);
    }

    public override string Label
    {
        get { return Name; }
    }
    public string TransitionToIdentifier { get; set; }


    
    public StateMachineStateData TransitionTo
    {
        get
        {
            return
                Node.Graph.NodeItems.FirstOrDefault(p => p.Identifier == TransitionToIdentifier) as StateMachineStateData;

        }
    }

    public StateMachineStateData StateMachineState
    {
        get { return Node as StateMachineStateData; }
    }

    public string TriggerId { get; set; }
    public string PropertyIdentifier { get; set; }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        if (TransitionToIdentifier != null)
            cls.Add("TransitionTo", new JSONData(TransitionToIdentifier));

        if (PropertyIdentifier != null)
            cls.Add("PropertyIdentifier", new JSONData(PropertyIdentifier));
    }

    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);
        if (cls["TransitionTo"] != null)
        {
            TransitionToIdentifier = cls["TransitionTo"].Value;
        }
        if (cls["PropertyIdentifier"] != null)
        {
            PropertyIdentifier = cls["PropertyIdentifier"].Value;
        }
    }
}