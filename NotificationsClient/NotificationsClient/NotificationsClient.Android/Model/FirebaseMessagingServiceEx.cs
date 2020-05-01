using Android.App;
using Android.Content;
using Firebase.Messaging;
using GalaSoft.MvvmLight.Ioc;
using Notifications;
using NotificationsClient.Model;

namespace NotificationsClient.Droid.Model
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseMessagingServiceEx : FirebaseMessagingService
    {
        private Settings Settings =>
            SimpleIoc.Default.GetInstance<Settings>();

        public override async void OnNewToken(string token)
        {
            base.OnNewToken(token);

            Settings.Token = token;
            Settings.IsRegisteredSuccessfully = false;

            // Try to register now
            var client = (NotificationsServiceClient)SimpleIoc
                .Default
                .GetInstance<INotificationsServiceClient>();

            await client.SendRegistrationToServer(token);
        }

        public override void OnMessageReceived(RemoteMessage remoteMessage)
        {
            base.OnMessageReceived(remoteMessage);

            if (!remoteMessage.Data.ContainsKey(FunctionConstants.UniqueId)
                || !remoteMessage.Data.ContainsKey(FunctionConstants.Title)
                || !remoteMessage.Data.ContainsKey(FunctionConstants.Body)
                || !remoteMessage.Data.ContainsKey(FunctionConstants.SentTimeUtc))
            {
                // Invalid notification received
                return;
            }

            var uniqueId = remoteMessage.Data[FunctionConstants.UniqueId];
            var title = remoteMessage.Data[FunctionConstants.Title];
            var body = remoteMessage.Data[FunctionConstants.Body];
            var sentTimeUtc = remoteMessage.Data[FunctionConstants.SentTimeUtc];
            var channel = string.Empty;

            if (remoteMessage.Data.ContainsKey(FunctionConstants.Channel))
            {
                channel = remoteMessage.Data[FunctionConstants.Channel];
            }
            
            var argument = FunctionConstants.UwpArgumentTemplate
                .Replace(FunctionConstants.UniqueId, uniqueId)
                .Replace(FunctionConstants.Title, title)
                .Replace(FunctionConstants.Body, body)
                .Replace(FunctionConstants.SentTimeUtc, sentTimeUtc)
                .Replace(FunctionConstants.Channel, channel);

            var notification = NotificationsClient.Model.Notification.Parse(argument);

            if (notification != null)
            {
                var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
                client.RaiseNotificationReceived(notification);
            }
        }
    }
}