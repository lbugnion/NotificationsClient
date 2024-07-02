using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notifications.Endpoint.Model;
using System.Text;

namespace Notifications.Endpoint
{
    public class SendNotification
    {
        private const string GenericChannelId = "Notifications";
        private const string TelegramId = "TelegramId";
        private const string UserIdVariableName = "UserId";
        private const string TelegramUrl = "https://api.telegram.org/bot{0}/sendMessage";

        private readonly ILogger<SendNotification> _logger;

        public SendNotification(ILogger<SendNotification> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SendNotification))]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "post",
                Route ="send")] 
            HttpRequest req)
        {
            _logger.LogInformation("SendNotification was called");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string body = data.body;
            string title = data.title;
            string channel = data.channel ?? string.Empty;

            if (string.IsNullOrEmpty(body)
                || string.IsNullOrEmpty(title))
            {
                return new BadRequestObjectResult(
                    "Please pass a body and a title in the payload");
            }

            var genericChannelMapping = Environment.GetEnvironmentVariable($"{TelegramId}{GenericChannelId}");

            var channelMapping = string.IsNullOrEmpty(channel) ? genericChannelMapping : string.Empty;
            var channelInMessage = channel;
            
            // A channel mapping was passed in the request
            if (string.IsNullOrEmpty(channelMapping))
            {
                channelMapping = Environment.GetEnvironmentVariable($"{TelegramId}{channel}");
                channelInMessage = string.Empty;
            }

            // Custom channel mapping is not found
            if (string.IsNullOrEmpty(channelMapping))
            {
                channelMapping = genericChannelMapping;
                channelInMessage = channel;
            }

            if (string.IsNullOrEmpty(channelMapping))
            {
                return new BadRequestObjectResult($"Channel mapping cannot be found, make sure you define {TelegramId}{channel}");
            }

            var userId = Environment.GetEnvironmentVariable(UserIdVariableName);

            if (string.IsNullOrEmpty(userId))
            {
                return new BadRequestObjectResult($"User ID cannot be found, make sure you define UserIdVariableName");
            }

            var url = string.Format(TelegramUrl, channelMapping);

            var notification = new Notification
            {
                ChatId = userId,
                Title = title,
                Message = body,
                ChannelInMessage = channelInMessage
            };

            var json = JsonConvert.SerializeObject(notification);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    return new BadRequestObjectResult(
                        $"Error sending notification: {response.StatusCode}");
                }
            }

            return new OkObjectResult("Notification sent");
        }
    }
}
