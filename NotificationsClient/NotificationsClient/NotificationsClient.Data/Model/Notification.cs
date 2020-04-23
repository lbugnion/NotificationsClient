using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Notifications;
using System;
using System.Globalization;

namespace NotificationsClient.Model
{
    public class Notification : ObservableObject
    {
        [JsonIgnore]
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id
        {
            get;
            set;
        }

        public string UniqueId
        {
            get;
            set;
        }

        public DateTime ReceivedTimeUtc
        {
            get;
            set;
        }

        public DateTime SentTimeUtc
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string Body
        {
            get;
            set;
        }

        public string Channel
        {
            get;
            set;
        }

        private bool _isUnread;

        public bool IsUnread
        {
            get => _isUnread;
            set => Set(ref _isUnread, value);
        }

        public static Notification Parse(string argument)
        {
            var notificationParts = argument.Split(
                new[] 
                {
                    FunctionConstants.UwpArgumentSeparator
                },
                StringSplitOptions.RemoveEmptyEntries);

            if (notificationParts.Length < FunctionConstants.UwpArgumentTemplateParts - 2)
            {
                return null;
            }

            var notification = new Notification();

            var success = DateTime.TryParseExact(
                notificationParts[(int)UwpArgumentsParts.SentTimeUtc],
                FunctionConstants.DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out DateTime sentTimeUtc);

            if (success)
            {
                notification.UniqueId = notificationParts[(int)UwpArgumentsParts.UniqueId];
                notification.Title = notificationParts[(int)UwpArgumentsParts.Title];
                notification.Body = notificationParts[(int)UwpArgumentsParts.Body];
                notification.SentTimeUtc = sentTimeUtc;
            }
            else
            {
                // Invalid notification
                return null;
            }

            if (string.IsNullOrEmpty(notification.UniqueId)
                || string.IsNullOrEmpty(notification.Title)
                || string.IsNullOrEmpty(notification.Body))
            {
                // Invalid notification
                return null;
            }

            if (notificationParts.Length > FunctionConstants.UwpArgumentTemplateParts - 1)
            {
                notification.Channel = notificationParts[(int)UwpArgumentsParts.Channel];
            }

            return notification;
        }
    }
}
