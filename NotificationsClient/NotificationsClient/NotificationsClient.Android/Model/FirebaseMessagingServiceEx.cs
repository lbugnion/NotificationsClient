using Android.App;
using Android.Content;
using Firebase.Messaging;
using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Model;
using System;
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

            var message = string.Empty;

            foreach (var key in remoteMessage.Data.Keys)
            {
                var value = remoteMessage.Data[key];
                message += $"{key}:{value} |";
            }

            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.RaiseNotificationReceived(message);
        }

        private void SendRegistrationToServer(string token)
        {
            try
            {
                var hub = new NotificationHub(Constants.NotificationHubName, Constants.NotificationHubConnectionString, this);

                // register device with Azure Notification Hub using the token from FCM
                var registration = hub.Register(token, Constants.NotificationHubTagName);

                // subscribe to the SubscriptionTags list with a simple template.
                var pnsHandle = registration.PNSHandle;
                var templateReg = hub.RegisterTemplate(
                    pnsHandle,
                    Constants.NotificationHubTemplateName, 
                    Template,
                    Constants.NotificationHubTagName);
            }
            catch (Exception e)
            {
            }
        }
    }
}