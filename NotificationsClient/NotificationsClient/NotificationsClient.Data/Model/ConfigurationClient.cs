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

        private string _configFilePath;
        private string _functionsAppName;
        private string _functionCode;

        public void SetVariables(
            DirectoryInfo appFolder,
            string functionsAppName,
            string functionCode)
        {
            _configFilePath = Path.Combine(appFolder.FullName, HubConfigFileName);
            _functionsAppName = functionsAppName;
            _functionCode = functionCode;
        }

        public HubConfiguration GetConfiguration()
        {
            if (string.IsNullOrEmpty(_configFilePath))
            {
                throw new InvalidOperationException("Please set the App Folder path first");
            }

            HubConfiguration config = null;

            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                config = JsonConvert.DeserializeObject<HubConfiguration>(json);
            }

            return config;
        }

        public async Task RefreshConfiguration()
        {
            if (string.IsNullOrEmpty(_configFilePath))
            {
                throw new InvalidOperationException("Please set the App Folder path first");
            }

            if (string.IsNullOrEmpty(_functionCode)
                || string.IsNullOrEmpty(_functionsAppName))
            {
                throw new InvalidOperationException(
                    "FunctionCode or FunctionsAppName are not defined in the ConfigurationClient");
            }

            if (File.Exists(_configFilePath))
            {
                File.Delete(_configFilePath);
            }

            var client = new HttpClient();

            var url = string.Format(
                ConfigurationFunctionUrl,
                _functionsAppName);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                url);

            request.Headers.Add(
                "x-functions-key",
                _functionCode);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Invalid response in ConfigurationClient with {_functionsAppName} and {_functionCode}");
            };

            var json = await response.Content.ReadAsStringAsync();
            File.WriteAllText(_configFilePath, json);
        }
    }
}
