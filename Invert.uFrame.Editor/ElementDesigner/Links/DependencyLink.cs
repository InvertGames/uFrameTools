using UnityEngine;

public class DependencyLink : BeizureLink
{
    public IViewModelItem To { get; set; }

    public IViewModelItem Item { get; set; }

    public override bool DrawShadow
    {
        get { return false; }
    }

    public override ISelectable Source { get { return Item; } }

    public override ISelectable Target { get { return To; } }
    public override Color GetColor(ElementsDiagram diagram)
    {
        return Color.black;
    }
}