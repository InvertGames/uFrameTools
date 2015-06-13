using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.IOC;

namespace Invert.GraphDesigner.WPF
{
    public class WPFPlugin : DiagramPlugin
    {
        public override decimal LoadPriority
        {
            get { return -1; }
        }

        public override bool Required
        {
            get { return true; }
        }

        public override void Initialize(UFrameContainer container)
        {
            container.Register <ContextMenuUI,WPFContextMenu>();
        }

        public override void Loaded(UFrameContainer container)
        {
            
        }
    }
}