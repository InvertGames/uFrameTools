using System.Collections.Generic;

namespace Invert.Core.GraphDesigner.Unity.WindowsPlugin
{
    
    public class ConsoleViewModel : WindowViewModel
    {
        private List<LogMessage> _messages;

        public List<LogMessage> Messages
        {
            get { return _messages ?? (_messages = new List<LogMessage>()); }
            set { _messages = value; }
        }
    }

}