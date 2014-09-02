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
    bool Dirty { get; set; }
    string ShouldFocus { get; set; }
    void Draw(float scale);
    void Refresh();
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
    private object _dataContext;

    public Drawer()
    {
    }

    public object DataContext
    {
        get { return _dataContext; }
        set
        {
            _dataContext = value;
            if (_dataContext != null)
            {
                DataContextChanged();
            }
        }
    }

    protected virtual void DataContextChanged()
    {
        
    }

    public GraphItemViewModel ViewModelObject
    {
        get { return DataContext as GraphItemViewModel; }
        set { DataContext = value; }
    }

    public Rect Bounds { get; set; }
    public virtual bool IsSelected { get; set; }
    public bool Dirty { get; set; }
    public string ShouldFocus { get; set; }

    public Drawer(GraphItemViewModel viewModelObject)
    {
        ViewModelObject = viewModelObject;
    }

    public virtual void Draw(float scale)
    {
        
    }

    public void Refresh()
    {
        Refresh(Bounds.position);
    }

    public virtual void Refresh(Vector2 position)
    {
        
    }
}

public interface INodeDrawer : IDrawer
{
    string ShouldFocus { get; }

    
    ElementsDiagram Diagram { get; set; }
    //Type CommandsType { get; }
    DiagramNodeViewModel ViewModel { get; set; }
    IDrawer[] Children { get; set; }
    void DoubleClicked();
    void OnDeselecting();
    void OnSelecting();
    void OnDeselected();
    void OnSelected();
    void OnMouseExit();
    void OnMouseEnter();
    void OnMouseMove();
    void OnDrag();
    void OnMouseUp();
}