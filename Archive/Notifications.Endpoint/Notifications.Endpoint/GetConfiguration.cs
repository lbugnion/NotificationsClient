using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Notifications.Model;

namespace Notifications.Endpoint
{
    public class GetConfiguration
    {
        private readonly ILogger<GetConfiguration> _logger;

        public GetConfiguration(ILogger<GetConfiguration> logger)
        {
            _logger = logger;
        }

        [Function(nameof(GetConfiguration))]
        public IActionResult Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "get",
            Route = "config")] 
            HttpRequest req)
        {
            _logger.LogInformation("GetConfiguration was called");

            var config = new HubConfiguration
            {
                HubName = Environment.GetEnvironmentVariable(Model.Notifications.HubNameVariableName),
                HubConnectionString = Environment.GetEnvironmentVariable(Model.Notifications.ConnectionStringVariableName)
            };

            return new JsonResult(config);
        }
    }
}
