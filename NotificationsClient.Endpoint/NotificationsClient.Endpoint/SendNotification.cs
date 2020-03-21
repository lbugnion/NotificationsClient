using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

            return new OkObjectResult($"Title: {title} | Message: {message}");
        }
    }
}
