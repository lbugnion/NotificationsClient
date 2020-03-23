using GalaSoft.MvvmLight.Ioc;
using Microsoft.WindowsAzure.Messaging;
using NotificationsClient.Model;
using System;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

namespace NotificationsClient.UWP.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        private const string Template = "<toast><visual><binding template=\"ToastText02\"><text id=\"1\">$(title)</text><text id=\"2\">$(body)</text></binding></visual></toast>";

        public async Task Initialize()
        {
            IMessageHandler messageHandler = SimpleIoc.Default.GetInstance<IMessageHandler>();

            try
            {
                var channel = await PushNotificationChannelManager
                    .CreatePushNotificationChannelForApplicationAsync();
                
                var hub = new NotificationHub(
                    Constants.NotificationHubName, 
                    Constants.NotificationHubConnectionString);
                
                await hub.RegisterNativeAsync(channel.Uri);
                await hub.RegisterTemplateAsync(
                    channel.Uri, 
                    Template, 
                    Constants.NotificationHubTemplateName);

                messageHandler.ShowInfo("Ready to receive notifications");
            }
            catch (Exception ex)
            {
                messageHandler.ShowError(ex.Message);
                return;
            }
        }
    }
}