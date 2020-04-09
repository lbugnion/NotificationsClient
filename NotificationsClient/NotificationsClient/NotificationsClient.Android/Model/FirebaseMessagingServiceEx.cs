using Android.App;
using Android.Content;
using Firebase.Messaging;
using GalaSoft.MvvmLight.Ioc;
using Notifications.Data;
using NotificationsClient.Model;
using System;
using System.Threading.Tasks;
using WindowsAzure.Messaging;

namespace NotificationsClient.Droid.Model
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseMessagingServiceEx : FirebaseMessagingService
    {
        private const string Template = "{\"notification\":{\"body\":\"$(body)\",\"title\":\"$(title)\"},\"data\":{\"body\":\"$(body)\",\"title\":\"$(title)\",\"channel\":\"$(channel)\"}}";

        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            SendRegistrationToServer(token);
        }

        public override void OnMessageReceived(RemoteMessage remoteMessage)
        {
            base.OnMessageReceived(remoteMessage);

            var title = string.Empty;
            var body = string.Empty;
            var channel = string.Empty;

            if (remoteMessage.Data.ContainsKey("title"))
            {
                title = remoteMessage.Data["title"];
            }
            if (remoteMessage.Data.ContainsKey("body"))
            {
                body = remoteMessage.Data["body"];
            }
            if (remoteMessage.Data.ContainsKey("channel"))
            {
                channel = remoteMessage.Data["channel"];
            }

            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.RaiseNotificationReceived(new NotificationsClient.Model.Notification
            {
                Body = body,
                Title = title,
                Channel = channel
            });
        }

        private async Task SendRegistrationToServer(string token)
        {
            Exception hubError = null;
            var configClient = new ConfigurationClient();

            var client = (NotificationsServiceClient)SimpleIoc
                .Default
                .GetInstance<INotificationsServiceClient>();

            try
            {
                var hubConfig = await configClient.GetConfiguration(false);
                TryRegisterHub(hubConfig, client, token);
            }
            catch (NotificationHubResourceNotFoundException ex)
            {
                hubError = ex;
            }
            catch (Exception ex)
            {
                if (ex is Java.Lang.AssertionError)
                {
                    // Invalid connection string
                    hubError = ex;
                }
                else
                {
                    client.RaiseError(ex.Message);
                }
            }

            if (hubError != null)
            {
                try
                {
                    var hubConfig = await configClient.GetConfiguration(true);
                    TryRegisterHub(hubConfig, client, token);
                }
                catch (Exception ex)
                {
                    client.RaiseError(ex.Message);
                }
            }
        }

        private void TryRegisterHub(
            HubConfiguration config,
            NotificationsServiceClient client,
            string token)
        {
            var hub = new NotificationHub(
                config.HubName,
                config.HubConnectionString,
                this);

            // register device with Azure Notification Hub using the token from FCM
            var registration = hub.Register(token, Constants.NotificationHubTagName);

            // subscribe to the SubscriptionTags list with a simple template.
            var pnsHandle = registration.PNSHandle;
            var templateReg = hub.RegisterTemplate(
                pnsHandle,
                Constants.NotificationHubTemplateName,
                Template,
                Constants.NotificationHubTagName);

            client.RaiseStatusReady();
        }
    }
}