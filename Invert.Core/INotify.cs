namespace Invert.Core
{
    public interface INotify
    {
        void Notify(string message, string icon);
        void Notify(string message, NotificationIcon icon);
    }
}