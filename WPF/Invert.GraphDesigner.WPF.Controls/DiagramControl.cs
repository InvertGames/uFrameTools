using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DiagramDesigner.Platform;
using Invert.Core.GraphDesigner;
using UnityEngine;

namespace Invert.GraphDesigner.WPF.Controls
{
    public class DiagramControl : Canvas
    {
        private DiagramDrawer _drawer;

        public DiagramViewModel ViewModel
        {
            get { return DataContext as DiagramViewModel; }
        }

        public DiagramDrawer Drawer
        {
            get { return _drawer; }
            set
            {
                _drawer = value;
                MouseEvent = null;
            }
        }

        public Pen GridColor { get; set; }
        public Pen GridSecondaryColor { get; set; }
        public DiagramControl()
        {
            
            this.DataContextChanged += DiagramControl_DataContextChanged;
            GridColor = DesignerStyles.GridLine;// new Pen(new SolidColorBrush(new Color() { ScR = 0.01f, ScG = 0.01f, ScB = 0.01f, ScA = 1f }), 1);
            GridSecondaryColor = DesignerStyles.GridLineSecondary; //new Pen(new SolidColorBrush(Colors.Black), 1);
            
        }

        private MouseEvent _event;
        public MouseEvent MouseEvent
        {
            get
            {
                if (_event == null)
                    _event = new MouseEvent(new ModifierKeyState(), Drawer);
                return _event;
            }
            set { _event = value; }
        }
        
        //protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        //{
        //    base.OnMouseDoubleClick(e);
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        MouseEvent.MouseButton = 0;
        //    }
        //    else if (e.RightButton == MouseButtonState.Pressed)
        //    {
        //        MouseEvent.MouseButton = 1;
        //    }
        //    else if (e.MiddleButton == MouseButtonState.Pressed)
        //    {
        //        MouseEvent.MouseButton = 2;
        //    }
        //    if (Drawer != null)
        //    {
        //        MouseEvent.CurrentHandler.OnMouseDoubleClick(MouseEvent);
        //    }
        //}
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                MouseEvent.ModifierKeyStates.Ctrl = true;
            }
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                MouseEvent.ModifierKeyStates.Shift = true;
            }
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                MouseEvent.ModifierKeyStates.Alt = true;
            }
            
            
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                MouseEvent.ModifierKeyStates.Ctrl = false;
            }
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                MouseEvent.ModifierKeyStates.Shift = false;
            }
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                MouseEvent.ModifierKeyStates.Alt = false;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            
            base.OnMouseDown(e);
            MouseEvent.IsMouseDown = true;
            MouseEvent.MouseDownPosition = e.GetPosition(this).ToVector2();
           
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                MouseEvent.MouseButton = 0;
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                MouseEvent.MouseButton = 1;
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                MouseEvent.MouseButton = 2;
            }
            if (Drawer != null)
            {
                if (e.ClickCount == 2)
                {
                    MouseEvent.CurrentHandler.OnMouseDoubleClick(MouseEvent);
                }else 
                if (MouseEvent.MouseButton == 1)
                {
                    MouseEvent.CurrentHandler.OnRightClick(MouseEvent);
                }
                else
                {
                    MouseEvent.CurrentHandler.OnMouseDown(MouseEvent);    
                }
                
            }
            if (SelectionChanged != null)
            {
                SelectionChanged();
            }
            //ViewModel.DeselectAll();
            e.Handled = true;
            InvalidateVisual();
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (ViewModel == null) return;
            MouseEvent.MousePosition = e.GetPosition(this).ToVector2();
            MouseEvent.MousePositionDelta = MouseEvent.MousePosition - MouseEvent.LastMousePosition;
            MouseEvent.MousePositionDeltaSnapped = MouseEvent.MousePosition.Snap(ViewModel.SnapSize * InvertGraphEditor.DesignerWindow.Scale) - MouseEvent.LastMousePosition.Snap(ViewModel.SnapSize * InvertGraphEditor.DesignerWindow.Scale);
            
            if (Drawer != null)
            {
                foreach (var child in Drawer.Children.OfType<ConnectorDrawer>())
                {
            
                        child.Refresh(InvertGraphEditor.PlatformDrawer, Vector2.zero);
                    
                }
            
                MouseEvent.CurrentHandler.OnMouseMove(MouseEvent);
            }
            InvalidateVisual();
            
            MouseEvent.LastMousePosition = MouseEvent.MousePosition;
            
        }
        
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            MouseEvent.IsMouseDown = false;

            MouseEvent.MouseUpPosition = e.GetPosition(this).ToVector2();
            if (e.LeftButton == MouseButtonState.Released)
            {
                MouseEvent.MouseButton = 0;
            }
            else if (e.RightButton == MouseButtonState.Released)
            {
                MouseEvent.MouseButton = 1;
            }
            else if (e.MiddleButton == MouseButtonState.Released)
            {
                MouseEvent.MouseButton = 2;
            }
            if (Drawer != null)
            {
                MouseEvent.CurrentHandler.OnMouseUp(MouseEvent);
            }

            InvalidateVisual();
        }

        public WindowsPlatformDrawer PlatformDrawer
        {
            get { return InvertGraphEditor.PlatformDrawer as WindowsPlatformDrawer; }
        }

        public Action SelectionChanged { get; set; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Drawer != null)
            { 
             
       
            
        
          
            }
            return base.ArrangeOverride(arrangeSize);
        }
        protected override Size MeasureOverride(Size constraint)
        {
            if (Drawer != null)
            { 
                Drawer.Refresh(InvertGraphEditor.PlatformDrawer);
               
            }
            foreach (UIElement item in this.InternalChildren)
            {
                item.Measure(constraint);
            }
            
            if (ViewModel != null)
            {

                return new Size(ViewModel.DiagramBounds.width, ViewModel.DiagramBounds.height);
            }
            return base.MeasureOverride(constraint);
        }

        void DiagramControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
 
            if (ViewModel == null) 
                return;

            if (ViewModel != null)
            {
                Drawer = new DiagramDrawer(ViewModel);
            }

        }



        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);


            dc.DrawRectangle(Background, null, new System.Windows.Rect(0f, 0f, this.ActualWidth, this.ActualHeight));
            var y = 0;
            var alternate = 0;
            for (var i = 0; y < this.ActualHeight; i++, y += 10)
            {
                if (alternate == 5)
                {
                    dc.DrawLine(GridSecondaryColor, new Point(0, y), new Point(this.ActualWidth, y));
                    alternate = 0;
                }
                else
                {
                    dc.DrawLine(GridColor, new Point(0, y), new Point(this.ActualWidth, y));
                    alternate++;
                }

            }
            var x = 0;
            alternate = 0;
            for (var i = 0; x < this.ActualWidth; i++, x += 10)
            {
                if (alternate == 5)
                {
                    dc.DrawLine(GridSecondaryColor, new Point(x, 0), new Point(x, this.ActualHeight));
                    alternate = 0;
                }
                else
                {
                    dc.DrawLine(GridColor, new Point(x, 0), new Point(x, this.ActualHeight));
                    alternate++;
                }


            }


        
            if (Drawer != null)
            {
                PlatformDrawer.BeginRender(this, MouseEvent);
                var platform = InvertGraphEditor.PlatformDrawer as WindowsPlatformDrawer;
                platform.Context = dc;
              
                Drawer.Draw(InvertGraphEditor.PlatformDrawer, 1f);
                PlatformDrawer.EndRender();
            }
      
            
        }

        void GraphItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
          
        }
    }

}
