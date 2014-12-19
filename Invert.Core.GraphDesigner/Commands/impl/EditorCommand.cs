using System;
using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public abstract class EditorCommand : 
#if !UNITY_DLL
      // System.Windows.Input.RoutedCommand,
        System.Windows.Input.ICommand,
#endif

 IEditorCommand
    {
        private List<IEditorCommand> _hooks;
        private string _title;


        public abstract Type For { get; }
#if !UNITY_DLL
        bool System.Windows.Input.ICommand.CanExecute(object parameter)
        {
            return true;
            return CanPerform(parameter) == null;
        }
#endif
        public virtual void Execute(object item)
        {
#if UNITY_DLL
            Perform(item);
#else
            // IN wpf this graph editor will invoke perform, not this method
            InvertGraphEditor.ExecuteCommand(this);
#endif
        }

        public event EventHandler CanExecuteChanged;

        public abstract void Perform(object arg);

        public List<IEditorCommand> Hooks
        {
            get { return _hooks ?? (_hooks = new List<IEditorCommand>()); }
            set { _hooks = value; }
        }

        public virtual string Name
        {
            get { return this.GetType().Name.Replace("Command",""); }
        }

        public virtual string Title
        {
            get
            {
                if (string.IsNullOrEmpty(_title))
                {
                    return Name;
                }
                return _title;
            }
            set { _title = value; }
        }

        public string CanExecute(object arg)
        {
            return null;
        }

        public virtual decimal Order { get { return 0; } }
        public virtual bool ShowAsDiabled { get { return false; } }
        public virtual string Group { get { return "Default"; }}
        public abstract string CanPerform(object arg);
        public virtual string Path { get { return Name; } }

        public virtual bool IsChecked(object arg)
        {
            return false;
        }

        public virtual IKeyBinding GetKeyBinding()
        {
            return null;
        } 
    }
    
}