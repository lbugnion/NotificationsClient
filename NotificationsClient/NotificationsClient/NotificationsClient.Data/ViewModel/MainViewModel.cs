using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Helpers;
using NotificationsClient.Model;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NotificationsClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private Notification _lastNotification = null;

        private SettingsViewModel SettingsVm => 
            SimpleIoc.Default.GetInstance<SettingsViewModel>();

        private ConfigurationClient ConfigClient =>
            SimpleIoc.Default.GetInstance<ConfigurationClient>();

        private INavigationService Nav => 
            SimpleIoc.Default.GetInstance<INavigationService>();

        private IDispatcherHelper Dispatcher =>
            SimpleIoc.Default.GetInstance<IDispatcherHelper>();

        private NotificationStorage Storage =>
            SimpleIoc.Default.GetInstance<NotificationStorage>();

        public Notification LastNotification
        {
            get => _lastNotification;
            set => Set(() => LastNotification, ref _lastNotification, value);
        }

        private string _status = string.Empty;

        public string Status
        {
            get => _status;
            set => Set(() => Status, ref _status, value);
        }

        public async Task Initialize()
        {
            ShowInfo("Initializing...");            

            SettingsVm.Model.PropertyChanged -= SettingsPropertyChanged;
            SettingsVm.Model.PropertyChanged += SettingsPropertyChanged;

            if (string.IsNullOrEmpty(SettingsVm.Model.FunctionsAppName)
                || string.IsNullOrEmpty(SettingsVm.Model.FunctionCode))
            {
                Nav.NavigateTo(ViewModelLocator.SettingsPageKey);
                return;
            }

            // Initialize and load the database

            try
            {
                await Storage.InitializeAsync();
            }
            catch (Exception ex)
            {
                ShowInfo($"Error when loading the notifications, try to synchronize ({ex.Message})");
            }

            // Prepare to receive new notifications

            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.NotificationReceived -= ClientNotificationReceived;
            client.NotificationReceived += ClientNotificationReceived;
            client.ErrorHappened -= ClientErrorHappened;
            client.ErrorHappened += ClientErrorHappened;
            client.StatusChanged -= ClientStatusChanged;
            client.StatusChanged += ClientStatusChanged;

            if (SettingsVm.Model.IsRegisteredSuccessfully)
            {
                await client.Initialize(false);
                ShowInfo("Ready to receive notifications");
                return;
            }

            try
            {
                await ConfigClient.RefreshConfiguration();
                await client.Initialize(true);
            }
            catch (Exception ex)
            {
                SettingsVm.Model.IsRegisteredSuccessfully = false;
                ShowInfo($"Error when initializing: {ex.Message}", true);
            }
        }

        private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SettingsVm.Model.FunctionsAppName)
                || string.IsNullOrEmpty(SettingsVm.Model.FunctionCode))
            {
                return;
            }

            if (e.PropertyName == nameof(Settings.FunctionsAppName)
                || e.PropertyName == nameof(Settings.FunctionCode))
            {
                SettingsVm.Model.IsRegisteredSuccessfully = false;
            }
        }

        private void ClientStatusChanged(object sender, NotificationStatus e)
        {
            switch (e)
            {
                case NotificationStatus.Initializing:
                    ShowInfo("Initializing...");
                    break;

                case NotificationStatus.Ready:
                    ShowInfo("Ready to receive notifications");
                    break;
            }
        }

        private void ClientErrorHappened(object sender, string message)
        {
            ShowInfo(message, true);
        }

        private void ClientNotificationReceived(object sender, Notification notification)
        {
            ShowInfo($"Notification received at {notification.ReceivedTimeUtc}");
            LastNotification = notification;
        }

        // TODO Handle isError
        public void ShowInfo(string message, bool isError = false)
        {
            Dispatcher.CheckBeginInvokeOnUI(() =>
            {
                Status = message;
            });
        }
    }
}
