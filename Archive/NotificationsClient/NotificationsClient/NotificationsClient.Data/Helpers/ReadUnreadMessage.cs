using GalaSoft.MvvmLight.Messaging;
using NotificationsClient.ViewModel;

namespace NotificationsClient.Helpers
{
    public class ReadUnreadMessage : MessageBase
    {
        public ReadUnreadMessage(
            NotificationViewModel sender)
            : base(sender)
        {
        }
    }
}
