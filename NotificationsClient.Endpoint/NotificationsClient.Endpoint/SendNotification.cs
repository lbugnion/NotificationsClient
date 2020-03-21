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

namespace NotificationsClient.Endpoint
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
            log.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string message = data.message;
            string title = data.title;
            string channel = data.channel;

            if (string.IsNullOrEmpty(message)
                || string.IsNullOrEmpty(title))
            {
                return new BadRequestObjectResult("Please pass a message and a title in the request body");
            }

            var properties = new Dictionary<string, string>
            {
                {
                    "message",
                    message
                },
                {
                    "title",
                    title
                }
            };

            if (!string.IsNullOrEmpty(channel))
            {
                properties.Add("channel", channel);
            }

            try
            {
                var outcome = await Notifications.Instance.Hub.SendTemplateNotificationAsync(properties);

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

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
