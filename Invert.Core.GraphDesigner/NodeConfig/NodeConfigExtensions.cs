namespace Invert.Core.GraphDesigner
{
    public static class NodeConfigExtensions
    {
        public static NodeConfig<TType> Inheritable<TType>(this NodeConfig<TType> config,string label = "Base") where TType : GenericInheritableNode
        {

            config.Container.RegisterConnectable<TType,BaseClassReference>();
            //config.Input<TType, BaseClassReference>(n =>
            //{
            //    var inheritable = n.Node as GenericInheritableNode;
                
            //    if (inheritable != null)
            //    {
            //        var baseNode = inheritable.BaseNode;
            //        if (baseNode == null) return label;
            //        return "Derived From: " + inheritable.BaseNode.Name;
            //    }
            //    return label;
            //},false, (o, i) =>
            //{

            //    return o is TType && i is BaseClassReference && i.Node != o.Node;
            //});
            //config.Container.RegisterInstance<IConnectionStrategy>(new InheritanceConnectionStrategy<TType>(), typeof(TType).Name + "_" + typeof(TType).Name + "InheritanceConnection");
            return config;
        }
    }
}