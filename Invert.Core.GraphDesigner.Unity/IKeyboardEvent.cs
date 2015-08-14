using UnityEngine;

namespace Invert.Core.GraphDesigner.Unity
{
    public interface IKeyboardEvent
    {
        void KeyEvent(KeyCode keyCode, ModifierKeyState state);
    }
}