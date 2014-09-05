using System;
using System.Collections.Generic;
using System.Linq;
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
    int ZOrder { get; }
    List<IDrawer> Children { get; set; }
   
    void OnDeselecting();
    void OnSelecting();
    void OnDeselected();
    void OnSelected();
    void OnMouseExit(MouseEvent e);
    void OnMouseEnter(MouseEvent e);
    void OnMouseMove(MouseEvent e);
    void OnDrag(MouseEvent e);
    void OnMouseUp(MouseEvent e);
    void OnMouseDoubleClick(MouseEvent mouseEvent);
    void OnRightClick(MouseEvent mouseEvent);
    void OnMouseDown(MouseEvent mouseEvent);
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


    public override void OnDeselecting()
    {
        base.OnDeselecting();
    }

    public override void OnSelecting()
    {
        base.OnSelecting();
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
    }

    public override void OnSelected()
    {
        base.OnSelected();
    }

    public override void OnMouseExit(MouseEvent e)
    {
        base.OnMouseExit(e);
    }

    public override void OnMouseEnter(MouseEvent e)
    {
        base.OnMouseEnter(e);
    }

    public override void OnMouseMove(MouseEvent e)
    {
        base.OnMouseMove(e);
    }

    public override void OnDrag(MouseEvent e)
    {
        base.OnDrag(e);
    }

    public override void OnMouseUp(MouseEvent e)
    {
        base.OnMouseUp(e);
    }

    public override void OnMouseDoubleClick(MouseEvent mouseEvent)
    {
        base.OnMouseDoubleClick(mouseEvent);
    }
}
public abstract class Drawer : IDrawer
{

    private object _dataContext;
    private List<IDrawer> _children;

    protected Drawer()
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

    public virtual Rect Bounds { get; set; }

    public virtual bool IsSelected
    {
        get { return ViewModelObject.IsSelected; }
        set { ViewModelObject.IsSelected = value; }
    }

    public bool Dirty { get; set; }
    public string ShouldFocus { get; set; }

    protected Drawer(GraphItemViewModel viewModelObject)
    {
        ViewModelObject = viewModelObject;
    }

    public virtual void Draw(float scale)
    {
        
    }

    public virtual void Refresh()
    {
        if (ViewModelObject == null)
        {
            Refresh(Vector3.zero);
        }
        else
        {
            Refresh(Bounds.position);
        }
        
    }

    public virtual void Refresh(Vector2 position)
    {
        
    }

    public virtual int ZOrder { get { return 0; } }

    public List<IDrawer> Children
    {
        get { return _children ?? (_children = new List<IDrawer>()); }
        set { _children = value; }
    }
    

    public virtual void OnDeselecting()
    {
       
    }

    public virtual void OnSelecting()
    {
    
    }

    public virtual void OnDeselected()
    {
      
        
    }

    public virtual void OnSelected()
    {
    }

    public virtual void OnMouseExit(MouseEvent e)
    {
        ViewModelObject.IsMouseOver = false;
    }

    public virtual void OnMouseEnter(MouseEvent e)
    {
        ViewModelObject.IsMouseOver = true;
    }

    public virtual void OnMouseMove(MouseEvent e)
    {
      
    }

    public virtual void OnDrag(MouseEvent e)
    {
     
    }

    public virtual void OnMouseUp(MouseEvent e)
    {
   
    }

    public virtual void OnMouseDoubleClick(MouseEvent mouseEvent)
    {
        
    }

    public virtual void OnRightClick(MouseEvent mouseEvent)
    {
        
    }

    public virtual void OnMouseDown(MouseEvent mouseEvent)
    {
        
    }
}

public interface INodeDrawer : IDrawer
{
    ElementsDiagram Diagram { get; set; }
    //Type CommandsType { get; }
    DiagramNodeViewModel ViewModel { get; set; }
}