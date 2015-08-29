using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Systems.GraphUI
{
    public interface IQueryDesignerWindowModalContent
    {
        void QueryDesignerWindowModalContent(List<DesignerWindowModalContent> content);
    }    
    
    public interface IQueryDesignerWindowOverlayContent
    {
        void QueryDesignerWindowOverlayContent(List<DesignerWindowOverlayContent> content);
    }

    public interface IOverlayDrawer
    {
        void Draw(Rect bouds);
        Rect CalculateBounds(Rect diagramRect);
    }

    public class DesignerWindowModalContent
    {
        public Action<Rect> Drawer { get; set; }
        public int ZIndex { get; set; }
    } 

    public class DesignerWindowOverlayContent
    {
        public IOverlayDrawer Drawer { get; set; }
        public bool DisableTransparency { get; set; }
    } 

}
