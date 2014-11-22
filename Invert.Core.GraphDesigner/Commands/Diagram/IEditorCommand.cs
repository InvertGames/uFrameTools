using System;
using System.Collections.Generic;
using Invert.uFrame.Editor;

namespace Invert.Core.GraphDesigner
{
    public interface IEditorCommand : IContextMenuItemCommand
    {
        Type For { get; }
        void Execute(object item);
        List<IEditorCommand> Hooks { get; }
        string Name { get; }
        string Title { get; set; }
        decimal Order { get; }
        bool ShowAsDiabled { get; }
        string Group { get;  }
        string CanPerform(object arg);


        IKeyBinding GetKeyBinding();
    }

    public abstract class EditorCommand<TFor> : EditorCommand, IDiagramContextCommand, IDiagramNodeCommand
    {
        public override Type For
        {
            get { return typeof(TFor); }
        }

        public sealed override void Perform(object item)
        {
            Perform((TFor)item);
        }

        public abstract void Perform(TFor node);

        public override string CanPerform(object arg)
        {
            return CanPerform((TFor) arg);
        }

        public sealed override bool IsChecked(object arg)
        {
            if (arg == null) return false;
            return IsChecked((TFor)arg);
        }

        public virtual bool IsChecked(TFor arg)
        {
            return false;
        }

        public abstract string CanPerform(TFor node);

    }
}