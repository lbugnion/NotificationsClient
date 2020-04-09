using Newtonsoft.Json;
using Notifications.Data;
using System.Net.Http;
using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    public class ConfigurationClient
    {
        private const string ConfigurationFunctionUrl = "https://notificationsendpoint.azurewebsites.net/api/config?code=anf5FFb16zHGybTZ95XQgjPvixzAQhdZQUHcY8r4J3vHHQl0pZVryQ==";

        // TODO Read from file
        private static string HubName;
        private static string HubConnectionString;

        public async Task<HubConfiguration> GetConfiguration(bool forceRefresh)
        {
            HubConfiguration config;

            if (forceRefresh
                || string.IsNullOrEmpty(HubName)
                || string.IsNullOrEmpty(HubConnectionString))
            {
                // TODO Pass key in header
                var client = new HttpClient();
                var json = await client.GetStringAsync(ConfigurationFunctionUrl);
                config = JsonConvert.DeserializeObject<HubConfiguration>(json);

                // TODO Save to file
                HubName = config.HubName;
                HubConnectionString = config.HubConnectionString;
            }
            else
            {
                config = new HubConfiguration
                {
                    HubName = HubName,
                    HubConnectionString = HubConnectionString
                };
            }

            return config;
        }
    }
}
