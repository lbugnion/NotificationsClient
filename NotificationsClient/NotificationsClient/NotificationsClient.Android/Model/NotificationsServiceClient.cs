using Android.App;
using Android.Gms.Common;
using Android.OS;
using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Model;
using System;
using System.Threading.Tasks;

namespace NotificationsClient.Droid.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        public event EventHandler<string> NotificationReceived;

        private const string ChannelId = "NotificationsClient.Channel";
        public const int NotificationId = 100;
        
        private MainActivity _context;

        public NotificationsServiceClient(MainActivity activity)
        {
            _context = activity;
        }

        // This method doesn't need to be asynchronous for Android but
        // is anyway for compatibility with UWP
        public async Task Initialize()
        {
            var resultCode = GoogleApiAvailability
                .Instance
                .IsGooglePlayServicesAvailable(_context);

            string errorMessage = null;
            var messageHandler = SimpleIoc.Default.GetInstance<IMessageHandler>();

            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability
                    .Instance
                    .IsUserResolvableError(resultCode))
                {
                    errorMessage = GoogleApiAvailability
                        .Instance
                        .GetErrorString(resultCode);
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    messageHandler.ShowError(errorMessage);
                }
                else
                {
                    messageHandler.ShowError("Unknown error when checking for service availability");
                }

                return;
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            try
            {
                var channel = new NotificationChannel(
                    ChannelId,
                    "Notifications for GalaSoft NotificationsClient",
                    NotificationImportance.Default);

                var notificationManager = (NotificationManager)_context.GetSystemService(
                    Android.Content.Context.NotificationService);

                notificationManager.CreateNotificationChannel(channel);

                messageHandler.ShowInfo("Ready to receive notifications");
            }
            catch (Exception ex)
            {
                messageHandler.ShowError(ex.Message);
            }
        }

        public void RaiseNotificationReceived(string message)
        {
            NotificationReceived?.Invoke(this, message);
        }
    }
}