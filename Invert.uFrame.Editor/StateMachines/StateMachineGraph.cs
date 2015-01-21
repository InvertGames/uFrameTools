using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JSONData = Invert.uFrame.Editor.JSONData;

public class StateMachineGraph : GraphData<StateMachineNodeData>
{
   
}

public class GraphData<T> : GraphData where T : IDiagramFilter, new()
{
    public T FilterNode
    {
        get { return (T)RootFilter; }
    }
    protected override IDiagramFilter CreateDefaultFilter()
    {
        return new T()
        {
            Name = name
        };
    }
}

public class ExternalSubsystemGraph : GraphData<SubSystemData>
{
    
}

public class ExternalElementGraph : GraphData<ElementData>
{

}

public class ExternalStateMachineGraph : GraphData<StateMachineNodeData>
{

}