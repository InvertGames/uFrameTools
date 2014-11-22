using Invert.Core.GraphDesigner;

namespace Invert.Core.GraphDesigner
{ 
    public interface ICommandUI
    {

        //void Initialize();
        //void DoSingleCommand(IEditorCommand command, object[] contextObjects, UFContextMenuItem item = null);
        //void DoMultiCommand(IEditorCommand parentCommand, IEditorCommand[] childCommands, object[] contextObjects);
        void AddCommand(IEditorCommand command);
        void Go();
        ICommandHandler Handler { get; set; }
        void GoBottom();
    }
}