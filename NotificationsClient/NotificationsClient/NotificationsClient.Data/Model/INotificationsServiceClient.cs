using System;
using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    public interface INotificationsServiceClient
    {
        event EventHandler<Notification> NotificationReceived;
        event EventHandler<string> ErrorHappened;
        event EventHandler<NotificationStatus> StatusChanged;

        Task Initialize();
        void RaiseNotificationReceived(Notification notification);
    }
}
