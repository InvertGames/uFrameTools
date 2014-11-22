using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class ViewComponentDrawer : DiagramNodeDrawer<ViewComponentNodeViewModel>
{
    private SectionHeaderDrawer _additiveScenesHeader;


    public ViewComponentDrawer(ViewComponentNodeViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader7; }
    }

  
    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
        //if (!ViewModel.Items.Any()) yield break;
        //yield return new DiagramSubItemGroup()
        //{
        //    Header = AdditiveScenesHeader,
        //    Items = ViewModel.Items.ToArray()
        //};
    }

    public override GUIStyle ItemStyle
    {
        get { return ElementDesignerStyles.Item4; }
    }

}