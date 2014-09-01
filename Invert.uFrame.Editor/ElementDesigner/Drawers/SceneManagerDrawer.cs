using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class SceneManagerDrawer : DiagramNodeDrawer<SceneManagerViewModel>
{
    private NodeItemHeader _transitionsHeader;



    public SceneManagerDrawer(SceneManagerData data,ElementsDiagram diagram) : base(diagram)
    {
        ViewModel = new SceneManagerViewModel(data);
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader6; }
    }


    public NodeItemHeader TransitionsHeader
    {
        get { return _transitionsHeader ?? (_transitionsHeader = new NodeItemHeader() { Label = "Transitions", HeaderType = typeof(SceneManagerData) }); }
        set { _transitionsHeader = value; }
    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
        //if (!NodeViewModel.Items.Any()) yield break;
        //yield return new DiagramSubItemGroup()
        //{
        //    Header = TransitionsHeader,
        //    Items = NodeViewModel.Items.ToArray()
        //};
    }

    public override GUIStyle ItemStyle
    {
        get { return ElementDesignerStyles.Item4; }
    }

}