using Invert.Common;
using Invert.Core.GraphDesigner;
using UnityEngine;

public class StateMachineNodeDrawer : DiagramNodeDrawer<StateMachineNodeViewModel>
{
    private SectionHeaderDrawer _transitionsHeader;

    public StateMachineNodeDrawer()
    {
    }

    protected override object HeaderStyle
    {
        get { return CachedStyles.NodeHeader11; }
    }

    public StateMachineNodeDrawer(StateMachineNodeViewModel viewModel)
        : base(viewModel)
    {

    }
}