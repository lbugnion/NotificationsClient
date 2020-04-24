using Microsoft.Azure.Cosmos.Table;
using System;

namespace Notifications
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

        public NotificationEntity()
        {
            PartitionKey = FunctionConstants.PartitionKey;
            RowKey = Guid.NewGuid().ToString();
        }
    }
}
