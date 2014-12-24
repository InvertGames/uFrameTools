using Invert.Core;
using Invert.Core.GraphDesigner;

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

        public override void Initialize(uFrameContainer container)
        {
            container.Register <ContextMenuUI,WPFContextMenu>();
        }

        public override void Loaded()
        {
            
        }
    }
}