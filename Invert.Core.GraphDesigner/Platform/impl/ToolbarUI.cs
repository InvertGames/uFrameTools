using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public class ToolbarUI : ICommandUI
    {
        public ToolbarUI()
        {
            LeftCommands = new List<IEditorCommand>();
            RightCommands = new List<IEditorCommand>();
            BottomLeftCommands = new List<IEditorCommand>();
            BottomRightCommands = new List<IEditorCommand>();
            AllCommands = new List<IEditorCommand>();
        }

        public List<IEditorCommand> AllCommands { get; set; }

        public List<IEditorCommand> LeftCommands { get; set; }
        public List<IEditorCommand> RightCommands { get; set; }
        public List<IEditorCommand> BottomLeftCommands { get; set; }
        public List<IEditorCommand> BottomRightCommands { get; set; }
       

        public void AddCommand(IEditorCommand command)
        {
            AllCommands.Add(command);
            var cmd = command as IToolbarCommand;
            if (cmd == null || cmd.Position == ToolbarPosition.Right)
            {
                RightCommands.Add(command);
            }
            else if (cmd.Position == ToolbarPosition.BottomLeft)
            {
                BottomLeftCommands.Add(command);
            }else if (cmd.Position == ToolbarPosition.BottomRight)
            {
                BottomRightCommands.Add(command);
            }
            else
            {
                LeftCommands.Add(command);
            }
        }

 

        public virtual void Go()
        {
    

           
        }

        public virtual void GoBottom()
        {
          
            
        }
        public ICommandHandler Handler { get; set; }

  
    }
}