using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DiagramDesigner.Controls;
using Invert.Core;
using Invert.Core.GraphDesigner;
using UnityEngine;

namespace DiagramDesigner.Platform
{
    public class PlatformPlugin : CorePlugin
    {
        public override decimal LoadPriority
        {
            get { return -3; }
        }

        public override bool Required
        {
            get { return true; }
        }

        public override bool Enabled
        {
            get { return true; }
            set
            {

            }
        }

        static PlatformPlugin()
        {
            if (!InvertApplication.IsTestMode)
            {
                InvertGraphEditor.PlatformDrawer = new WindowsPlatformDrawer();
            }
            InvertGraphEditor.Platform = new WindowsPlatformOperations();

            //InvertGraphEditor.Prefs = new WindowsPrefs();


        }

        public override void Initialize(uFrameContainer container)
        {
            container.RegisterInstance<IStyleProvider>(new DesignerStyles());
            //// Where the generated code files are placed
            container.Register<ICodePathStrategy, DefaultCodePathStrategy>("Default");

            container.RegisterInstance<IWindowManager>(new WindowsWindowManager());

        }

        public override void Loaded(uFrameContainer container)
        {

        }
    }

    //public static class WindowsNodeConfigExtensions
    //{
    //    public static NodeConfig<TNodeData> AddNode<TNodeData>(this IUFrameContainer container, string tag = null) where TNodeData : GenericNode
    //    {
    //        container.RegisterDataViewModel<TNodeData, ScaffoldNode<TNodeData>.ViewModel>();
    //        container.RegisterControl<ScaffoldNode<TNodeData>.ViewModel, DiagramNodeControl>();
    //        ;

    //        return container.GetNodeConfig<TNodeData>();
    //    }
    //    public static NodeConfig<TNodeData> AddNode<TNodeData,TViewModel>(this IUFrameContainer container, string tag = null) where TNodeData : GenericNode where TViewModel : DiagramNodeViewModel
    //    {
    //        container.RegisterDataViewModel<TNodeData, TViewModel>();
    //        container.RegisterControl<TViewModel,ScaffoldViewModel<TViewModel>.Control>();

    //        return container.GetNodeConfig<TNodeData>();
    //    }

    //    public static IUFrameContainer RegisterControl<TViewModel,TControl>(this IUFrameContainer container, string tag = null)

    //    {
    //        container.RegisterRelation<TViewModel,System.Windows.Controls.Control,TControl>();
    //        return container;
    //    }

    //    public static Control GetNodeControl(this IUFrameContainer container, ViewModel vm)
    //    {
    //        return container.ResolveRelation<Control>(vm.GetType()) as Control;
    //    }
    //}

    public static class PlatformExtensions
    {
        public static Point ToPoint(this Vector2 vector)
        {
            return new Point(vector.x, vector.y);
        }

        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(Convert.ToSingle(point.X), Convert.ToSingle(point.Y));
        }
    }
}