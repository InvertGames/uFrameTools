using System.CodeDom;
using System.Linq;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;

public class StateMachinePlugin : DiagramPlugin
{
    public override void Initialize(uFrameContainer container)
    {
        container.Register<GraphData, StateMachineGraph>("State Machine");

        uFrameEditor.RegisterGraphItem<StateMachineNodeData, StateMachineNodeViewModel, StateMachineNodeDrawer>();

        uFrameEditor.RegisterGraphItem<StateMachineStateData, StateMachineStateNodeViewModel, StateMachineStateNodeDrawer>();
        uFrameEditor.RegisterGraphItem<StateMachineTransition, StateMachineTransitionViewModel, ItemDrawer>();
//        uFrameEditor.RegisterGraphItem<StateMachineVariableData, StateMachineVariableItemViewModel, ElementItemDrawer>();
        uFrameEditor.RegisterGraphItem<StateMachineActionData, StateActionNodeViewModel, StateActionNodeDrawer>();
        
        container.RegisterInstance<IConnectionStrategy>(new StateMachineTransitionConnectionStrategy(), "StateMachineTransitionConnectionStrategy");
        container.RegisterInstance<IConnectionStrategy>(new ElementStateMachineConnectionStrategy(), "ElementStateMachineConnectionStrategy");
        container.RegisterInstance<IConnectionStrategy>(new ElementStateVariableConnectionStrategy(), "ElementStateVariableConnectionStrategy");
        container.RegisterInstance<IConnectionStrategy>(new ComputedTransitionConnectionStrategy(), "ComputedTransitionConnectionStrategy");
        container.RegisterInstance<IConnectionStrategy>(new StartStateConnectionStrategy(), "StartStateConnectionStrategy");

        uFrameEditor.RegisterFilterNode<StateMachineFilter, StateMachineNodeData>();
        //uFrameEditor.RegisterFilterItem<StateMachineFilter, StateMachineVariableData>();
        uFrameEditor.RegisterFilterNode<StateMachineNodeData, StateMachineStateData>();
        //uFrameEditor.RegisterFilterNode<StateMachineStateData, StateMachineStateData>();
        uFrameEditor.RegisterFilterNode<StateMachineStateData, StateMachineActionData>();
        //uFrameEditor.RegisterFilterItem<StateMachineNodeData, StateMachineTransition>();
        uFrameEditor.RegisterFilterNode<ElementData, StateMachineNodeData>();
        uFrameEditor.RegisterFilterNode<ElementData, StateMachineStateData>();
        uFrameEditor.RegisterFilterNode<StateMachineNodeData, ElementData>();
        uFrameEditor.RegisterFilterNode<StateMachineFilter, ElementData>();


       // uFrameEditor.RegisterFilterNode<ViewData, StateMachineNodeData>();
       // uFrameEditor.RegisterFilterNode<StateMachineNodeData, ElementData>();


        container.Register<DesignerGeneratorFactory, StateMachineCodeFactory>("StateMachineCodeFactory");
        container.Register<DesignerGeneratorFactory, StateMachineStateCodeFactory>("StateMachineStateCodeFactory");
        container.RegisterInstance<ITypeGeneratorPostProcessor>(new StateMachineViewModelProcessor(),"StateMachineViewModelPostProcessor");
        
    }

 
}


