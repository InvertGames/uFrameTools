using System;
using System.Collections.Generic;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public interface IDrawer
{
    GraphItemViewModel ViewModelObject { get; }
    Rect Bounds { get; set; }
    bool IsSelected { get; set; }
    void Draw(float scale);
    void Refresh(Vector2 position);
}

public class Drawer<TViewModel> : Drawer where TViewModel : GraphItemViewModel
{
    public Drawer(GraphItemViewModel viewModelObject) : base(viewModelObject)
    {

    }

    public Drawer(TViewModel viewModelObject) : base(viewModelObject)
    {

    }

    public TViewModel ViewModel
    {
        get { return (TViewModel)ViewModelObject; }
    }
}
public class Drawer : IDrawer
{
    public Drawer()
    {
    }

    public GraphItemViewModel ViewModelObject { get; set; }
    public Rect Bounds { get; set; }
    public virtual bool IsSelected { get; set; }

    public Drawer(GraphItemViewModel viewModelObject)
    {
        ViewModelObject = viewModelObject;
    }

    public virtual void Draw(float scale)
    {
        
    }

    public virtual void Refresh(Vector2 position)
    {
        
    }
}

public interface INodeDrawer : IDrawer
{
    string ShouldFocus { get; }

    
    ElementsDiagram Diagram { get; set; }
    Type CommandsType { get; }
    bool Dirty { get; set; }
    DiagramNodeViewModel ViewModel { get; set; }
    IDrawer[] Children { get; set; }
    void DoubleClicked();
    void OnDeselecting(InputManager inputManager);
    void OnSelecting(InputManager inputManager);
    void OnDeselected(InputManager inputManager);
    void OnSelected(InputManager inputManager);
    void OnMouseExit(InputManager inputManager);
    void OnMouseEnter(InputManager inputManager);
    void OnMouseMove(InputManager inputManager);
    void OnDrag(InputManager inputManager);
    void OnMouseUp();
}