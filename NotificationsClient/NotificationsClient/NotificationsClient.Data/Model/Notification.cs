using Newtonsoft.Json;
using Notifications;
using SQLite;
using System;
using System.Globalization;

namespace NotificationsClient.Model
{
    public class Notification
    {
        [JsonIgnore]
        [PrimaryKey, AutoIncrement]
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

        public string Unusual
        {
            get;
            set;
        }

        public static Notification Parse(string argument)
        {
            var notificationParts = argument.Split(
                new[] 
                {
                    FunctionConstants.UwpArgumentSeparator
                },
                StringSplitOptions.RemoveEmptyEntries);

            var notification = new Notification();

            if (notificationParts.Length < FunctionConstants.UwpArgumentTemplateParts - 1)
            {
                notification.Unusual = argument;
            }
            else
            {
                DateTime sentTimeUtc;
                var success = DateTime.TryParseExact(
                    notificationParts[(int)UwpArgumentsParts.SentTimeUtc],
                    FunctionConstants.DateTimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out sentTimeUtc);

                if (success)
                {
                    notification.UniqueId = notificationParts[(int)UwpArgumentsParts.UniqueId];
                    notification.Title = notificationParts[(int)UwpArgumentsParts.Title];
                    notification.Body = notificationParts[(int)UwpArgumentsParts.Body];
                    notification.SentTimeUtc = sentTimeUtc;
                }
                else
                {
                    notification.Unusual = argument;
                }
            }

            if (notificationParts.Length > FunctionConstants.UwpArgumentTemplateParts - 1)
            {
                notification.Channel = notificationParts[(int)UwpArgumentsParts.Channel];
            }

            notification.ReceivedTimeUtc = DateTime.UtcNow;
            return notification;
        }
    }
}
