using Microsoft.Azure.NotificationHubs;

namespace TestSendNotifications
{
    public class Notifications
    {
        private const string ConnectionString = "Endpoint=sb://lbnotifications.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=hw3WssanDG6Xt3bZJD2EceHDPxxkf+Jol9Jpgmov9Cg=";
        private const string HubName = "lbnotifications";

        public static Notifications Instance = new Notifications();

        public NotificationHubClient Hub 
        { 
            get; 
            set; 
        }

        private Notifications()
        {
            Hub = NotificationHubClient.CreateClientFromConnectionString(
                ConnectionString,
                HubName);
        }
    }
}
