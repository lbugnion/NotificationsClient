using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace NotificationsClient.Model
{
    public class ChannelInfo : ObservableObject
    {
        public string ChannelName
        {
            get;
        }

        public ObservableCollection<Notification> Notifications
        {
            get;
            set;
        }

        public ChannelInfo(string channelName)
        {
            ChannelName = channelName;
            Notifications = new ObservableCollection<Notification>();
        }
    }
}
