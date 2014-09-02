using System;
using System.Collections.Generic;
using Invert.Common;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class NodeItemHeader : Drawer<DiagramNodeViewModel>
{
    public NodeItemHeader(GraphItemViewModel viewModelObject) : base(viewModelObject)
    {
    }

    public NodeItemHeader(DiagramNodeViewModel viewModelObject) : base(viewModelObject)
    {
    }

    public string Label { get; set; }

    public delegate void AddItemClickedEventHandler();

    public event AddItemClickedEventHandler OnAddItem;

    public IEditorCommand AddCommand { get; set; }

    protected virtual void OnOnAddItem()
    {
        AddItemClickedEventHandler handler = OnAddItem;
        if (handler != null) handler();
    }

    public void Draw( float scale)
    {
        var style = ElementDesignerStyles.HeaderStyle;//.Scale(scale);
        //style.normal.textColor = textColorStyle.normal.textColor;
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
                uFrameEditor.ExecuteCommand(AddCommand);
            }    
        }
        
    }

    public Rect Position { get; set; }
    public Type HeaderType { get; set; }


}