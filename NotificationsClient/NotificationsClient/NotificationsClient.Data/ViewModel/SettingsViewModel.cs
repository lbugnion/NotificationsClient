using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using NotificationsClient.Model;
using System.IO;

namespace NotificationsClient.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private const string SettingsFileName = "settings.json";

        private string _settingsFilePath;

        public MainViewModel Main => SimpleIoc.Default.GetInstance<MainViewModel>();
        public Settings Model => SimpleIoc.Default.GetInstance<Settings>();

        static SettingsViewModel()
        {
            SimpleIoc.Default.Register<Settings>();
        }

        public void LoadSettings()
        {
            _settingsFilePath = Path.Combine(
                Model.GetAppFolder().FullName,
                SettingsFileName);

            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonConvert.DeserializeObject<Settings>(json);

                Model.Set(settings);
            }

            Model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(
            object sender, 
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            var json = JsonConvert.SerializeObject(Model);
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
