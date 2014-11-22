using System;

namespace Invert.Core.GraphDesigner
{
    public abstract class ContextEditorCommand<TFor,TCommand> : EditorCommand<TFor>, IContextMenuItemCommand
    {
        //public string Path { get; private set; }
        //public bool Checked { get; set; }

        public Type ContextItemFor
        {
            get { return typeof (TCommand); }
        }
    }
}