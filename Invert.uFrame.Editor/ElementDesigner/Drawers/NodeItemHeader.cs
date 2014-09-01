using System;
using System.Collections.Generic;
using Invert.Common;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEngine;

public class NodeItemHeader
{

    public string Label { get; set; }

    public delegate void AddItemClickedEventHandler();

    public event AddItemClickedEventHandler OnAddItem;

    public IEditorCommand AddCommand { get; set; }

    protected virtual void OnOnAddItem()
    {
        AddItemClickedEventHandler handler = OnAddItem;
        if (handler != null) handler();
    }

    public void Draw(ElementsDiagram diagram, float scale,GUIStyle textColorStyle)
    {
        var style = ElementDesignerStyles.HeaderStyle;//.Scale(scale);
        style.normal.textColor = textColorStyle.normal.textColor;
        style.fontStyle = FontStyle.Bold;

        GUI.Box(Position.Scale(scale), Label, style);
        var btnRect = new Rect();
        btnRect.y = Position.y + ((Position.height / 2) - 8);
        btnRect.x = Position.x + Position.width - 18;
        btnRect.width = 16;
        btnRect.height = 16;
        if (AddCommand != null)
        {
            if (GUI.Button(btnRect.Scale(scale), string.Empty, ElementDesignerStyles.AddButtonStyle))
            {
                // This is a ugly hack but it'll get r dun
                diagram.DeselectAll();
                diagram.MouseOverViewData.IsSelected = true;
                diagram.ExecuteCommand(AddCommand);
            }    
        }
        
    }

    public Rect Position { get; set; }
    public Type HeaderType { get; set; }

    public void CreateLink(IDiagramNode container, IGraphItem target)
    {
        throw new NotImplementedException();
    }

    public bool CanCreateLink(IGraphItem target)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] nodes)
    {
        yield break;
    }
}