using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notifications.Data;
using System.Net.Http;

namespace NotificationsClient.Endpoint
{
    public static class GetConfiguration
    {
        [FunctionName("GetConfiguration")]
        public static IActionResult Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get",
                Route = "config")] 
            HttpRequest req,
            ILogger log)
        {
            // TODO Add error handling

            log.LogInformation("GetConfiguration was called");

            var config = new HubConfiguration
            {
                HubName = Environment.GetEnvironmentVariable(Notifications.HubNameVariableName),
                HubConnectionString = Environment.GetEnvironmentVariable(Notifications.ConnectionStringVariableName)
            };

            return new JsonResult(config);
        }
    }
}
