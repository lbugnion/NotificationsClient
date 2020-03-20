using Android.App;
using Android.Gms.Common;
using Android.OS;
using Firebase.Iid;
using NotificationsClient.Model;
using System.Diagnostics;

namespace NotificationsClient.Droid.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        private const string ChannelId = "NotificationsClient.Channel";
        public const int NotificationId = 100;
        
        private MainActivity _context;

        public NotificationsServiceClient(MainActivity activity)
        {
            _context = activity;
        }

        public (bool result, string errorMessage) AreOnlineServicesAvailable()
        {
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(_context);
            string errorMessage = null;

            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    errorMessage = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                }
            }

            return (resultCode == ConnectionResult.Success, errorMessage);
        }

        public void Initialize()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(
                ChannelId,
                "Notifications for GalaSoft NotificationsClient",
                NotificationImportance.Default);

            var notificationManager = (NotificationManager)_context.GetSystemService(
                Android.Content.Context.NotificationService);

            notificationManager.CreateNotificationChannel(channel);
        }
    }
}