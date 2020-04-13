using Android.App;
using Android.Content;
using Firebase.Messaging;
using GalaSoft.MvvmLight.Ioc;
using Notifications;
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

        private Settings Settings =>
            SimpleIoc.Default.GetInstance<Settings>();

        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);

            // Try to register now
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

        internal async Task SendRegistrationToServer(string token)
        {
            var client = (NotificationsServiceClient)SimpleIoc
                .Default
                .GetInstance<INotificationsServiceClient>();

            if (string.IsNullOrEmpty(Settings.FunctionsAppName)
                || string.IsNullOrEmpty(Settings.FunctionCode))
            {
                // Save token for later
                Settings.Token = token;
                client.FirebaseService = this;
                return;
            }

            Exception hubError = null;
            var configClient = SimpleIoc.Default.GetInstance<ConfigurationClient>();

            try
            {
                var hubConfig = configClient.GetConfiguration();
                TryRegisterHub(hubConfig, client, token);
                Settings.IsRegisteredSuccessfully = true;
            }
            catch (NotificationHubResourceNotFoundException ex)
            {
                // Invalid name
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
                    await configClient.RefreshConfiguration();
                    var hubConfig = configClient.GetConfiguration();
                    TryRegisterHub(hubConfig, client, token);
                    Settings.IsRegisteredSuccessfully = true;
                }
                catch (Exception ex)
                {
                    Settings.IsRegisteredSuccessfully = false;
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