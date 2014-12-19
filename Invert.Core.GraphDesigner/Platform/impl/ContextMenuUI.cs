using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public class ContextMenuUI : ICommandUI
    {
        public bool Flatten { get; set; }
        public List<IEditorCommand> Commands { get; set; }

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
    }
}