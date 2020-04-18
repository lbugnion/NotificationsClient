using GalaSoft.MvvmLight;

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

        public ChannelInfo()
        {
        }
    }
}
