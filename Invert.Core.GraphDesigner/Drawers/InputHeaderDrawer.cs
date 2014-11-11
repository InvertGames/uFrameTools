using Invert.Common;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class InputHeaderDrawer : Drawer<GraphItemViewModel>
{
    public InputHeaderDrawer(GraphItemViewModel viewModelObject)
        : base(viewModelObject)
    {

    }
    public override Rect Bounds
    {
        get { return ViewModelObject.Bounds; }
        set { ViewModelObject.Bounds = value; }
    }
    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
        var size = ElementDesignerStyles.HeaderStyle.CalcSize(new GUIContent(ViewModel.Name));
      
        Bounds = new Rect(position.x + 25, position.y, size.x + 25 , 28);
        ViewModel.ConnectorBounds = new Rect(position.x + 25, position.y, Bounds.width - 20, 28);
        
    }

    public override void Draw(float scale)
    {
        base.Draw(scale);
        
        GUI.Label(Bounds.Scale(scale), ViewModel.Name,ElementDesignerStyles.HeaderStyle);
    }
}