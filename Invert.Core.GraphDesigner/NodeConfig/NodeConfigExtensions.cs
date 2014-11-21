using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor
{
    public static class NodeConfigExtensions
    {
        public static NodeConfig<TType> Inheritable<TType>(this NodeConfig<TType> config,string label = "Base") where TType : GenericInheritableNode
        {
            config.Input<TType, BaseClassReference>((n) =>
            {
                var inheritable = n.Node as GenericInheritableNode;
                if (inheritable != null)
                {
                    var baseNode = inheritable.BaseNode;
                    if (baseNode == null) return label;
                    return "Derived From: " + inheritable.BaseNode.Name;
                }
                return label;
            },false, (o, i) =>
            {
                return o is TType && i is BaseClassReference;
            });
            //config.Container.RegisterInstance<IConnectionStrategy>(new InheritanceConnectionStrategy<TType>(), typeof(TType).Name + "_" + typeof(TType).Name + "InheritanceConnection");
            return config;
        }
    }
}