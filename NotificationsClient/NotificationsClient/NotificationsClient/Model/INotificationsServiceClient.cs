using System;
using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    public interface INotificationsServiceClient
    {
        event EventHandler<string> NotificationReceived;
        event EventHandler<string> ErrorHappened;
        event EventHandler<NotificationStatus> StatusChanged;

        Task Initialize();
        void RaiseNotificationReceived(string message);
    }
}
