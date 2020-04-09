﻿using Microsoft.WindowsAzure.Messaging;
using Notifications.Data;
using NotificationsClient.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

namespace NotificationsClient.UWP.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        public event EventHandler<Notification> NotificationReceived;
        public event EventHandler<string> ErrorHappened;
        public event EventHandler<NotificationStatus> StatusChanged;

        private const string Separator = "|@|";

        private static readonly string Template = $"<toast activationType=\"foreground\" launch=\"$(argument)\"><visual><binding template=\"ToastGeneric\"><text id=\"1\">$(title)</text><text id=\"2\">$(body)</text></binding></visual></toast>";

        public async Task Initialize()
        {
            Exception hubError = null;
            var configClient = new ConfigurationClient();
            PushNotificationChannel channel = null;

            try
            {
                channel = await PushNotificationChannelManager
                    .CreatePushNotificationChannelForApplicationAsync();

                channel.PushNotificationReceived += ChannelPushNotificationReceived;

                var hubConfig = await configClient.GetConfiguration(false);
                await TryRegisterHub(hubConfig, channel);
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
                    var hubConfig = await configClient.GetConfiguration(true);
                    await TryRegisterHub(hubConfig, channel);
                }
                catch (Exception ex)
                {
                    ErrorHappened?.Invoke(this, ex.Message);
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

            StatusChanged?.Invoke(this, NotificationStatus.Ready);
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
            }
        }

        public void RaiseNotificationReceived(Notification notification)
        {
            NotificationReceived?.Invoke(this, notification);
        }

        internal void RaiseNotificationReceived(string arguments)
        {
            var notificationParts = arguments.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

            if (notificationParts.Length == 3)
            {
                RaiseNotificationReceived(new Notification
                {
                    Title = notificationParts[0],
                    Body = notificationParts[1],
                    Channel = notificationParts[2]
                });
            }
        }
    }
}