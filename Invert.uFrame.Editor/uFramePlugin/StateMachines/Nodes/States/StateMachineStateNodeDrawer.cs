using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class StateMachineStateNodeDrawer : DiagramNodeDrawer<StateMachineStateNodeViewModel>
{
    private SectionHeaderDrawer _transitionsHeader;

    public StateMachineStateNodeDrawer()
    {
    }

    
    public StateMachineStateNodeDrawer(StateMachineStateNodeViewModel viewModel)
        : base(viewModel)
    {
    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
    
    }

    public override void Draw(float scale)
    {
        base.Draw(scale);
        if (NodeViewModel.IsCurrentState)
        {
          
                var adjustedBounds = new Rect(Bounds.x - 9, Bounds.y + 1, Bounds.width + 19, Bounds.height + 9);
                ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(Scale), ElementDesignerStyles.BoxHighlighter1, string.Empty, 20);
            
        }
    }
}