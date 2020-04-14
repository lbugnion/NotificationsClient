using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Microsoft.Azure.NotificationHubs;

namespace Notifications
{
    public static class SendNotification
    {
        [FunctionName("SendNotification")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "post",
                Route = "send")] 
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SendNotification was called");

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

            var uniqueId = Guid.NewGuid().ToString();
            var sentTimeUtc = DateTime.UtcNow;
            var argument = FunctionConstants.UwpArgumentTemplate
                .Replace(FunctionConstants.UniqueId, uniqueId)
                .Replace(FunctionConstants.Title, title)
                .Replace(FunctionConstants.Body, body)
                .Replace(FunctionConstants.SentTimeUtc, sentTimeUtc.ToString(FunctionConstants.DateTimeFormat))
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
                    sentTimeUtc.ToString(FunctionConstants.DateTimeFormat)
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
                Notifications.Initialize(
                    Environment.GetEnvironmentVariable(
                        Notifications.ConnectionStringVariableName),
                    Environment.GetEnvironmentVariable(
                        Notifications.HubNameVariableName));

                var outcome = await Notifications.Instance.Hub
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

                log.LogInformation(result);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
