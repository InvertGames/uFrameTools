//using Invert.uFrame.Editor.ViewModels;
//using UnityEngine;

//public class ElementStateMachineConnectionStrategy : DefaultConnectionStrategy<ElementData, StateMachineNodeData>
//{
//    public override Color ConnectionColor
//    {
//        get { return Color.white; }
//    }

//    public override bool IsConnected(ElementData output, StateMachineNodeData input)
//    {
//        return input.ElementIdentifier == output.Identifier;
//    }

//    protected override void ApplyConnection(ElementData output, StateMachineNodeData input)
//    {
//        input.ElementIdentifier = output.Identifier;
//    }

//    protected override void RemoveConnection(ElementData output, StateMachineNodeData input)
//    {
//        input.ElementIdentifier = null;
//    }
//}