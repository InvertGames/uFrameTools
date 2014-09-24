using System.Collections.Generic;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class StateMachineTransitionConnectionStrategy :
    DefaultConnectionStrategy<StateTransitionData, StateMachineStateData>
{
    public override Color ConnectionColor
    {
        get { return Color.cyan; }
    }

    public override bool IsStateLink
    {
        get { return true; }
    }



    protected override bool IsConnected(StateTransitionData outputData, StateMachineStateData inputData)
    {
        return outputData.TransitionToIdentifier == inputData.Identifier;
    }

    protected override void ApplyConnection(StateTransitionData output, StateMachineStateData input)
    {
        output.TransitionToIdentifier = input.Identifier;
    }

    protected override void RemoveConnection(StateTransitionData output, StateMachineStateData input)
    {
        output.TransitionToIdentifier = null;
    }
}