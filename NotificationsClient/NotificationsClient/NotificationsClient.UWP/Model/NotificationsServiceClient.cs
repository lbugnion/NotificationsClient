using Microsoft.WindowsAzure.Messaging;
using NotificationsClient.Model;
using System;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

namespace NotificationsClient.UWP.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        public event EventHandler<Notification> NotificationReceived;
        public event EventHandler<string> ErrorHappened;
        public event EventHandler<NotificationStatus> StatusChanged;

        private const string Template = "<toast><visual><binding template=\"ToastText02\"><text id=\"1\">$(title)</text><text id=\"2\">$(body)</text></binding></visual></toast>";

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
            // TODO Handle notification message
            NotificationReceived?.Invoke(this, new Notification
            {
                Body = "TODO",
                Title = "TODO",
                Channel = "TODO"
            });
        }

        public void RaiseNotificationReceived(Notification notification)
        {
            NotificationReceived?.Invoke(this, notification);
        }
    }
}