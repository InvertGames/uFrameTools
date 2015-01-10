using System.Collections.Generic;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IItemDrawer : IDrawer
    {
    
    }
    public interface IDrawer 
    {
        GraphItemViewModel ViewModelObject { get; }
        Rect Bounds { get; set; }
        bool IsSelected { get; set; }
        bool Dirty { get; set; }
        string ShouldFocus { get; set; }
        void Draw(IPlatformDrawer platform, float scale);
        void Refresh(IPlatformDrawer platform);
        void Refresh(IPlatformDrawer platform, Vector2 position);
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
}