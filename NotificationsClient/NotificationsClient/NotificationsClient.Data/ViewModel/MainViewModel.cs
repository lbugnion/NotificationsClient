using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Helpers;
using NotificationsClient.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationsClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private SettingsViewModel SettingsVm => 
            SimpleIoc.Default.GetInstance<SettingsViewModel>();

        private ConfigurationClient ConfigClient =>
            SimpleIoc.Default.GetInstance<ConfigurationClient>();

        private INavigationService Nav => 
            SimpleIoc.Default.GetInstance<INavigationService>();

        private IDialogService Dialog =>
            SimpleIoc.Default.GetInstance<IDialogService>();

        private IDispatcherHelper Dispatcher =>
            SimpleIoc.Default.GetInstance<IDispatcherHelper>();

        private NotificationStorage Storage =>
            SimpleIoc.Default.GetInstance<NotificationStorage>();

        public async Task DeleteChannel(ChannelInfoViewModel channelInfo)
        {
            if (channelInfo.NumberOfNotifications == 0)
            {
                return;
            }

            if (!await Dialog.ShowMessage(
                "This is irreversible and will also delete all these notifications on other devices!!",
                "Are you sure?",
                "Yes",
                "No",
                null))
            {
                return;
            }

            if (channelInfo == _allNotifications)
            {
                _allNotifications.Clear();                
                
                while (Channels.Count > 1)
                {
                    Channels.Remove(Channels.Last());
                }
            }
            else
            {
                if (Channels.Contains(channelInfo))
                {
                    foreach (var notif in channelInfo.Model.Notifications)
                    {
                        _allNotifications.RemoveNotification(notif);
                    }

                    Channels.Remove(channelInfo);
                }
            }
        }

        private string _status = string.Empty;

        public ObservableCollection<ChannelInfoViewModel> Channels
        {
            get;
            private set;
        }

        private ChannelInfoViewModel _allNotifications;

        public string Status
        {
            get => _status;
            set => Set(() => Status, ref _status, value);
        }

        private RelayCommand<ChannelInfoViewModel> _navigateToChannelCommand;

        public RelayCommand<ChannelInfoViewModel> NavigateToChannelCommand
        {
            get => _navigateToChannelCommand
                ?? (_navigateToChannelCommand = new RelayCommand<ChannelInfoViewModel>(
                channel =>
                {
                    Nav.NavigateTo(ViewModelLocator.ChannelPageKey, channel);
                }));
        }

        public MainViewModel()
        {
            Channels = new ObservableCollection<ChannelInfoViewModel>();
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

                // TODO Load the Channels collection

            }
            catch (Exception ex)
            {
                ShowInfo($"Error when loading the notifications, try to synchronize ({ex.Message})");
            }

            Channels.Add(_allNotifications = new ChannelInfoViewModel(
                new ChannelInfo("All notifications")));

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

            Dispatcher.CheckBeginInvokeOnUI(() =>
            {
                var channel = Channels.FirstOrDefault(c => c.Model.ChannelName == notification.Channel);

                if (channel == null)
                {
                    channel = new ChannelInfoViewModel(new ChannelInfo(notification.Channel));
                    Channels.Add(channel);
                }

                _allNotifications.AddNotification(notification);
                channel.AddNotification(notification);
            });
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
