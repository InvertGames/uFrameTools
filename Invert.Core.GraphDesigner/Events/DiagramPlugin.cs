using Invert.IOC;

namespace Invert.Core.GraphDesigner
{
    public abstract class DiagramPlugin : CorePlugin, IDiagramPlugin
    {
        public override void Initialize(UFrameContainer container)
        {
        
        }

        public void ListenFor<TEvents>() where TEvents : class
        {
            InvertApplication.ListenFor<TEvents>(this);
        }
        public override bool Enabled
        {
            get
            {
                if (InvertGraphEditor.Prefs == null) return true; // Testability
                return InvertGraphEditor.Prefs.GetBool("UFRAME_PLUGIN_" + this.GetType().Name, EnabledByDefault);
            }
            set { InvertGraphEditor.Prefs.SetBool("UFRAME_PLUGIN_" + this.GetType().Name, value); }
        }

        public override void Loaded(UFrameContainer container)
        {

        }

   
    }
}