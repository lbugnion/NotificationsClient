using Newtonsoft.Json;
using Notifications.Data;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    public class ConfigurationClient
    {
        private const string ConfigurationFunctionUrl = "https://notificationsendpoint.azurewebsites.net/api/config?code=anf5FFb16zHGybTZ95XQgjPvixzAQhdZQUHcY8r4J3vHHQl0pZVryQ==";
        public const string ConfigFileName = "config.json";

        public async Task<HubConfiguration> GetConfiguration(bool forceRefresh)
        {
            var rootFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var configFolder = new DirectoryInfo(Path.Combine(rootFolderPath, "GalaSoft.NotificationsClient"));

            if (!configFolder.Exists)
            {
                configFolder.Create();
            }

            var configFilePath = Path.Combine(configFolder.FullName, ConfigFileName);

            HubConfiguration config = null;

            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<HubConfiguration>(json);
            }

            if (forceRefresh
                || config == null)
            {
                // TODO Pass key in header
                var client = new HttpClient();
                var json = await client.GetStringAsync(ConfigurationFunctionUrl);
                config = JsonConvert.DeserializeObject<HubConfiguration>(json);

                File.WriteAllText(configFilePath, json);
            }

            return config;
        }
    }
}
