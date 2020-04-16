using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace NotificationsClient.Model
{
    public class ChannelInfo : ObservableObject
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id
        {
            get;
            set;
        }

        public string ChannelName
        {
            get;
            set;
        }

        [SQLite.Ignore]
        public ObservableCollection<Notification> Notifications
        {
            get;
            set;
        }

        public ChannelInfo(string channelName)
            : this()
        {
            ChannelName = channelName;
        }

        public ChannelInfo()
        {
            Notifications = new ObservableCollection<Notification>();
        }
    }
}
