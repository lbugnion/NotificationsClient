#if DEBUG

using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Model;
using System;

namespace NotificationsClient
{
    public static class DesignDataGenerator
    {
        public static Notification GetRandomNotification(string channel)
        {
            return new Notification
            {
                Title = "This is a random notification",
                Body = $"Random body {Guid.NewGuid()}",
                SentTimeUtc = DateTime.UtcNow - TimeSpan.FromHours(1),
                Channel = channel,
                UniqueId = Guid.NewGuid().ToString()
            };
        }

        public static void SendRandomNotifications(int numberOfNotifications, int groupSize)
        {
            var groupIndex = 0;
            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();

            for (var index = 0; index < numberOfNotifications; index++)
            {
                if (++groupIndex >= groupSize)
                {
                    groupIndex = 0;
                }

                var channel = $"Channel {groupIndex}";
                var notification = GetRandomNotification(channel);
                client.RaiseNotificationReceived(notification);
            }
        }
    }
}

#endif