using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
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
        private ChannelInfoViewModel _allNotifications;
        private string _status = Texts.StartingUp;
        private bool _isStatusBlinking;
        private RelayCommand<ChannelInfoViewModel> _navigateToChannelCommand;
        private bool _isDatabaseLoaded;

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

        public ObservableCollection<ChannelInfoViewModel> Channels
        {
            get;
            private set;
        }

        public string Status
        {
            get => _status;
            set => Set(() => Status, ref _status, value);
        }

        public bool IsStatusBlinking
        {
            get => _isStatusBlinking;
            set => Set(ref _isStatusBlinking, value);
        }

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

            // TODO Remove this and handle in SettingsVm instead.
            SettingsVm.Model.PropertyChanged -= SettingsPropertyChanged;
            SettingsVm.Model.PropertyChanged += SettingsPropertyChanged;

            if (string.IsNullOrEmpty(SettingsVm.Model.FunctionsAppName)
                || string.IsNullOrEmpty(SettingsVm.Model.FunctionCode))
            {
                Nav.NavigateTo(ViewModelLocator.SettingsPageKey);
                return;
            }

            // Prepare to receive new notifications

            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.NotificationReceived -= ClientNotificationReceived;
            client.NotificationReceived += ClientNotificationReceived;
            client.ErrorHappened -= ClientErrorHappened;
            client.ErrorHappened += ClientErrorHappened;
            client.StatusChanged -= ClientStatusChanged;
            client.StatusChanged += ClientStatusChanged;
            client.RaiseDelayedNotifications();

            if (_isDatabaseLoaded)
            {
                return;
            }

            // Initialize and load the database

            try
            {
                _allNotifications = new ChannelInfoViewModel(
                    new ChannelInfo
                    {
                        ChannelName = Texts.AllNotificationsChannelTitle
                    },
                    true);

                await Storage.InitializeAsync();

                var channelsInDb = (await Storage.GetAllChannels());

                foreach (var channel in channelsInDb)
                {
                    var notificationsInChannel = (await Storage.GetChannelNotifications(channel))
                        .OrderByDescending(n => n.ReceivedTimeUtc);

                    var channelVm = new ChannelInfoViewModel(channel);

                    foreach (var notification in notificationsInChannel)
                    {
                        var notificationVm = new NotificationViewModel(notification);
                        channelVm.AddNewNotification(notificationVm);
                        _allNotifications.AddNewNotification(notificationVm);
                    }

                    Channels.Add(channelVm);
                }

                // Notifications in one channel are already sorted when
                // we get them from the DB

                _allNotifications.Notifications
                    .Sort((a, b) => b.Model.ReceivedTimeUtc.CompareTo(a.Model.ReceivedTimeUtc));

                Channels.Sort((a, b) => b.LastReceived.CompareTo(a.LastReceived));

            }
            catch (Exception ex)
            {
                ShowInfo(string.Format(Texts.ErrorLoadingFromStorage, ex.Message));
            }

            Channels.Insert(0, _allNotifications);
            _isDatabaseLoaded = true;
            Messenger.Default.Register<ReadUnreadMessage>(this, HandleReadUnreadMessage);

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

        private void HandleReadUnreadMessage(ReadUnreadMessage message)
        {
            var sender = message.Sender as NotificationViewModel;

            if (sender == null)
            {
                return;
            }

            _allNotifications.RaisePropertyChanged(() => _allNotifications.IsUnread);

            var channel = Channels.FirstOrDefault(c => c.Model.ChannelName == sender.Model.Channel);
            channel?.RaisePropertyChanged(() => channel.IsUnread);
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

        private void ClientNotificationReceived(object sender, NotificationEventArgs args)
        {
            if (args.Notification.ReceivedTimeUtc <= DateTime.MinValue)
            {
                args.Notification.ReceivedTimeUtc = DateTime.UtcNow;
            }

            args.Notification.IsUnread = true;

            ShowInfo(string.Format(
                Texts.NotificationReceived,
                args.Notification.ReceivedTimeUtc));

            IsStatusBlinking = !args.IsDelayed;

            Dispatcher.CheckBeginInvokeOnUI(() =>
            {
                var channel = Channels.FirstOrDefault(
                    c => c.Model.ChannelName == args.Notification.Channel);

                if (channel == null)
                {
                    channel = new ChannelInfoViewModel(
                        new ChannelInfo
                        {
                            ChannelName = args.Notification.Channel
                        });

                    Channels.Add(channel);
                    Storage.SaveChannelInfo(channel.Model);
                }

                var notificationVm = new NotificationViewModel(args.Notification);

                _allNotifications.AddNewNotification(notificationVm);
                channel.AddNewNotification(notificationVm);
                Storage.SaveNotification(args.Notification);

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

        public async Task DeleteChannel(ChannelInfoViewModel channelInfo)
        {
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
                    var channel = Channels.Last();
                    Channels.Remove(channel);
                }
            }
            else
            {
                if (Channels.Contains(channelInfo))
                {
                    channelInfo.Clear();
                    Channels.Remove(channelInfo);
                }
            }

            ShowInfo(Texts.ReadyForNotifications);
        }

    }
}
