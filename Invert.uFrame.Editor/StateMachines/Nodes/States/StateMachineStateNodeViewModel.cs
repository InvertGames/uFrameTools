using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;

public class StateMachineStateNodeViewModel : DiagramNodeViewModel<StateMachineStateData>
{
    public StateMachineStateNodeViewModel(StateMachineStateData graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {
    }

    public bool IsCurrentState { get; set; }

    public void AddTransition(ViewModelPropertyData item)
    {
        GraphItem.Transitions.Add(new StateMachineTransition()
        {
            Node = GraphItem,
            Name = GraphItem.Data.GetUniqueName("Transition"),
            PropertyIdentifier = item.Identifier
        });
    }
    public void AddTransition()
    {
        GraphItem.Transitions.Add(new StateMachineTransition()
        {
            Node = GraphItem,
            Name = GraphItem.Data.GetUniqueName("Transition"),
            
        });
    }
}