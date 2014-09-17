using Invert.uFrame.Editor.ViewModels;

public class StateMachineTransitionViewModel : ItemViewModel<StateMachineTransition>
{
    public StateMachineTransitionViewModel(StateMachineTransition data, DiagramNodeViewModel nodeViewModel)
        : base(nodeViewModel)
    {
        DataObject = data;
    }
}