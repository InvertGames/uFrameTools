using Invert.Common;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class ConnectionDrawer : Drawer<ConnectionViewModel>
{
    public override int ZOrder
    {
        get { return -1; }
    }

    public ConnectionDrawer(ConnectionViewModel viewModelObject) : base(viewModelObject)
    {
    }

    public override void Draw(float scale)
    {
        base.Draw(scale);
     
        var _startPos = ViewModel.ConnectorA.Bounds.center;
        var _endPos = ViewModel.ConnectorB.Bounds.center;

        var _startRight = ViewModel.ConnectorA.Direction == ConnectorDirection.Output;
        var _endRight = ViewModel.ConnectorB.Direction == ConnectorDirection.Output;


        var multiplier = Mathf.Min(30f,(_endPos.x - _startPos.x) * 0.3f);
     

        var m2 = 3;
        if (multiplier < 0)
        {
            _startRight = !_startRight;
            _endRight = !_endRight;
        }

        
        var startTan = _startPos + (_endRight ? -Vector2.right * m2 : Vector2.right * m2) * multiplier;

        var endTan = _endPos + (_startRight ? -Vector2.right * m2 : Vector2.right * m2) * multiplier;

        var shadowCol = new Color(0, 0, 0, 0.1f);

        for (int i = 0; i < 3; i++) // Draw a shadow
            UnityEditor.Handles.DrawBezier(_startPos * scale, _endPos * scale, startTan * ElementDesignerStyles.Scale, endTan * ElementDesignerStyles.Scale, shadowCol, null, (i + 1) * 5);

        UnityEditor.Handles.DrawBezier(_startPos * scale, _endPos * scale, startTan * ElementDesignerStyles.Scale, endTan * ElementDesignerStyles.Scale, ViewModel.Color, null, 3);
    }
}