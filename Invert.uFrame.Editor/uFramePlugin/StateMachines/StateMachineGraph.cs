using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JSONData = Invert.uFrame.Editor.JSONData;

public class StateMachineGraph : GenericGraphData<StateMachineNodeData>
{
   
}

public class ExternalSubsystemGraph : GenericGraphData<SubSystemData>
{
    public override string Name
    {
        get { return RootFilter.Name; }
    }
}

public class ExternalElementGraph : GenericGraphData<ElementData>
{
    public override string Name
    {
        get { return RootFilter.Name; }
    }
}

public class ExternalStateMachineGraph : GenericGraphData<StateMachineNodeData>
{
    public override string Name
    {
        get { return RootFilter.Name; }
    }
}