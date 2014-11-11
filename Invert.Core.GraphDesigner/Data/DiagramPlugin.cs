using Invert.Core;
using Invert.uFrame;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;

namespace Invert.uFrame.Editor
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