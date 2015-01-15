using System.Collections.Generic;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public abstract class Drawer : IDrawer, IItemDrawer
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

        public virtual void Draw(IPlatformDrawer platform, float scale)
        {

        }

        public virtual void Refresh(IPlatformDrawer platform)
        {
            if (ViewModelObject == null)
            {
                Refresh(platform, Vector2.zero);
            }
            else
            {
                Refresh(platform, Bounds.position);
            }
        
        }

        public virtual void Refresh(IPlatformDrawer platform, Vector2 position)
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

        public virtual void OnLayout()
        {
            
        }
    }

    public interface INodeDrawer : IDrawer
    {
        DiagramDrawer Diagram { get; set; }
        //Type CommandsType { get; }
        DiagramNodeViewModel ViewModel { get; set; }
    }
}