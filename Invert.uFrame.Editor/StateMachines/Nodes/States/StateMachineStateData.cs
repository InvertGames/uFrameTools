using System.Collections.Generic;
using System.Linq;

public class StateMachineStateData : DiagramNode
{
    private List<StateMachineTransition> _transitions = new List<StateMachineTransition>();

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get { yield break; }
    }

    public List<StateMachineTransition> Transitions
    {
        get { return _transitions; }
        set { _transitions = value; }
    }

    public override string Label
    {
        get { return this.Name; }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { return Transitions.Cast<IDiagramNodeItem>(); }
        set { Transitions = value.OfType<StateMachineTransition>().ToList(); }
    }
    
    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Data.RemoveNode(this);
    }

    //public override void Serialize(JSONClass cls)
    //{
    //    base.Serialize(cls);
    //}

    //public override void Deserialize(JSONClass cls, INodeRepository repository)
    //{
    //    base.Deserialize(cls, repository);

    //}
}