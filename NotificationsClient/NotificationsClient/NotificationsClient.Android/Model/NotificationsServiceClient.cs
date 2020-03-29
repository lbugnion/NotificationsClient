using Android.App;
using Android.Gms.Common;
using Android.OS;
using NotificationsClient.Model;
using System;
using System.Threading.Tasks;

namespace NotificationsClient.Droid.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        public event EventHandler<string> NotificationReceived;
        public event EventHandler<string> ErrorHappened;
        public event EventHandler<NotificationStatus> StatusChanged;

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
                    ErrorHappened?.Invoke(this, errorMessage);
                }
                else
                {
                    ErrorHappened?.Invoke(this, "Unknown error when checking for service availability");
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

                StatusChanged?.Invoke(this, NotificationStatus.Initializing);
            }
            catch (Exception ex)
            {
                ErrorHappened?.Invoke(this,ex.Message);
            }
        }

        public void RaiseNotificationReceived(string message)
        {
            NotificationReceived?.Invoke(this, message);
        }

        internal void RaiseStatusReady()
        {
            StatusChanged?.Invoke(this, NotificationStatus.Ready);
        }

        internal void RaiseError(string errorMessage)
        {
            ErrorHappened?.Invoke(this, errorMessage);
        }
    }
}