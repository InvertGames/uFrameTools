using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor
{
    public static class NodeConfigExtensions
    {
        public static NodeConfig<TType> Inheritable<TType>(this NodeConfig<TType> config) where TType : GenericInheritableNode
        {
            config.Container.RegisterInstance<IConnectionStrategy>(new InheritanceConnectionStrategy<TType>(), typeof(TType).Name + "_" + typeof(TType).Name + "InheritanceConnection");
            return config;
        }
    }
}