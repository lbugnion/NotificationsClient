using NotificationsClient.Model;

namespace NotificationsClient.ViewModel
{
    public class NotificationDeletedEventArgs
    {
        public NotificationViewModel Notification
        {
            get;
            set;
        }
    }
}