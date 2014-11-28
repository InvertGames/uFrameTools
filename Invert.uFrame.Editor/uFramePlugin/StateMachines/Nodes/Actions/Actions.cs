using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;


public class StateMachineActionData : DiagramNode
{
    public override IEnumerable<IDiagramNodeItem> DisplayedItems
    {
        get { yield break; }
    }

    public override string Label
    {
        get { return null; }
    }

    public string ActionClassType { get; set; }



    public override IEnumerable<IDiagramNodeItem> PersistedItems
    {
        get { yield break; }
        set
        {

        }
    }

    public override void NodeItemRemoved(IDiagramNodeItem item)
    {
        
    }
}

public class StateActionNodeViewModel : DiagramNodeViewModel<StateMachineActionData>
{
    public StateActionNodeViewModel(StateMachineActionData graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {
    }


}

public class StateActionNodeDrawer : DiagramNodeDrawer<StateActionNodeViewModel>
{
    public StateActionNodeDrawer(StateActionNodeViewModel viewModel)
        : base(viewModel)
    {

    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader8; }
    }

}
