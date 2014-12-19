using System;
using System.Collections.Generic;
#if !UNITY_DLL
using System.Windows.Input;
#endif
namespace Invert.Core.GraphDesigner
{
    public interface IEditorCommand :
#if !UNITY_DLL
    IContextMenuItemCommand
#else
         IContextMenuItemCommand
#endif
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
#if !UNITY_DLL
        void Perform(object arg);
#endif

        IKeyBinding GetKeyBinding();
    }
}