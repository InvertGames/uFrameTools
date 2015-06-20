using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.IOC;
using Invert.Platform.Gwen;

namespace Invert.GraphDesigner.Standalone
{
    public class StandalonePlugin : DiagramPlugin
    {
        static StandalonePlugin()
        {
            InvertApplication.CachedAssemblies.Add(typeof(StandalonePlugin).Assembly);
        }
        public override void Initialize(UFrameContainer container)
        {
            container.RegisterInstance<IAssetManager>(new WindowsAssetManager());
        }
    }
}