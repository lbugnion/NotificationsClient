using Android.App;
using Android.Content;
using Firebase.Messaging;
using GalaSoft.MvvmLight.Ioc;
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
            var client = (NotificationsServiceClient)SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            await client.SendRegistrationToServer(token);
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
    }
}