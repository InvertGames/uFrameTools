using System;
using Invert.uFrame.Editor.ElementDesigner;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class SimpleKeyBinding : KeyBinding<IEditorCommand>
    {

        public SimpleKeyBinding(Action<ElementsDiagram> command, KeyCode key, bool requireControl = false, bool requireAlt = false, bool requireShift = false)
            : base(key, requireControl, requireAlt, requireShift)
        {
            _command = new SimpleEditorCommand<ElementsDiagram>(command);
        }

        public SimpleKeyBinding(Action<ElementsDiagram> command, string name, KeyCode key, bool requireControl = false, bool requireAlt = false, bool requireShift = false)
            : base(name, key, requireControl, requireAlt, requireShift)
        {
            _command = new SimpleEditorCommand<ElementsDiagram>(command);
        }
        public SimpleKeyBinding(IEditorCommand command, KeyCode key, bool requireControl = false, bool requireAlt = false, bool requireShift = false)
            : base(key, requireControl, requireAlt, requireShift)
        {
            _command = command;
        }

        public SimpleKeyBinding(IEditorCommand command, string name, KeyCode key, bool requireControl = false, bool requireAlt = false, bool requireShift = false)
            : base(name, key, requireControl, requireAlt, requireShift)
        {
            _command = command;
        }


    }
}