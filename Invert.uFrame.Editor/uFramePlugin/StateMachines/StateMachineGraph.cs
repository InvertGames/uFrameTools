using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JSONData = Invert.uFrame.Editor.JSONData;

public class StateMachineGraph : GenericGraphData<StateMachineNodeData>
{
   
}

public class ExternalSubsystemGraph : UnityGraphData<GenericGraphData<SubSystemData>>
{
    public override string Name
    {
        get { return RootFilter.Name; }
    }
}

public class ExternalElementGraph : UnityGraphData<GenericGraphData<ElementData>>
{
    public override string Name
    {
        get { return RootFilter.Name; }
    }
}

public class ExternalStateMachineGraph : UnityGraphData<GenericGraphData<StateMachineNodeData>>
{
    public override string Name
    {
        get { return RootFilter.Name; }
    }
}