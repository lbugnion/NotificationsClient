using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Helpers;
using NotificationsClient.Model;
using NotificationsClient.Resources;
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
                Texts.DeleteNotificationWarningMessage,
                Texts.AreYouSure,
                Texts.Yes,
                Texts.No,
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

            ShowInfo(Texts.ReadyForNotifications);
        }

        private string _status = Texts.StartingUp;

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

        private bool _isStatusBlinking;

        public bool IsStatusBlinking
        {
            get => _isStatusBlinking;
            set => Set(ref _isStatusBlinking, value);
        }

        private RelayCommand<ChannelInfoViewModel> _navigateToChannelCommand;
        private bool _isDatabaseLoaded;

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
            ShowInfo(Texts.Initializing);            

            SettingsVm.Model.PropertyChanged -= SettingsPropertyChanged;
            SettingsVm.Model.PropertyChanged += SettingsPropertyChanged;

            if (string.IsNullOrEmpty(SettingsVm.Model.FunctionsAppName)
                || string.IsNullOrEmpty(SettingsVm.Model.FunctionCode))
            {
                Nav.NavigateTo(ViewModelLocator.SettingsPageKey);
                return;
            }

            if (_isDatabaseLoaded)
            {
                return;
            }

            // Initialize and load the database

            try
            {
                //await Storage.InitializeAsync();

                // TODO Load the Channels collection

            }
            catch (Exception ex)
            {
                ShowInfo(string.Format(Texts.ErrorLoadingFromStorage, ex.Message));
            }

            Channels.Insert(0, _allNotifications = new ChannelInfoViewModel(
                new ChannelInfo(Texts.AllNotificationsChannelTitle),
                true));

            _isDatabaseLoaded = true;

            // Prepare to receive new notifications

            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.NotificationReceived -= ClientNotificationReceived;
            client.NotificationReceived += ClientNotificationReceived;
            client.ErrorHappened -= ClientErrorHappened;
            client.ErrorHappened += ClientErrorHappened;
            client.StatusChanged -= ClientStatusChanged;
            client.StatusChanged += ClientStatusChanged;

#if DEBUG
            if (ViewModelLocator.UseDesignData)
            {
                DesignDataGenerator.SendRandomNotifications(200, 12);
            }
#endif

            if (SettingsVm.Model.IsRegisteredSuccessfully)
            {
                await client.Initialize(false);
                ShowInfo(Texts.ReadyForNotifications);
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
                ShowInfo(string.Format(Texts.ErrorInitializing, ex.Message), true);
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
                    ShowInfo(Texts.Initializing);
                    break;

                case NotificationStatus.Ready:
                    ShowInfo(Texts.ReadyForNotifications);
                    break;
            }
        }

        private void ClientErrorHappened(object sender, string message)
        {
            ShowInfo(message, true);
        }

        private void ClientNotificationReceived(object sender, Notification notification)
        {
            if (notification.ReceivedTimeUtc <= DateTime.MinValue)
            {
                notification.ReceivedTimeUtc = DateTime.UtcNow;
            }

            ShowInfo(string.Format(
                Texts.NotificationReceived, 
                notification.ReceivedTimeUtc));

            IsStatusBlinking = true;

            Dispatcher.CheckBeginInvokeOnUI(() =>
            {
                var channel = Channels.FirstOrDefault(c => c.Model.ChannelName == notification.Channel);

                if (channel == null)
                {
                    channel = new ChannelInfoViewModel(new ChannelInfo(notification.Channel));
                    Channels.Add(channel);
                }

                _allNotifications.AddNewNotification(notification);
                channel.AddNewNotification(notification);

                Channels.Sort((a, b) => 
                {
                    if (a.IsAllNotifications)
                    {
                        return -1;
                    }

                    if (b.IsAllNotifications)
                    {
                        return 1;
                    }

                    return b.LastReceived.CompareTo(a.LastReceived);
                });
            });
        }

        public void ShowInfo(string message, bool isError = false)
        {
            Status = message;
            IsStatusBlinking = isError;
        }
    }
}
