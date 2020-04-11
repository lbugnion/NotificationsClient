using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using NotificationsClient.Model;
using System.IO;

namespace NotificationsClient.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private const string SettingsFileName = "settings.json";

        private static SettingsViewModel _instance;

        private string _functionCode = string.Empty;
        private string _functionsAppName = string.Empty;

        public static SettingsViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingsViewModel();
                    _instance.Initialize();
                }

                return _instance;
            }
        }

        public string FunctionCode
        {
            get => _functionCode;
            set 
            { 
                if (Set(() => FunctionCode, ref _functionCode, value))
                {
                    _settings.FunctionCode = _functionCode;
                    Save();
                }                
            }
        }

        public string FunctionsAppName
        {
            get => _functionsAppName;
            set
            {
                if (Set(() => FunctionsAppName, ref _functionsAppName, value))
                {
                    _settings.FunctionsAppName = _functionsAppName;
                    Save();
                }
            }
        }

        private Settings _settings;
        private string _settingsFilePath;

        private void Initialize()
        {
            _settingsFilePath = Path.Combine(
                ConfigurationClient.GetConfigurationFolder().FullName,
                SettingsFileName);

            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                _settings = JsonConvert.DeserializeObject<Settings>(json);

                FunctionCode = _settings.FunctionCode;
                FunctionsAppName = _settings.FunctionsAppName;
            }
            else
            {
                _settings = new Settings();
            }

            // TODO Remove when we have sorted out the first navigation
            _settings.FunctionCode = "anf5FFb16zHGybTZ95XQgjPvixzAQhdZQUHcY8r4J3vHHQl0pZVryQ==";
            _settings.FunctionsAppName = "notificationsendpoint";
            FunctionCode = _settings.FunctionCode;
            FunctionsAppName = _settings.FunctionsAppName;
        }

        private void Save()
        {
            var json = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
