using Android.App;
using Android.Gms.Common;
using Android.OS;
using GalaSoft.MvvmLight.Ioc;
using Notifications;
using NotificationsClient.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowsAzure.Messaging;

namespace NotificationsClient.Droid.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        private static readonly string Template = $"{{\"notification\":{{\"{FunctionConstants.Body}\":\"$({FunctionConstants.Body})\",\"{FunctionConstants.Title}\":\"$({FunctionConstants.Title})\"}},\"data\":{{\"{FunctionConstants.UniqueId}\":\"$({FunctionConstants.UniqueId})\",\"{FunctionConstants.Title}\":\"$({FunctionConstants.Title})\",\"{FunctionConstants.Body}\":\"$({FunctionConstants.Body})\",\"{FunctionConstants.SentTimeUtc}\":\"$({FunctionConstants.SentTimeUtc})\",\"{FunctionConstants.Channel}\":\"$({FunctionConstants.Channel})\"}}}}";

        public event EventHandler<NotificationEventArgs> NotificationReceived;
        public event EventHandler<string> ErrorHappened;
        public event EventHandler<NotificationStatus> StatusChanged;

        private const string ChannelId = "NotificationsClient.Channel";
        public const int NotificationId = 100;

        private List<NotificationsClient.Model.Notification> _delayedNotifications 
            = new List<NotificationsClient.Model.Notification>();

        private Settings Settings =>
            SimpleIoc.Default.GetInstance<Settings>();

        internal FirebaseMessagingServiceEx FirebaseService 
        { 
            get; 
            set; 
        }

        private MainActivity _context;

        public NotificationsServiceClient(MainActivity activity)
        {
            _context = activity;
        }

        // This method doesn't need to be asynchronous for Android but
        // is anyway for compatibility with UWP
        public async Task Initialize(bool registerHub)
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

                // Check if token was already received
                if (!string.IsNullOrEmpty(Settings.Token)
                    && !Settings.IsRegisteredSuccessfully)
                {
                    // This cannot happen on the UI thread!!
                    await Task.Run(async () =>
                    {
                        await SendRegistrationToServer(Settings.Token);
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorHappened?.Invoke(this, ex.Message);
            }
        }

        public void RaiseNotificationReceived(
            NotificationsClient.Model.Notification notification,
            bool isDelayed)
        {
            if (isDelayed)
            {
                _delayedNotifications.Add(notification);
            }
            else
            {
                NotificationReceived?.Invoke(this, new NotificationEventArgs
                {
                    Notification = notification
                });
            }
        }

        internal void RaiseStatusReady()
        {
            StatusChanged?.Invoke(this, NotificationStatus.Ready);
        }

        internal void RaiseError(string errorMessage)
        {
            ErrorHappened?.Invoke(this, errorMessage);
        }


        public async Task SendRegistrationToServer(string token)
        {
            var client = (NotificationsServiceClient)SimpleIoc
                .Default
                .GetInstance<INotificationsServiceClient>();

            Exception hubError = null;
            var configClient = SimpleIoc.Default.GetInstance<ConfigurationClient>();

            try
            {
                var hubConfig = configClient.GetConfiguration();

                if (hubConfig == null)
                {
                    return;
                }

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
                MainActivity.Context);

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

        public void RaiseDelayedNotifications()
        {
            if (NotificationReceived == null)
            {
                return;
            }

            foreach (var notification in _delayedNotifications)
            {
                NotificationReceived(this, new NotificationEventArgs
                {
                    Notification = notification,
                    IsDelayed = true
                });
            }
        }
    }
}