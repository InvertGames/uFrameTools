//using Invert.uFrame.Editor.ViewModels;
//using UnityEngine;

//public class ComputedPropertyInputsConnectionStrategy :
//    DefaultConnectionStrategy<ITypedItem, ComputedPropertyData>
//{
//    public override Color ConnectionColor
//    {
//        get { return Color.white; }
//    }

//    protected override bool CanConnect(ITypedItem output, ComputedPropertyData input)
//    {
//        if (output.Identifier == input.Identifier) return false;
//        //if (!(output is ComputedPropertyData) && !(output is ViewPropertyData)) return false;
//        return base.CanConnect(output, input);
//    }

//    public override bool IsConnected(ITypedItem output, ComputedPropertyData input)
//    {
//        return input.DependantPropertyIdentifiers.Contains(output.Identifier);
//    }

//    protected override void ApplyConnection(ITypedItem output, ComputedPropertyData input)
//    {
//        input.DependantPropertyIdentifiers.Add(output.Identifier);
//    }

//    protected override void RemoveConnection(ITypedItem output, ComputedPropertyData input)
//    {
//        input.DependantPropertyIdentifiers.Remove(output.Identifier);
//    }
//}