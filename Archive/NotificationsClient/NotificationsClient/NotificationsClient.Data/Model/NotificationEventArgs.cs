namespace NotificationsClient.Model
{
    public class NotificationEventArgs
    {
        public Notification Notification
        {
            get;
            set;
        }

        public bool IsDelayed
        {
            get;
            set;
        }
    }
}
