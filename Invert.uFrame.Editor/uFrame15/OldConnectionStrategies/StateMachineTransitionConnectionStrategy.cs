//using System.Collections.Generic;
//using Invert.uFrame.Editor.ViewModels;
//using UnityEngine;

//public class StateMachineTransitionConnectionStrategy :
//    DefaultConnectionStrategy<StateTransitionData, StateMachineStateData>
//{
//    public override Color ConnectionColor
//    {
//        get { return Color.cyan; }
//    }

//    public override bool IsStateLink
//    {
//        get { return true; }
//    }


//    public override bool IsConnected(StateTransitionData output, StateMachineStateData input)
//    {
//        return output.TransitionToIdentifier == input.Identifier;
//    }

//    protected override void ApplyConnection(IGraphData graph, StateTransitionData output, StateMachineStateData input)
//    {
//        base.ApplyConnection(graph, output, input);
//        output.TransitionToIdentifier = input.Identifier;
//    }

//    protected override void RemoveConnection(IGraphData graph, StateTransitionData output, StateMachineStateData input)
//    {
//        base.RemoveConnection(graph, output, input);
//        output.TransitionToIdentifier = null;
//    }
//}