using GalaSoft.MvvmLight.Ioc;
using Microsoft.WindowsAzure.Messaging;
using Notifications;
using NotificationsClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.UI.Xaml;

// TODO Maybe we can rationalize this code with the other NotificationsServiceClient
namespace NotificationsClient.UWP.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        public event EventHandler<NotificationEventArgs> NotificationReceived;
        public event EventHandler<string> ErrorHappened;
        public event EventHandler<NotificationStatus> StatusChanged;

        private static readonly string Template = $"<toast activationType=\"foreground\" launch=\"$(argument)\"><visual><binding template=\"ToastGeneric\"><text id=\"1\">$({FunctionConstants.Title})</text><text id=\"2\">$({FunctionConstants.Body})</text></binding></visual></toast>";

        private Settings Settings =>
            SimpleIoc.Default.GetInstance<Settings>();

        private List<Notification> _delayedNotifications
            = new List<Notification>();

        public async Task Initialize(bool registerHub)
        {
            StatusChanged?.Invoke(this, NotificationStatus.Initializing);

            Exception hubError = null;
            var configClient = SimpleIoc.Default.GetInstance<ConfigurationClient>();
            PushNotificationChannel channel = null;

            try
            {
                channel = await PushNotificationChannelManager
                    .CreatePushNotificationChannelForApplicationAsync();

                channel.PushNotificationReceived += ChannelPushNotificationReceived;
            }
            catch (Exception ex)
            {
                ErrorHappened?.Invoke(this, $"Error when creating channel: {ex.Message}");
                return;
            }

            if (!registerHub)
            {
                StatusChanged?.Invoke(this, NotificationStatus.Ready);
                return;
            }

            try
            {
                var hubConfig = configClient.GetConfiguration();
                await TryRegisterHub(hubConfig, channel);
                Settings.IsRegisteredSuccessfully = true;
                StatusChanged?.Invoke(this, NotificationStatus.Ready);
            }
            catch (NotificationHubNotFoundException ex)
            {
                // Might need to refresh the information
                hubError = ex;
            }
            catch (RegistrationException ex)
            {
                hubError = ex;
            }
            catch (Exception ex)
            {
                ErrorHappened?.Invoke(this, ex.Message);
            }

            if (hubError != null)
            {
                try
                {
                    await configClient.RefreshConfiguration();
                    var hubConfig = configClient.GetConfiguration();
                    await TryRegisterHub(hubConfig, channel);
                    Settings.IsRegisteredSuccessfully = true;
                    StatusChanged?.Invoke(this, NotificationStatus.Ready);
                }
                catch (Exception ex)
                {
                    ErrorHappened?.Invoke(this, ex.Message);
                    Settings.IsRegisteredSuccessfully = false;
                }
            }
        }

        private async Task TryRegisterHub(
            HubConfiguration config,
            PushNotificationChannel channel)
        {
            var hub = new NotificationHub(
                config.HubName,
                config.HubConnectionString);

            await hub.RegisterNativeAsync(channel.Uri);
            await hub.RegisterTemplateAsync(
                channel.Uri,
                Template,
                Constants.NotificationHubTemplateName);
        }

        private void ChannelPushNotificationReceived(
            PushNotificationChannel sender, 
            PushNotificationReceivedEventArgs args)
        {
            if (args.NotificationType == PushNotificationType.Toast)
            {
                var toastNode = args
                    .ToastNotification
                    .Content
                    .FirstChild; // toast

                var launchAttribute = toastNode.Attributes.FirstOrDefault(a => a.NodeName == "launch");
                RaiseNotificationReceived(launchAttribute.NodeValue.ToString());

                var isVisible = ((App)Application.Current).IsWindowVisible;
                var isActivated = ((App)Application.Current).IsWindowActive;
                args.Cancel = isVisible && isActivated;
            }
        }

        public void RaiseNotificationReceived(Notification notification)
        {
            if (NotificationReceived == null)
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

        internal void RaiseNotificationReceived(string arguments)
        {
            var notification = Notification.Parse(arguments);

            if (notification != null)
            {
                RaiseNotificationReceived(notification);
            }
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