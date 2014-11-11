using System;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class SimpleEditorCommand<TFor> : EditorCommand<TFor>
    {
        private string _name;
        public override string Name
        {
            get { return _name ?? base.Name; }
        }

        public SimpleEditorCommand(Action<TFor> performAction,string name = null)
        {
            PerformAction = performAction;
            _name = name;
        }

        public Action<TFor> PerformAction { get; set; }

        public override string CanPerform(TFor arg)
        {
            return null;
        }

        public override void Perform(TFor arg)
        {
            this.PerformAction(arg);
        }
    }
}