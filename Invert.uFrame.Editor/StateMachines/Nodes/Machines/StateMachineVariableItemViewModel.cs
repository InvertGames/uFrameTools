using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

//public class StateMachineVariableItemViewModel : ElementItemViewModel<StateMachineVariableData>
//{
//    public StateMachineVariableItemViewModel(StateMachineVariableData data, DiagramNodeViewModel nodeViewModel)
//        : base(data, nodeViewModel)
//    {
//        DataObject = data;
//    }
    
//    public override string TypeLabel
//    {
//        get
//        {
//            return ElementDataBase.TypeAlias(Data.RelatedTypeName);
//        }
//    }
//}

public class ElementStateMachineConnectionStrategy : DefaultConnectionStrategy<ElementData, StateMachineNodeData>
{
    public override Color ConnectionColor
    {
        get { return Color.white; }
    }
    
    protected override bool IsConnected(ElementData outputData, StateMachineNodeData inputData)
    {
        return inputData.ElementIdentifier == outputData.Identifier;
    }

    protected override void ApplyConnection(ElementData output, StateMachineNodeData input)
    {
        input.ElementIdentifier = output.Identifier;
    }

    protected override void RemoveConnection(ElementData output, StateMachineNodeData input)
    {
        input.ElementIdentifier = null;
    }
}
public class ElementStateVariableConnectionStrategy : DefaultConnectionStrategy<ViewModelPropertyData, StateMachineNodeData>
{
    public override Color ConnectionColor
    {
        get { return Color.white; }
    }

    protected override bool IsConnected(ViewModelPropertyData outputData, StateMachineNodeData inputData)
    {
        return inputData.StatePropertyIdentifier == outputData.Identifier;
    }

    protected override void ApplyConnection(ViewModelPropertyData output, StateMachineNodeData input)
    {
        input.StatePropertyIdentifier = output.Identifier;
    }

    protected override void RemoveConnection(ViewModelPropertyData output, StateMachineNodeData input)
    {
        input.StatePropertyIdentifier = null;
    }
}