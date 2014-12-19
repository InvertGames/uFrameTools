using System;

namespace Invert.Core.GraphDesigner
{
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