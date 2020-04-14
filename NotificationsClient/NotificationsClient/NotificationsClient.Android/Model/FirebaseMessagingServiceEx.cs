using Android.App;
using Android.Content;
using Firebase.Messaging;
using GalaSoft.MvvmLight.Ioc;
using Notifications;
using NotificationsClient.Model;
using System;

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

            var uniqueId = string.Empty;
            var title = string.Empty;
            var body = string.Empty;
            var sentTimeUtc = string.Empty;
            var channel = string.Empty;

            if (remoteMessage.Data.ContainsKey(FunctionConstants.UniqueId))
            {
                uniqueId = remoteMessage.Data[FunctionConstants.UniqueId];
            }
            if (remoteMessage.Data.ContainsKey(FunctionConstants.Title))
            {
                title = remoteMessage.Data[FunctionConstants.Title];
            }
            if (remoteMessage.Data.ContainsKey(FunctionConstants.Body))
            {
                body = remoteMessage.Data[FunctionConstants.Body];
            }
            if (remoteMessage.Data.ContainsKey(FunctionConstants.SentTimeUtc))
            {
                sentTimeUtc = remoteMessage.Data[FunctionConstants.SentTimeUtc];
            }
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

            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.RaiseNotificationReceived(notification);
        }
    }
}