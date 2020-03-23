using GalaSoft.MvvmLight.Ioc;
using Microsoft.WindowsAzure.Messaging;
using NotificationsClient.Model;
using System;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.UI.Popups;

namespace NotificationsClient.UWP.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        public (bool result, string errorMessage) AreOnlineServicesAvailable()
        {
            return (true, null);
        }

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
                
                var result = await hub.RegisterNativeAsync(channel.Uri);

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