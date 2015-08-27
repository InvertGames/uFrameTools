using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public class ContextMenuItem
    {
        private string _title;

        public string Title
        {
            get
            {
            
                return _title ?? Command.GetType().Name;
            }
            set { _title = value; }
        }

        public string Path { get; set; }
        public ICommand Command { get; set; }
        public string Group { get; set; }
        public object Order { get; set; }
        public bool Checked { get; set; }
    }
    public class ContextMenuUI
    {
        private List<ContextMenuItem> _commands;
        public bool Flatten { get; set; }

        public List<ContextMenuItem> Commands
        {
            get { return _commands ?? (_commands = new List<ContextMenuItem>()); }
            set { _commands = value; }
        }

        public ContextMenuUI()
        {
            Commands = new List<ContextMenuItem>();
        }

        public void AddCommand(ContextMenuItem command)
        {
            Commands.Add(command);
        }

     
        public virtual void Go()
        {
          
        }

 
        public void GoBottom()
        {
                
        }

        public virtual void AddSeparator()
        {
            
        }
    }
}