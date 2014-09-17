using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class StateMachineNodeDrawer : DiagramNodeDrawer<StateMachineNodeViewModel>
{
    private NodeItemHeader _transitionsHeader;

    public StateMachineNodeDrawer()
    {
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader11; }
    }

    public NodeItemHeader TransitionsHeader
    {
        get
        {
            if (_transitionsHeader == null)
            {
                _transitionsHeader = Container.Resolve<NodeItemHeader>(null, false, ViewModel);
                _transitionsHeader.Label = "Transitions";
                _transitionsHeader.HeaderType = typeof(SceneTransitionItemViewModel);
                if (NodeViewModel.IsLocal)
                    _transitionsHeader.AddCommand = new SimpleEditorCommand<StateMachineNodeViewModel>((node) =>
                    {
                        
                    });
            }
            return _transitionsHeader;
        }
        set { _transitionsHeader = value; }
    }

    public StateMachineNodeDrawer(StateMachineNodeViewModel viewModel)
        : base(viewModel)
    {

    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
        if (!NodeViewModel.IsCurrentFilter)
        drawers.Insert(1, TransitionsHeader);
        
    }
}