using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class ComputedTransitionConnectionStrategy :
    DefaultConnectionStrategy<ITypeDiagramItem, StateMachineTransition>
{
    public override Color ConnectionColor
    {
        get { return Color.white; }
    }

    protected override bool IsConnected(ITypeDiagramItem outputData, StateMachineTransition inputData)
    {
        return inputData[outputData.Identifier];
    }

    protected override void ApplyConnection(ITypeDiagramItem output, StateMachineTransition input)
    {
        input[output.Identifier] = true;
    }

    protected override void RemoveConnection(ITypeDiagramItem output, StateMachineTransition input)
    {
        input[output.Identifier] = false;
    }
}

public class StartStateConnectionStrategy :
    DefaultConnectionStrategy<StateMachineNodeData, StateMachineStateData>
{
    public override Color ConnectionColor
    {
        get { return Color.white; }
    }

    public override bool IsStateLink
    {
        get { return true; }
    }

    protected override bool IsConnected(StateMachineNodeData outputData, StateMachineStateData inputData)
    {
        return outputData.StartStateIdentifier == inputData.Identifier;
    }

    protected override void ApplyConnection(StateMachineNodeData output, StateMachineStateData input)
    {
        output.StartStateIdentifier = input.Identifier;
    }

    protected override void RemoveConnection(StateMachineNodeData output, StateMachineStateData input)
    {
        output.StatePropertyIdentifier = null;
    }
}