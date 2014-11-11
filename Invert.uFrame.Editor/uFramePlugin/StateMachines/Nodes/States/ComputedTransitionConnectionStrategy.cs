using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class ComputedTransitionConnectionStrategy :
    DefaultConnectionStrategy<ITypedItem, StateMachineTransition>
{
    public override Color ConnectionColor
    {
        get { return Color.white; }
    }

    protected override bool IsConnected(ITypedItem output, StateMachineTransition input)
    {
        return input[output.Identifier];
    }

    protected override void ApplyConnection(ITypedItem output, StateMachineTransition input)
    {
        input[output.Identifier] = true;
    }

    protected override void RemoveConnection(ITypedItem output, StateMachineTransition input)
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

    protected override bool IsConnected(StateMachineNodeData output, StateMachineStateData input)
    {
        return output.StartStateIdentifier == input.Identifier;
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