using System;
using Invert.Core.GraphDesigner;
#if !UNITY_DLL
using KeyCode = System.Windows.Forms.Keys;
#else
using UnityEngine;
#endif
namespace Invert.Core.GraphDesigner
{ 
    public interface IKeyBinding
    {
        bool RequireShift { get; set; }
        bool RequireAlt { get; set; }
        bool RequireControl { get; set; }
        KeyCode Key { get; set; }


        string Name { get; set; }
        Type CommandType { get; }
        IEditorCommand Command { get; }
    }
    public interface IKeyboardEvent
    {
        bool KeyEvent(KeyCode keyCode, ModifierKeyState state);
    }
}