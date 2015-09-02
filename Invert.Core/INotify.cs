namespace Invert.Core
{
    public interface INotify
    {
        void Notify(string message, string icon, int time = 5000);
        void Notify(string message, NotificationIcon icon, int time = 5000);
    }
}