using GalaSoft.MvvmLight;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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

        [SQLite.Ignore]
        public DateTime LastReceived
        {
            get
            {
                if (Notifications.Count == 0)
                {
                    return DateTime.MinValue;
                }

                return Notifications.First().ReceivedTimeUtc;
            }
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
