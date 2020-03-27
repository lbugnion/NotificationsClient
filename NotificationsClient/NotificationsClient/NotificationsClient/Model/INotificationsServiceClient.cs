using System;
using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    public interface INotificationsServiceClient
    {
        event EventHandler<string> NotificationReceived;

        Task Initialize();
        void RaiseNotificationReceived(string message);
    }
}
