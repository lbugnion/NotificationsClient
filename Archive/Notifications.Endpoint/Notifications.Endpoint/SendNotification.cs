using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notifications.Model;

namespace Notifications.Endpoint
{
    public class SendNotification
    {
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
                Route = "send")] 
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
                    "Please pass a body and a title in the request");
            }

            var sentTimeUtc = DateTime.UtcNow;
            var uniqueId = Guid.NewGuid().ToString();

            var sentTimeUtcString = sentTimeUtc.ToString(
                FunctionConstants.DateTimeFormat);

            var argument = FunctionConstants.UwpArgumentTemplate
                .Replace(FunctionConstants.UniqueId, uniqueId)
                .Replace(FunctionConstants.Title, title)
                .Replace(FunctionConstants.Body, body)
                .Replace(FunctionConstants.SentTimeUtc, sentTimeUtcString)
                .Replace(FunctionConstants.Channel, channel);

            var properties = new Dictionary<string, string>
            {
                {
                    FunctionConstants.UniqueId,
                    uniqueId
                },
                {
                    FunctionConstants.Title,
                    title
                },
                {
                    FunctionConstants.Body,
                    body
                },
                {
                    FunctionConstants.SentTimeUtc,
                    sentTimeUtcString
                },
                {
                    FunctionConstants.Argument,
                    argument
                }
            };

            if (!string.IsNullOrEmpty(channel))
            {
                properties.Add(FunctionConstants.Channel, channel);
            }

            try
            {
                Model.Notifications.Initialize(
                    Environment.GetEnvironmentVariable(
                        Model.Notifications.ConnectionStringVariableName),
                    Environment.GetEnvironmentVariable(
                        Model.Notifications.HubNameVariableName));

                var outcome = await Model.Notifications.Instance.Hub
                    .SendTemplateNotificationAsync(properties);

                var result = string.Empty;

                if (outcome.State == NotificationOutcomeState.Completed)
                {
                    if (outcome.Success > 0)
                    {
                        result = $"Sent notification to {outcome.Success} devices";
                    }
                    else
                    {
                        result = "Notification was sent to 0 device";
                    }
                }
                else if (outcome.State == NotificationOutcomeState.Enqueued)
                {
                    result = "Notification enqueued for sending";
                }
                else
                {
                    result = "Couldn't complete the operation";
                }

                _logger.LogInformation(result);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
