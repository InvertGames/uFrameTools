﻿namespace Invert.Core.GraphDesigner
{
    public abstract class DiagramPlugin : CorePlugin, IDiagramPlugin
    {
        public override bool Enabled
        {
            get { return InvertGraphEditor.Prefs.GetBool("UFRAME_PLUGIN_" + this.GetType().Name, EnabledByDefault); }
            set { InvertGraphEditor.Prefs.SetBool("UFRAME_PLUGIN_" + this.GetType().Name, value); }
        }

        public override void Loaded()
        {
            
        }

        public virtual void CommandExecuted(ICommandHandler handler, IEditorCommand command)
        {
            
        }
    }
}