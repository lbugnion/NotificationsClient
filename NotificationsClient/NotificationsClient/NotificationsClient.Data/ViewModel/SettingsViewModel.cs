using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using NotificationsClient.Model;
using System;
using System.IO;

namespace NotificationsClient.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private const string AppFolderName = "GalaSoft.NotificationsClient";
        private const string SettingsFileName = "settings.json";

        private string _settingsFilePath;

        public Settings Model
        {
            get => SimpleIoc.Default.GetInstance<Settings>();
        }

        static SettingsViewModel()
        {
            SimpleIoc.Default.Register<Settings>();
        }

        public void LoadSettings()
        {
            _settingsFilePath = Path.Combine(
                GetAppFolder().FullName,
                SettingsFileName);

            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonConvert.DeserializeObject<Settings>(json);

                Model.Set(settings);
            }

            Model.PropertyChanged += ModelPropertyChanged;

            // TODO Remove when we have sorted out the first navigation
            //Model.FunctionCode = "anf5FFb16zHGybTZ95XQgjPvixzAQhdZQUHcY8r4J3vHHQl0pZVryQ==";
            //Model.FunctionsAppName = "notificationsendpoint";
        }

        private void ModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            var json = JsonConvert.SerializeObject(Model);
            File.WriteAllText(_settingsFilePath, json);
        }

        public DirectoryInfo GetAppFolder()
        {
            var rootFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var configFolder = new DirectoryInfo(Path.Combine(rootFolderPath, AppFolderName));

            if (!configFolder.Exists)
            {
                configFolder.Create();
            }

            return configFolder;
        }
    }
}
