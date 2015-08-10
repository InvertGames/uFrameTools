using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity.WindowsPlugin
{
 
    public class WindowsSystem : DiagramPlugin, IOpenWindow
    {

        public Vector2 DefaultSize
        {
            get { return new Vector2(400, 400); }
        }

        public void OpenWindow<T>(T viewModel, WindowType type = WindowType.Normal, Vector2? position = null, Vector2? size = null,
            Vector2? minSize = null, Vector2? maxSize = null) where T : GraphItemViewModel
        {
            var window = ScriptableObject.CreateInstance<UnityWindowDrawer>();
            window.ViewModelObject = viewModel;

            var finalSize = size ?? DefaultSize;
            var finalPosition = position ?? new Vector2((Screen.currentResolution.width - finalSize.x) / 2, (Screen.currentResolution.height - finalSize.y) / 2);

            switch (type)
            {
                case WindowType.Normal:
                    window.Show();
                    break;
                case WindowType.Popup:
                    window.ShowPopup();
                    break;
                case WindowType.FocusPopup:
                    window.ShowAsDropDown(new Rect(finalPosition,Vector2.one),finalSize );
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }

            window.minSize = minSize ?? finalSize;
            window.maxSize = maxSize ?? finalSize;

            window.position = new Rect(finalSize, finalPosition);
            window.Focus();
            window.Repaint();

        }
    }


    public class UnityWindowDrawer : EditorWindow, IDrawer {
        private IPlatformDrawer _platformDrawer;
        private List<IDrawer> _children;
        private GraphItemViewModel _viewModelObject;

        public IPlatformDrawer PlatformDrawer
        {
            get { return _platformDrawer ?? (_platformDrawer = InvertApplication.Container.Resolve<IPlatformDrawer>()); }
            set { _platformDrawer = value; }
        }

        void OnGUI()
        {
            if (ViewModelObject == null) return;
            Draw(PlatformDrawer,1.0f);
        }

        public GraphItemViewModel ViewModelObject
        {
            get { return _viewModelObject; }
            set
            {
                _viewModelObject = value; 
                RefreshContent(Children);
            }
        }

        public Rect Bounds
        {
            get { return this.position; }
            set { }
        }

        public bool Enabled
        {
            get { return true; }
            set { }
        }

        public bool IsSelected { get; set; }

        public bool Dirty { get; set; }

        public string ShouldFocus { get; set; }


        public void RefreshContent(List<IDrawer> children)
        {
            children.Clear();
            children.Add(InvertApplication.Container.CreateDrawer(ViewModelObject));
        }

        public void Draw(IPlatformDrawer platform, float scale)
        {
            Refresh(PlatformDrawer,new Vector2(0,0));
            DrawChildren();
        }

        public void Refresh(IPlatformDrawer platform)
        {
        }

        public void OnLayout()
        {
        }

        public void DrawChildren()
        {
            foreach (var item in Children)
            {
                if (item.Dirty)
                {
                    Refresh(PlatformDrawer, item.Bounds.position, false);
                    item.Dirty = false;
                }
                item.Draw(PlatformDrawer, 1.0f);
            }
        }

        public void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            foreach (var child in Children)
            {
                child.Refresh(platform, new Vector2(10, 0), hardRefresh);
            }

            foreach (var child in Children)
            {
                child.OnLayout();
            }
        }

        public int ZOrder { get; private set; }

        public List<IDrawer> Children
        {
            get { return _children ?? (_children = new List<IDrawer>()); }
            set { _children = value; }
        }

        public IDrawer ParentDrawer { get; set; }

        public void OnDeselecting()
        {
        }

        public void OnSelecting()
        {
        }

        public void OnDeselected()
        {
        }

        public void OnSelected()
        {
        }

        public void OnMouseExit(MouseEvent e)
        {
        }

        public void OnMouseEnter(MouseEvent e)
        {
        }

        public void OnMouseMove(MouseEvent e)
        {
        }

        public void OnDrag(MouseEvent e)
        {
        }

        public void OnMouseUp(MouseEvent e)
        {
        }

        public void OnMouseDoubleClick(MouseEvent mouseEvent)
        {
        }

        public void OnRightClick(MouseEvent mouseEvent)
        {
        }

        public void OnMouseDown(MouseEvent mouseEvent)
        {
        }
    
    }

}
