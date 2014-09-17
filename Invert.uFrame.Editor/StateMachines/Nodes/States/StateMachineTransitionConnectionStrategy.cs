using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class StateMachineTransitionConnectionStrategy :
    DefaultConnectionStrategy<StateMachineTransition, StateMachineStateData>
{
    public override Color ConnectionColor
    {
        get { return Color.cyan; }
    }

    protected override bool IsConnected(StateMachineTransition outputData, StateMachineStateData inputData)
    {
        return outputData.TransitionToIdentifier == inputData.Identifier;
    }

    protected override void ApplyConnection(StateMachineTransition output, StateMachineStateData input)
    {
        output.TransitionToIdentifier = input.Identifier;
    }

    protected override void RemoveConnection(StateMachineTransition output, StateMachineStateData input)
    {
        output.TransitionToIdentifier = null;
    }
}