namespace Invert.Core.GraphDesigner
{
    public static class ConfigExtensions
    {

   
        public static NodeConfig<TNode> GetNodeConfig<TNode>(this IUFrameContainer container) where TNode : GenericNode, IConnectable
        {
            var config = container.Resolve<NodeConfigBase>(typeof(TNode).Name) as NodeConfig<TNode>;
            if (config == null)
            {
                var nodeConfig = new NodeConfig<TNode>(container)
                {
                    
                    NodeType = typeof(TNode),
                };
                container.RegisterInstance<NodeConfigBase>(nodeConfig, typeof(TNode).Name);
                //nodeConfig.Section(string.Empty, _ => _.ChildItems.OfType<GenericConnectionReference>(), false);
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



     
    }
}