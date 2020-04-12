using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using Notifications.Data;
using NotificationsClient.ViewModel;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    public class ConfigurationClient
    {
        private const string ConfigurationFunctionUrl = "https://{0}.azurewebsites.net/api/config";
        private const string ConfigFileName = "config.json";
        private const string ConfigurationFolderName = "GalaSoft.NotificationsClient";

        private Settings Settings => SimpleIoc.Default.GetInstance<Settings>();

        public static DirectoryInfo GetConfigurationFolder()
        {
            var rootFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var configFolder = new DirectoryInfo(Path.Combine(rootFolderPath, ConfigurationFolderName));

            if (!configFolder.Exists)
            {
                configFolder.Create();
            }

            return configFolder;
        }

        public async Task<HubConfiguration> GetConfiguration(bool forceRefresh)
        {
            if (string.IsNullOrEmpty(Settings.FunctionCode)
                || string.IsNullOrEmpty(Settings.FunctionsAppName))
            {
                throw new InvalidOperationException(
                    "FunctionCode or FunctionsAppName are not defined in the settings");
            }

            var configFilePath = Path.Combine(
                GetConfigurationFolder().FullName, 
                ConfigFileName);

            HubConfiguration config = null;

            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<HubConfiguration>(json);
            }

            if (forceRefresh
                || config == null)
            {
                var client = new HttpClient();

                var url = string.Format(
                    ConfigurationFunctionUrl,
                    Settings.FunctionsAppName);

                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    url);

                request.Headers.Add(
                    "x-functions-key", 
                    Settings.FunctionCode);

                var response = await client.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();
                config = JsonConvert.DeserializeObject<HubConfiguration>(json);
                File.WriteAllText(configFilePath, json);
            }

            return config;
        }
    }
}
