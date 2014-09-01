using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using UnityEngine;

public class SubSystemDrawer : DiagramNodeDrawer<SubSystemViewModel>
{
    private NodeItemHeader _transitionsHeader;


    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader1; }
    }

    public SubSystemDrawer()
    {
    }

    public SubSystemDrawer(SubSystemData data, ElementsDiagram diagram)
        : base(diagram)
    {
        ViewModel = new SubSystemViewModel(data);
        Diagram = diagram;

    }

    public override GUIStyle ItemStyle
    {
        get { return ElementDesignerStyles.Item4; }
    }

}