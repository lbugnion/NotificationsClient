using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Helpers;
using NotificationsClient.Model;
using System;

namespace NotificationsClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private Notification _lastNotification = null;

        private Settings Settings => 
            SimpleIoc.Default.GetInstance<Settings>();

        private INavigationService Nav => 
            SimpleIoc.Default.GetInstance<INavigationService>();

        private IDispatcherHelper Dispatcher =>
            SimpleIoc.Default.GetInstance<IDispatcherHelper>();

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

        public void Initialize()
        {
            if (string.IsNullOrEmpty(Settings.FunctionsAppName)
                || string.IsNullOrEmpty(Settings.FunctionCode))
            {
                Nav.NavigateTo(ViewModelLocator.SettingsPageKey);
                return;
            }

            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.NotificationReceived += ClientNotificationReceived;
            client.ErrorHappened += ClientErrorHappened;
            client.StatusChanged += ClientStatusChanged;
            client.Initialize();
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
            ShowInfo("Notification received at " + DateTime.Now);
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
