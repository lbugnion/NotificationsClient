using Microsoft.Azure.NotificationHubs;

namespace Notifications.Model
{
    public class Notifications
    {
        public const string ConnectionStringVariableName = "HubConnectionString";
        public const string HubNameVariableName = "HubName";
        public const string AzureWebJobsStorageName = "AzureWebJobsStorage";

        public static Notifications Instance = null;

        public static void Initialize(string connectionString, string hubName)
        {
            if (Instance != null)
            {
                return; 
            }

            Instance = new Notifications(connectionString, hubName);
        }

        public NotificationHubClient Hub 
        { 
            get; 
            private set; 
        }

        private Notifications(string connectionString, string hubName)
        {
            Hub = NotificationHubClient.CreateClientFromConnectionString(
                connectionString,
                hubName);
        }
    }
}
