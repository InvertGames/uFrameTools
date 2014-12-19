using Invert.Core;
using Invert.Core.GraphDesigner;

namespace DiagramDesigner.Platform
{
    public class DiagramDesignerPlugin : DiagramPlugin
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
        public DiagramDesignerPlugin()
        {
            InvertGraphEditor.Prefs = new WindowsPrefs();
        }

        public override void Initialize(uFrameContainer container)
        {
          
        }
    }
}