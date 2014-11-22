using Invert.Common;
using Invert.Core.GraphDesigner;
using UnityEngine;

public class StateMachineNodeDrawer : DiagramNodeDrawer<StateMachineNodeViewModel>
{
    private SectionHeaderDrawer _transitionsHeader;

    public StateMachineNodeDrawer()
    {
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader11; }
    }

    public StateMachineNodeDrawer(StateMachineNodeViewModel viewModel)
        : base(viewModel)
    {

    }
}