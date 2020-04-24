using Microsoft.Azure.Cosmos.Table;
using System;

namespace Notifications.Model
{
    public class NotificationEntity : TableEntity
    {
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

        public LastOperation LastOperation
        {
            get;
            set;
        }

        public string LastOperationString
        {
            get => LastOperation.ToString();
            set => Enum.Parse(typeof(LastOperation), value);
        }

        public DateTime LastChangeUtc
        {
            get;
            set;
        }

        public NotificationEntity()
        {
            LastOperation = LastOperation.None;
            PartitionKey = FunctionConstants.PartitionKey;
            RowKey = Guid.NewGuid().ToString();
        }
    }
}
