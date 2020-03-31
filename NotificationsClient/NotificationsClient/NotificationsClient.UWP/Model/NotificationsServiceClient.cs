using Microsoft.WindowsAzure.Messaging;
using NotificationsClient.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

namespace NotificationsClient.UWP.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        public event EventHandler<Notification> NotificationReceived;
        public event EventHandler<string> ErrorHappened;
        public event EventHandler<NotificationStatus> StatusChanged;

        private const string Template = "<toast activationType=\"foreground\" launch=\"$(argument)\"><visual><binding template=\"ToastGeneric\"><text id=\"1\">$(title)</text><text id=\"2\">$(body)</text></binding></visual></toast>";

        public async Task Initialize()
        {

            try
            {
                var channel = await PushNotificationChannelManager
                    .CreatePushNotificationChannelForApplicationAsync();

                channel.PushNotificationReceived += ChannelPushNotificationReceived;
                
                var hub = new NotificationHub(
                    Constants.NotificationHubName, 
                    Constants.NotificationHubConnectionString);
                
                await hub.RegisterNativeAsync(channel.Uri);
                await hub.RegisterTemplateAsync(
                    channel.Uri, 
                    Template, 
                    Constants.NotificationHubTemplateName);

                StatusChanged?.Invoke(this, NotificationStatus.Ready);
            }
            catch (Exception ex)
            {
                ErrorHappened?.Invoke(this, ex.Message);
            }
        }

        private void ChannelPushNotificationReceived(
            PushNotificationChannel sender, 
            PushNotificationReceivedEventArgs args)
        {
            if (args.NotificationType == PushNotificationType.Toast)
            {
                var toastNode = args
                    .ToastNotification
                    .Content
                    .FirstChild; // toast

                var launchAttribute = toastNode.Attributes.FirstOrDefault(a => a.NodeName == "launch");
                RaiseNotificationReceived(launchAttribute.NodeValue.ToString());
            }
        }

        public void RaiseNotificationReceived(Notification notification)
        {
            NotificationReceived?.Invoke(this, notification);
        }

        internal void RaiseNotificationReceived(string arguments)
        {
            var notificationParts = arguments.Split("|@|", StringSplitOptions.RemoveEmptyEntries);

            if (notificationParts.Length == 3)
            {
                RaiseNotificationReceived(new Notification
                {
                    Body = notificationParts[0],
                    Title = notificationParts[1],
                    Channel = notificationParts[2]
                });
            }
        }
    }
}