using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Invert.MVVM;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor.MVVMDesigner
{
    public class DiagramViewModel : ViewModel
    {

        public readonly P<string> _TitleProperty = new P<string>("New Diagram");
        public readonly P<int> _SnapSizeProperty = new P<int>(10);
        public readonly P<Vector2> _ScrollPositionProperty = new P<Vector2>(new Vector2(0, 0));

        public string Title
        {
            get { return _TitleProperty.Value; }
            set { _TitleProperty.Value = value; }
        }

        public int SnapSize
        {
            get { return _SnapSizeProperty.Value; }
            set { _SnapSizeProperty.Value = value; }
        }

        public Vector2 ScrollPosition
        {
            get { return _ScrollPositionProperty.Value; }
            set { _ScrollPositionProperty.Value = value; }
        }

    }

    public class View<TViewModel> : IViewFor<TViewModel>
    {
        private ViewModel _contextObject;

        public Type ViewFor
        {
            get { return typeof (TViewModel); }
        }

        public ViewModel ContextObject
        {
            get { return _contextObject; }
            set { this.SetContext(ref _contextObject,value); }
        }


        public virtual void Bind()
        {
            throw new NotImplementedException();
        }

        public void UnBind()
        {
            throw new NotImplementedException();
        }

        public TViewModel Context { get; set; }

        public View()
        {
        }

        public View(TViewModel context)
        {
            Context = context;
        }
    }

    public interface IView
    {
        Type ViewFor { get; }
        ViewModel ContextObject { get; set; }
        void Bind();
        void UnBind();
    }

    public interface IViewFor<TViewModel> : IView
    {

        TViewModel Context { get; set; }
    }

    public static class IViewExtensions
    {
        public static void SetContext(this IView view,ref ViewModel contextField, ViewModel value)
        {
            if (contextField != null)
            {
                view.UnBind();
            }
            if (value != null)
            {
                contextField = value;
            }
        }
    }

    public class UFWindow : EditorWindow
    {
        private IWindowView _mainView;

        public IWindowView MainView
        {
            get { return _mainView; }
            set
            {
                _mainView = value;

                if (_mainView != null)
                _mainView.ContextObject.PropertyChanged += ContextObjectOnPropertyChanged;
            }
        }

        public UFWindow(IWindowView mainView)
        {
            _mainView = mainView;
        }

        public UFWindow()
        {
        }

        private void ContextObjectOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            Repaint();
        }

        public void OnGUI()
        {
            if (MainView != null)
            {
                MainView.Rect = new Rect(0,0,Screen.width,Screen.height);
                MainView.Draw();
            }
                
        }


    }

    public interface IUnityView : IView
    {

        void Draw();
    }

    public interface IWindowView : IUnityView
    {
        Rect Rect { get; set; }

    }

}
