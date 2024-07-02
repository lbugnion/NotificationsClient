#if DEBUG

using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Helpers;
using NotificationsClient.Model;
using System;

namespace NotificationsClient
{
    public static class DesignDataGenerator
    {
        private static Random _random = new Random();

        public static Notification GetRandomNotification(string channel)
        {
            return new Notification
            {
                Title = "This is a random notification",
                Body = $"Random body {Guid.NewGuid()}",
                SentTimeUtc = DateTime.UtcNow - TimeSpan.FromHours(1),
                Channel = channel,
                UniqueId = Guid.NewGuid().ToString(),
                ReceivedTimeUtc = DateTime.Now - TimeSpan.FromMinutes(_random.NextDouble() * 14400)
            };
        }

        public static void SaveRandomNotifications(int numberOfNotifications, int numberOfGroups)
        {
            var storage = SimpleIoc.Default.GetInstance<NotificationStorage>();

            var groupIndex = 1;
            var saveChannel = true;

            for (var index = 0; index < numberOfNotifications; index++)
            {
                if (saveChannel)
                {
                    var currentChannel = new ChannelInfo
                    {
                        ChannelName = "Channel " + groupIndex
                    };

                    storage.SaveChannelInfo(currentChannel);
                }

                var notification = GetRandomNotification("Channel " + groupIndex);
                storage.SaveNotification(notification);

                if (++groupIndex >= numberOfGroups)
                {
                    groupIndex = 1;
                    saveChannel = false;
                }
            }
        }
    }
}

#endif