using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class ConnectionHandler : DiagramInputHander
{
    public ConnectorViewModel StartConnector { get; set; }
    public ConnectionViewModel CurrentConnection { get; set; }

    public List<ConnectorViewModel> PossibleConnections { get; set; }

    public ConnectionHandler(DiagramViewModel diagramViewModel, ConnectorViewModel startConnector)
        : base(diagramViewModel)
    {
        StartConnector = startConnector;
        PossibleConnections = new List<ConnectorViewModel>();

        foreach (var connector in diagramViewModel.GraphItems.OfType<ConnectorViewModel>())
        {
            foreach (var strategy in InvertGraphEditor.ConnectionStrategies)
            {

                if (strategy.Connect(diagramViewModel, StartConnector, connector) != null)
                {
                    PossibleConnections.Add(connector);
                }
            }
        }
        foreach (var a in PossibleConnections)
        {
            a.IsMouseOver = true;
        }

    }


    public override void OnMouseDown(MouseEvent e)
    {
        foreach (var a in PossibleConnections)
        {
            a.IsMouseOver = false;
        }
        e.Cancel();
    }

    public override void OnMouseMove(MouseEvent e)
    {
        base.OnMouseMove(e);
        var _startPos = StartConnector.Bounds.center;

        var _endPos = e.MousePosition;

        var endViewModel = ViewModelAtMouse as ConnectorViewModel;
        var color = Color.green;



        if (endViewModel == null)
        {
            var nodeAtMouse = ViewModelAtMouse as DiagramNodeViewModel;
            if (nodeAtMouse != null)
            {

                foreach (var connector in nodeAtMouse.Connectors)
                {

                    ConnectionViewModel connection = null;
                    foreach (var strategy in InvertGraphEditor.ConnectionStrategies)
                    {

                        //try and connect them
                        connection = strategy.Connect(DiagramViewModel, StartConnector, connector);
                        if (connection != null)
                            break;

                    }
                    if (connection != null)
                    {
                        CurrentConnection = connection;
                        _endPos = connector.Bounds.center;
                        connector.HasConnections = true;
                        break;
                    }
                }
                if (CurrentConnection != null)
                {
                    // Grab the default connector
                    var adjustedBounds = new Rect(nodeAtMouse.Bounds.x - 9, nodeAtMouse.Bounds.y + 1, nodeAtMouse.Bounds.width + 19, nodeAtMouse.Bounds.height + 9);
                    ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(ElementDesignerStyles.Scale), ElementDesignerStyles.NodeBackground, string.Empty, 20);
                }
            }

        }
        else
        {
            if (InvertGraphEditor.Settings.ShowGraphDebug)
            GUI.Label(new Rect(e.MousePosition.x,e.MousePosition.y,200,50),endViewModel.ConnectorForType.Name,ElementDesignerStyles.HeaderStyle );
            foreach (var strategy in InvertGraphEditor.ConnectionStrategies)
            {
                //try and connect them
                var connection = strategy.Connect(DiagramViewModel, StartConnector, endViewModel);
                if (connection != null)
                {
                    CurrentConnection = connection;
                    break;
                }
            }
            if (CurrentConnection == null)
            {
                color = Color.red;
            }
            else
            {
                _endPos = endViewModel.Bounds.center;

            }
        }


        var _startRight = StartConnector.Direction == ConnectorDirection.Output;
        var _endRight = false;

        var startTan = _startPos + (_endRight ? -Vector2.right * 3 : Vector2.right * 3) * 30;

        var endTan = _endPos + (_startRight ? -Vector2.right * 3 : Vector2.right * 3) * 30;

        var shadowCol = new Color(0, 0, 0, 0.1f);

        for (int i = 0; i < 3; i++) // Draw a shadow
            UnityEditor.Handles.DrawBezier(_startPos * ElementDesignerStyles.Scale, _endPos * ElementDesignerStyles.Scale, startTan * ElementDesignerStyles.Scale, endTan * ElementDesignerStyles.Scale, shadowCol, null, (i + 1) * 5);

        UnityEditor.Handles.DrawBezier(_startPos * ElementDesignerStyles.Scale, _endPos * ElementDesignerStyles.Scale, startTan * ElementDesignerStyles.Scale, endTan * ElementDesignerStyles.Scale, color, null, 3);

    }

    public override void OnMouseUp(MouseEvent e)
    {
        base.OnMouseUp(e);
        if (CurrentConnection != null)
        {
            InvertGraphEditor.ExecuteCommand((v) =>
            {

                CurrentConnection.Apply(CurrentConnection);
            });
        }
        foreach (var a in PossibleConnections)
        {
            a.IsMouseOver = false;
        }
        e.Cancel();
    }

}