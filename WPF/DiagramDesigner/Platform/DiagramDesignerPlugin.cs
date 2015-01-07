using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.Documentation;
using Invert.GraphDesigner.WPF;

namespace DiagramDesigner.Platform
{
    public class DiagramDesignerPlugin : DiagramPlugin
    {
        public override decimal LoadPriority
        {
            get { return -500; }
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
        static DiagramDesignerPlugin()
        {
            InvertGraphEditor.Prefs = new WindowsPrefs();
            
        }

        public override void Initialize(uFrameContainer container)
        {
          
        }
    }
}