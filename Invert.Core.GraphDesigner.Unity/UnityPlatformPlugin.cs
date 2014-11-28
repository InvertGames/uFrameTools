using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.UnitySpecific;
using Invert.uFrame;
using Invert.uFrame.Editor;
using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity
{
    public class UnityPlatformPlugin : CorePlugin
    {
        public override decimal LoadPriority
        {
            get { return -95; }
        }

        public override bool Required
        {
            get { return true; }
        }

        static UnityPlatformPlugin()
        {
            InvertGraphEditor.Prefs = new UnityPlatformPreferences();
            InvertApplication.Logger = new UnityPlatform();
            InvertGraphEditor.Platform = new UnityPlatform();
            InvertGraphEditor.PlatformDrawer = new UnityDrawer();
        }
        public override bool Enabled { get { return true; } set{} }
        public override void Initialize(uFrameContainer container)
        {
            
           
            container.RegisterInstance<IAssetManager>(new UnityAssetManager());
            // Drawers
         
            container.Register<SectionHeaderDrawer, SectionHeaderDrawer>();
            container.RegisterItemDrawer<GenericItemHeaderViewModel, GenericChildItemHeaderDrawer>();

            container.RegisterDrawer<SectionHeaderViewModel, SectionHeaderDrawer>();
            container.RegisterDrawer<ConnectorViewModel, ConnectorDrawer>();
            container.RegisterDrawer<ConnectionViewModel, ConnectionDrawer>();
            container.RegisterDrawer<InputOutputViewModel, SlotDrawer>();

            // Default Graph Item Drawers
            container.RegisterDrawer<EnumNodeViewModel, DiagramEnumDrawer>();
            container.RegisterDrawer<EnumItemViewModel, EnumItemDrawer>();
            container.RegisterDrawer<ClassPropertyItemViewModel, ElementItemDrawer>();
            container.RegisterDrawer<ClassCollectionItemViewModel, ElementItemDrawer>();
            container.RegisterDrawer<ClassNodeViewModel, ClassNodeDrawer>();


            // Command Drawers
            container.Register<ToolbarUI, UnityToolbar>();
            container.Register<ContextMenuUI, UnityContextMenu>();

            container.RegisterInstance<IGraphEditorSettings>(new UFrameSettings());
            // Where the generated code files are placed
            container.Register<ICodePathStrategy, DefaultCodePathStrategy>("Default");
            container.RegisterInstance<IToolbarCommand>(new DiagramSettingsCommand() { Title = "Settings" }, "SettingsCommand");
            container.RegisterInstance<IWindowManager>(new UnityWindowManager());

           
        }

        public override void Loaded()
        {
           
        }
    }


    public static class UnityPlatformContainerExtensions
    {
        public static IDrawer CreateDrawer(this IUFrameContainer container, ViewModel viewModel)
        {
            return CreateDrawer<IDrawer>(container, viewModel);
        }

        public static IDrawer CreateDrawer<TDrawerBase>(this IUFrameContainer container, ViewModel viewModel) where TDrawerBase : IDrawer
        {
            if (viewModel == null)
            {
                Debug.LogError("Data is null.");
                return null;
            }
            var drawer = container.ResolveRelation<TDrawerBase>(viewModel.GetType(), viewModel);
            if (drawer == null)
            {
                Debug.Log(String.Format("Couldn't Create drawer for {0}.", viewModel.GetType()));
            }
            return drawer;
        }

        public static IUFrameContainer RegisterGraphItem<TModel, TViewModel, TDrawer>(this IUFrameContainer container)
        {
            container.RegisterRelation<TModel, ViewModel, TViewModel>();
            container.RegisterDrawer<TViewModel, TDrawer>();
            return container;
        }

        public static void RegisterItemDrawer<TViewModel, TDrawer>(this IUFrameContainer container)
        {
            container.RegisterRelation<TViewModel, IDrawer, TDrawer>();
        }
        public static void RegisterDrawer<TViewModel, TDrawer>(this IUFrameContainer container)
        {
            container.RegisterRelation<TViewModel, IDrawer, TDrawer>();
        }

        public static IUFrameContainer RegisterChildGraphItem<TModel, TViewModel, TDrawer>(this IUFrameContainer container)
        {
            container.RegisterRelation<TModel, ItemViewModel, TViewModel>();
            container.RegisterItemDrawer<TViewModel, TDrawer>();
            return container;
        }

        public static NodeConfig<TNodeData> AddNode<TNodeData, TNodeViewModel, TNodeDrawer>(this IUFrameContainer container, string name) where TNodeData : GenericNode, IConnectable
        {

            container.AddItem<TNodeData>();
            container.RegisterGraphItem<TNodeData, TNodeViewModel, TNodeDrawer>();
            var config = container.GetNodeConfig<TNodeData>();
            if (config.Tags.Count > 0)
                return config;
            config.Tags.Add(name ?? typeof(TNodeData).Name);
            config.Name = name;
            return config;
        }
        public static NodeConfig<TNodeData> AddNode<TNodeData>(this IUFrameContainer container, string tag = null)
       where TNodeData : GenericNode
        {
            var config = container.AddNode<TNodeData, ScaffoldNode<TNodeData>.ViewModel, ScaffoldNode<TNodeData>.Drawer>(tag);
            return config;
        }


        public static IUFrameContainer AddItem<TNodeData, TNodeViewModel, TNodeDrawer>(this IUFrameContainer container) where TNodeData : IDiagramNodeItem
        {
            container.RegisterChildGraphItem<TNodeData, TNodeViewModel, TNodeDrawer>();
            return container;
        }
        public static IUFrameContainer AddItem<TNodeData>(this IUFrameContainer container) where TNodeData : IDiagramNodeItem
        {
            container.RegisterChildGraphItem<TNodeData, ScaffoldNodeChildItem<TNodeData>.ViewModel, ScaffoldNodeChildItem<TNodeData>.Drawer>();
            return container;
        }
        public static IUFrameContainer AddTypeItem<TNodeData>(this IUFrameContainer container) where TNodeData : ITypedItem
        {
            container.AddItem<TNodeData>();
            container.RegisterChildGraphItem<TNodeData,
                ScaffoldNodeTypedChildItem<TNodeData>.ViewModel,
                ScaffoldNodeTypedChildItem<TNodeData>.Drawer>();
            return container;
        }
        public static IUFrameContainer AddTypeItem<TNodeData, TViewModel, TDrawer>(this IUFrameContainer container) where TNodeData : ITypedItem
        {
            container.AddItem<TNodeData>();
            container.RegisterChildGraphItem<TNodeData,
                TViewModel,
                TDrawer>();
            return container;
        }
        public static NodeConfig<TGraphNode> AddGraph<TGraphType, TGraphNode>(this IUFrameContainer container, string name)
            where TGraphType : GraphData
            where TGraphNode : GenericNode
        {

            container.Register<GraphData, TGraphType>(name);
            return AddNode<TGraphNode>(container, name);
        }
        public static IUFrameContainer RegisterGraphItem<TModel>(this uFrameContainer container) where TModel : GenericNode
        {
            container.RegisterGraphItem<TModel, ScaffoldNode<TModel>.ViewModel, ScaffoldNode<TModel>.Drawer>();
            //RegisterDrawer();
            return container;
        }
    }
    public class ScaffoldNode<TData> where TData : GenericNode
    {

        public class Drawer : GenericNodeDrawer<TData, ViewModel>
        {


            public Drawer(ViewModel viewModel)
                : base(viewModel)
            {

            }
        }

        public class ItemDrawer : ScaffoldNodeChildItem<TData>.Drawer
        {
            public ItemDrawer(ScaffoldNodeChildItem<TData>.ViewModel viewModel)
                : base(viewModel)
            {
            }
        }

        public class ItemViewModel : ScaffoldNodeChildItem<TData>.ViewModel
        {
            public ItemViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }

        public class ScaffoldTypedItemDrawer : ScaffoldNodeTypedChildItem<TData>.Drawer
        {
            public ScaffoldTypedItemDrawer(ScaffoldNodeTypedChildItem<TData>.ViewModel viewModel)
                : base(viewModel)
            {
            }
        }

        public class TypedItemViewModel : ScaffoldNodeTypedChildItem<TData>.ViewModel
        {
            public TypedItemViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }

        public class ViewModel : GenericNodeViewModel<TData>
        {
            public ViewModel(TData graphItemObject, DiagramViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }
    }

    public class ScaffoldNodeChildItem<TData> where TData : IDiagramNodeItem
    {
        public class Drawer : ItemDrawer
        {
            public Drawer(ViewModel viewModel)
                : base(viewModel)
            {
            }
        }

        public class ViewModel : GenericItemViewModel<TData>
        {
            public ViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }
    }

    public class ScaffoldNodeTypedChildItem<TData> where TData : ITypedItem
    {
        public class Drawer : ElementItemDrawer
        {
            public Drawer(ViewModel viewModel)
                : base(viewModel)
            {
            }
        }

        public class ViewModel : TypedItemViewModel
        {
            public override string TypeLabel
            {
                get { return Data.RelatedTypeName; }
            }

            public ViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }


    }

}
