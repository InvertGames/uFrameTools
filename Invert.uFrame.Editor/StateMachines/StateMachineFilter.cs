using System;

[Serializable]
public class StateMachineFilter : DiagramFilter
{
    public override bool ImportedOnly
    {
        get { return true; }
    }
    public override string Name
    {
        get { return "Root"; }
    }
}