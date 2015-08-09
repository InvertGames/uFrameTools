using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public class ContextMenuUI : ICommandUI
    {
        private List<IEditorCommand> _commands;
        public bool Flatten { get; set; }

        public List<IEditorCommand> Commands
        {
            get { return _commands ?? (_commands = new List<IEditorCommand>()); }
            set { _commands = value; }
        }

        public ContextMenuUI()
        {
            Commands = new List<IEditorCommand>();
        }

        public void AddCommand(IEditorCommand command)
        {
            Commands.Add(command);
        }

     
        public virtual void Go()
        {
          
        }

        public ICommandHandler Handler { get; set; }
        public void GoBottom()
        {
                
        }

        public virtual void AddSeparator(string empty)
        {
            
        }
    }
}