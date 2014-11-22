using System;

namespace Invert.Core.GraphDesigner
{
    public class DebugCommand : ElementsDiagramToolbarCommand
    {
        public string _name;

        public DebugCommand(string name, Action<DiagramViewModel> action)
        {
            _name = name;
            Action = action;
        }

        public DebugCommand(Action<DiagramViewModel> action)
        {
            Action = action;
        }

        public override string Name
        {
            get { return _name; }
        }

        Action<DiagramViewModel> Action { get; set; }
        public override void Perform(DiagramViewModel node)
        {
            if (Action != null)
                Action(node);

        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.BottomRight; }
        }
    }
}