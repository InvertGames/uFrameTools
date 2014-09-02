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
    
    public override void Refresh(Vector2 position)
    {
        base.Refresh(position);
     

        

        Bounds = new Rect(position.x, position.y,100,25);
    }

    public Rect _AddButtonRect;

    public override void Draw( float scale)
    {
        base.Draw(scale);
        var style = ElementDesignerStyles.HeaderStyle;
        _AddButtonRect = new Rect
        {
            y = Bounds.y + ((Bounds.height/2) - 8),
            x = Bounds.x + Bounds.width - 18,
            width = 16,
            height = 16
        };

        //.Scale(scale);
        //style.normal.textColor = textColorStyle.normal.textColor;
        style.fontStyle = FontStyle.Bold;

        GUI.Box(Bounds.Scale(scale), Label, style);
       
        //if (AddCommand != null)
        //{
            if (GUI.Button(_AddButtonRect.Scale(scale), string.Empty, ElementDesignerStyles.AddButtonStyle))
            {
                uFrameEditor.ExecuteCommand(AddCommand);
            }    
        //}
        
    }

    
    public Type HeaderType { get; set; }


}