namespace Notifications
{
    public class FunctionConstants
    {
        public const string UwpArgumentSeparator = "|@|";

        public const string UniqueId = "uniqueid";
        public const string Title = "title";
        public const string Body = "body";
        public const string SentTimeUtc = "senttimeutc";
        public const string Channel = "channel";
        public const string Argument = "argument";

        public static readonly string UwpArgumentTemplate = $"{UniqueId}{UwpArgumentSeparator}{Title}{UwpArgumentSeparator}{Body}{UwpArgumentSeparator}{SentTimeUtc}{UwpArgumentSeparator}{Channel}";
        public const int UwpArgumentTemplateParts = 5;
        public const string DateTimeFormat = "yyyyMMddHHmmss";

        public const string PartitionKey = "partition";
        public const string NotificationTableName = "notifications";
    }
}
