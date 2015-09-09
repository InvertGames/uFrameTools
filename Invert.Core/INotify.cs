using System;

namespace Invert.Core
{
    public interface INotify
    {
        void Notify(string message, string icon, int time = 5000 );
        void Notify(string message, NotificationIcon icon, int time = 5000);
        void NotifyWithActions(string message, NotificationIcon icon, params NotifyActionItem[] actions);
    }

    public class NotifyActionItem
    {
       public string Title { get; set; }
       public Action Action { get; set; }
    }
}