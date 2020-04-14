using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using Notifications;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    public class ConfigurationClient
    {
        private const string ConfigurationFunctionUrl = "https://{0}.azurewebsites.net/api/config";
        private const string HubConfigFileName = "hubconfig.json";

        private Settings Settings => SimpleIoc.Default.GetInstance<Settings>();

        public HubConfiguration GetConfiguration()
        {
            var configFilePath = Path.Combine(Settings.GetAppFolder().FullName, HubConfigFileName);

            HubConfiguration config = null;

            if (File.Exists(configFilePath))
            {
                var json = File.ReadAllText(configFilePath);
                config = JsonConvert.DeserializeObject<HubConfiguration>(json);
            }

            return config;
        }

        public async Task RefreshConfiguration()
        {
            var configFilePath = Path.Combine(Settings.GetAppFolder().FullName, HubConfigFileName);

            if (string.IsNullOrEmpty(configFilePath))
            {
                throw new InvalidOperationException("Please set the App Folder path first");
            }

            if (string.IsNullOrEmpty(Settings.FunctionCode)
                || string.IsNullOrEmpty(Settings.FunctionsAppName))
            {
                throw new InvalidOperationException(
                    "FunctionCode or FunctionsAppName are not defined in the ConfigurationClient");
            }

            if (File.Exists(configFilePath))
            {
                File.Delete(configFilePath);
            }

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

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Invalid response in ConfigurationClient with {Settings.FunctionsAppName} and {Settings.FunctionCode}");
            };

            var json = await response.Content.ReadAsStringAsync();
            File.WriteAllText(configFilePath, json);
        }
    }
}
