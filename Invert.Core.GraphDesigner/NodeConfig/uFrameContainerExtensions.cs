﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor
{
    public static class uFrameContainerExtensions
    {
        public static uFrameContainer SetNodeColor<TNode>(this uFrameContainer container, NodeColor color) where TNode : GenericNode, IConnectable
        {
            ((NodeConfig) container.GetNodeConfig<TNode>()).Color = color;
            return container;
        }
        public static NodeConfig<TNodeData> AddNode<TNodeData>(this IUFrameContainer container, string tag = null)
            where TNodeData : GenericNode
        {
            var config = container.AddNode<TNodeData, ScaffoldNode<TNodeData>.ViewModel, ScaffoldNode<TNodeData>.Drawer>();
            config.Tags.Add(tag ?? typeof(TNodeData).Name);
            return config;
        }
        
        public static NodeConfig<TNodeData> AddNode<TNodeData, TNodeViewModel, TNodeDrawer>(this IUFrameContainer container) where TNodeData : GenericNode, IConnectable
        {
           
            container.AddItem<TNodeData>();
            container.RegisterGraphItem<TNodeData, TNodeViewModel, TNodeDrawer>();
            return container.GetNodeConfig<TNodeData>();
        }

        public static IUFrameContainer AddItem<TNodeData, TNodeViewModel, TNodeDrawer>(this IUFrameContainer container) where TNodeData : IDiagramNodeItem
        {
            container.RegisterChildGraphItem<TNodeData, TNodeViewModel, TNodeDrawer>();
            return container;
        }
        public static IUFrameContainer AddItem<TNodeData>(this IUFrameContainer container) where TNodeData : IDiagramNodeItem
        {
            container.RegisterChildGraphItem<TNodeData, ScaffoldNodeChildItem<TNodeData>.ViewModel,ScaffoldNodeChildItem<TNodeData>.Drawer>();
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

        public static NodeConfig<TNode> GetNodeConfig<TNode>(this IUFrameContainer container) where TNode : GenericNode, IConnectable
        {
            var config = container.Resolve<NodeConfig>(typeof(TNode).Name) as NodeConfig<TNode>;
            if (config == null)
            {
                var nodeConfig = new NodeConfig<TNode>(container)
                {
                    NodeType = typeof(TNode),
                };
                container.RegisterInstance<NodeConfig>(nodeConfig, typeof(TNode).Name);

                return nodeConfig;
            }
            return config;
        }

        //public static IUFrameContainer ScaffoldNodeChild<TNode, TChildItem>(this IUFrameContainer container, string header = null)
        //    where TChildItem : GenericNodeChildItem
        //    where TNode : GenericNode, IConnectable
        //{
        //    container.RegisterNodeSection<TNode, TChildItem>(header);
        //    container.RegisterGraphItem<TChildItem, ScaffoldNodeChildItem<TChildItem>.ViewModel, ScaffoldNodeChildItem<TChildItem>.Drawer>();
        //    return container;
        //}

        //public static IUFrameContainer RegisterNodeSection<TNode, TChildItem>(this IUFrameContainer container, string header = null, Func<TNode,IEnumerable<TChildItem>> selector = null) where TNode : GenericNode, IConnectable
        //{
        //    var config = container.GetNodeConfig<TNode>();
        //    var sectionConfig = new NodeConfigSection<TNode>()
        //    {
        //        ChildType = typeof (TChildItem),
        //        Name = header,
        //    };
        //    if (selector != null)
        //    {
        //        sectionConfig.Selector = p => selector(p).Cast<IGraphItem>();
        //    }
        //    config.Sections.Add(sectionConfig);
        //    return container;
        //}



        public static NodeConfig<TGraphNode> AddGraph<TGraphType,TGraphNode>(this IUFrameContainer container, string name)
            where TGraphType : GraphData
            where TGraphNode : GenericNode
        {
            
            container.Register<GraphData, TGraphType>(name);
            return AddNode<TGraphNode>(container,name);
        }
    }
}