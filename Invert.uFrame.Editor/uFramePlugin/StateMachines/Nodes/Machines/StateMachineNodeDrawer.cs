using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
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