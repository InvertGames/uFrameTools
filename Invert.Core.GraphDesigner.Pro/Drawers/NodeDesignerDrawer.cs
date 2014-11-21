using UnityEngine;

public class NodeDesignerDrawer : GenericNodeDrawer<ShellNodeTypeNode, ShellNodeTypeViewModel>
{
    public NodeDesignerDrawer(ShellNodeTypeViewModel viewModel) : base(viewModel)
    {
    }

    
    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
       // Bounds = new Rect(Bounds.x, Bounds.y, Bounds.width, Bounds.height + 50f);

    }

    public override void Draw(float scale)
    {
        base.Draw(scale);

    }
}