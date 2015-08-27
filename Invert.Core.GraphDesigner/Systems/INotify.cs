using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner.Systems
{
    public interface INotify
    {
        void Notify(string message, string icon);
        void Notify(string message, NotificationIcon icon);
    }

    public enum NotificationIcon
    {
        Info,
        Error,
        Warning
    }
}
