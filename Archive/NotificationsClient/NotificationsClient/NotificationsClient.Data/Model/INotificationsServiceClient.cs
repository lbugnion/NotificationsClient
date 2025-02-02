﻿using System;
using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    public interface INotificationsServiceClient
    {
        event EventHandler<NotificationEventArgs> NotificationReceived;
        event EventHandler<string> ErrorHappened;
        event EventHandler<NotificationStatus> StatusChanged;

        Task Initialize(bool registerHub);
        void RaiseNotificationReceived(Notification notification);
        void RaiseDelayedNotifications();
    }
}
