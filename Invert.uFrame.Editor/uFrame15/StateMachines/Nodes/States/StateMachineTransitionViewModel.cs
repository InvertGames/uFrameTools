using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;

public class StateMachineTransitionViewModel : ItemViewModel<StateMachineTransition>
{
    public StateMachineTransitionViewModel(StateMachineTransition data, DiagramNodeViewModel nodeViewModel)
        : base(nodeViewModel)
    {
        DataObject = data;
    }
}

public class StateTransitionViewModel : ItemViewModel<StateTransitionData>
{
    public StateTransitionViewModel(StateTransitionData data, DiagramNodeViewModel nodeViewModel)
        : base(nodeViewModel)
    {
        DataObject = data;
    }
}