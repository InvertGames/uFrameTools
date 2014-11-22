using Invert.Core;
using Invert.uFrame;

using UnityEditor;

namespace Invert.Core.GraphDesigner
{
    public abstract class DiagramPlugin : CorePlugin, IDiagramPlugin
    {
        public override bool Enabled
        {
            get { return EditorPrefs.GetBool("UFRAME_PLUGIN_" + this.GetType().Name, EnabledByDefault); }
            set { EditorPrefs.SetBool("UFRAME_PLUGIN_" + this.GetType().Name, value); }
        }

        public override void Loaded()
        {
            
        }
    }
}