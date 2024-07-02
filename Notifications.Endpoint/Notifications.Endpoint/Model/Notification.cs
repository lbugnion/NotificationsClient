using Newtonsoft.Json;

namespace Notifications.Endpoint.Model
{
    public class Notification
    {
        private const string ParseModeMarkdown = "MarkdownV2";

        [JsonProperty("chat_id")]
        public string ChatId { get; set; }

        [JsonProperty("text")]
        public string Text
        {
            get
            {
                var title = Title.PrepareForChatBot();
                var message = Message.PrepareForChatBot();

                if (string.IsNullOrEmpty(ChannelInMessage))
                {
                    return $"*{title}*\n{message}\n\-\-\-\-";
                }

                return $"> {ChannelInMessage}\n*{title}*\n{message}\n\-\-\-\-";
            }
        }

        [JsonProperty("parse_mode")]
        public string ParseMode => ParseModeMarkdown;

        [JsonIgnore]
        public string ChannelInMessage { get; set; }

        [JsonIgnore]
        public string Title { get; set; }

        [JsonIgnore]
        public string Message { get; set; }
    }
}
