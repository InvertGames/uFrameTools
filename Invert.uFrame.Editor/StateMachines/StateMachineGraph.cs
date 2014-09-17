using System.Collections;
using JSONData = Invert.uFrame.Editor.JSONData;

public class StateMachineGraph : GraphData
{
    private StateMachineFilter _rootFilter = new StateMachineFilter();

    public override IDiagramFilter RootFilter
    {
        get { return StateMachineFilter ?? (StateMachineFilter = new StateMachineFilter()); }
        set { StateMachineFilter = value as StateMachineFilter; }
    }

    public StateMachineFilter StateMachineFilter
    {
        get { return _rootFilter; }
        set { _rootFilter = value; }
    }
}