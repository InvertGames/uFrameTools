using Invert.Common;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class ConnectorDrawer : Drawer<ConnectorViewModel>
{
    public override int ZOrder
    {
        get { return 1; }
    }

    public ConnectorDrawer(ConnectorViewModel viewModelObject) : base(viewModelObject)
    {
    }

    public Texture2D Texture
    {
        get
        {
            if (ViewModel.Direction == ConnectorDirection.Input)
            {
                switch (ViewModel.Side)
                {
                    case ConnectorSide.Left:
                        return ElementDesignerStyles.ArrowRightTexture;
                        break;
                    case ConnectorSide.Right:
                        return ElementDesignerStyles.ArrowLeftTexture;
                        break;
                    case ConnectorSide.Bottom:
                        return ElementDesignerStyles.ArrowUpTexture;
                    case ConnectorSide.Top:
                        return ElementDesignerStyles.ArrowDownTexture;
                }
            }
            else
            {
                switch (ViewModel.Side)
                {
                    case ConnectorSide.Left:
                        return ElementDesignerStyles.ArrowLeftTexture;
                        break;
                    case ConnectorSide.Right:
                        return ElementDesignerStyles.ArrowRightTexture;
                        break;
                    case ConnectorSide.Bottom:
                        return ElementDesignerStyles.ArrowDownTexture;
                    case ConnectorSide.Top:
                        return ElementDesignerStyles.ArrowUpTexture;
                }
            }
            return ElementDesignerStyles.ArrowLeftTexture;
        }
    }

    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
      
    }
    public override Rect Bounds
    {
        get { return ViewModelObject.Bounds; }
        set { ViewModelObject.Bounds = value; }
    }

    public override void OnMouseDown(MouseEvent mouseEvent)
    {
        base.OnMouseDown(mouseEvent);
        if (mouseEvent.MouseButton == 0 && (ViewModel.Direction == ConnectorDirection.Output || ViewModel.Direction == ConnectorDirection.TwoWay))
        {
            mouseEvent.Begin(new ConnectionHandler(uFrameEditor.CurrentDiagramViewModel, ViewModel));
            mouseEvent.NoBubble = true;
            return;
        }
    }

    public override void OnRightClick(MouseEvent mouseEvent)
    {
        base.OnRightClick(mouseEvent);
        
    }

    public override void Draw(float scale)
    {
        base.Draw(scale);
        var connectorFor = ViewModel.ConnectorFor;
        var connectorBounds = ViewModel.ConnectorFor.ConnectorBounds;
        var forItem = connectorFor as ItemViewModel;
        if (forItem != null)
        {
            if (forItem.NodeViewModel.IsCollapsed)
            {
                connectorBounds = forItem.NodeViewModel.ConnectorBounds;
            }
        }
        var nodePosition = connectorBounds;
        var texture = Texture;
        var pos = new Vector2(0f, 0f);

        if (ViewModel.Side == ConnectorSide.Left)
        {
            pos.x = nodePosition.x;
            pos.y = nodePosition.y + (nodePosition.height * ViewModel.SidePercentage);
            pos.y -= (texture.height/2f);
            pos.x -= (texture.width) + 2;
        }
        else if (ViewModel.Side == ConnectorSide.Right)
        {
            pos.x = nodePosition.x + nodePosition.width;
            pos.y = nodePosition.y + (nodePosition.height * ViewModel.SidePercentage);
            pos.y -= (texture.height / 2f);
            pos.x += 2;
        }
        else if (ViewModel.Side == ConnectorSide.Bottom)
        {
            pos.x = nodePosition.x + (nodePosition.width * ViewModel.SidePercentage);
            pos.y = nodePosition.y + nodePosition.height;
            pos.x -= (texture.width / 2f);
            //pos.y += texture.height;
        }
        else if (ViewModel.Side == ConnectorSide.Top)
        {
            pos.x = nodePosition.x + (nodePosition.width * ViewModel.SidePercentage);
            pos.y = nodePosition.y;
            pos.x -= (texture.width / 2f);
            pos.y -= texture.height;
        }

        Bounds = new Rect(pos.x, pos.y, texture.width, texture.height);
        if (!ViewModel.IsMouseOver)
        {
            var mouseOverBounds = new Rect(Bounds);
            //mouseOverBounds.x -= mouseOverBounds.width*0.2f;
            mouseOverBounds.y += mouseOverBounds.height*0.125f;
            mouseOverBounds.x += mouseOverBounds.width*0.125f;
            mouseOverBounds.width *= 0.75f;
            mouseOverBounds.height *= 0.75f;
            Bounds = mouseOverBounds;
        }
        //if (ViewModelObject.IsMouseOver)
        //{
        //    EditorGUI.DrawRect(Bounds.Scale(scale), Color.black);
        //}
        if (!ViewModel.ConnectorFor.IsMouseOver && !ViewModel.IsMouseOver && !ViewModel.HasConnections) return;
        if (ViewModel.HasConnections)
        {
            GUI.DrawTexture(Bounds.Scale(scale), texture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(Bounds.Scale(scale), texture, ScaleMode.StretchToFill, true);

        }
        GUI.DrawTexture(Bounds.Scale(scale), texture, ScaleMode.StretchToFill, true);
        
    }

    
}