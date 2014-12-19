using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachineGraph : GenericGraphData<StateMachineNodeData>
{
   
}

public class ExternalSubsystemGraph : UnityGraphData<GenericGraphData<SubSystemData>>
{
}

public class ExternalElementGraph : UnityGraphData<GenericGraphData<ElementData>>
{
   
}

public class ExternalStateMachineGraph : UnityGraphData<GenericGraphData<StateMachineNodeData>>
{
   
}