using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;

public class StateMachineStateNodeViewModel : DiagramNodeViewModel<StateMachineStateData>
{
    public StateMachineStateNodeViewModel(StateMachineStateData graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {
    }

    public bool IsCurrentState { get; set; }

    //public void AddTransition(ViewModelPropertyData item)
    //{
    //    GraphItem.Transitions.Add(new StateMachineTransition()
    //    {
    //        Node = GraphItem,
    //        Name = GraphItem.Project.GetUniqueName("Transition"),
    //        PropertyIdentifier = item.Identifier
    //    });
    //}
    public void AddTransition()
    {
        var currentFilter = DiagramViewModel.CurrentRepository.CurrentFilter as StateMachineNodeData;

        ItemSelectionWindow.Init("Select Transition",currentFilter.Transitions.Cast<IItem>().ToArray(), (item) =>
        {
            InvertGraphEditor.ExecuteCommand((diagram) =>
            {
                GraphItem.Transitions.Add(new StateTransitionData()
                {
                    Node = GraphItem,
                    Name = GraphItem.Project.GetUniqueName("Transition"),
                    TransitionIdentifier = ((IDiagramNodeItem)item).Identifier,
                    TransitionToIdentifier = null
                });
            });
        });
      
    }
}