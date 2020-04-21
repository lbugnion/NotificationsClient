using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NotificationsClient.ViewModel
{
    public class ChannelInfoViewModel : ViewModelBase
    {
        public event EventHandler<NotificationDeletedEventArgs> NotificationDeleted;

        private RelayCommand<bool> _deleteCommand;
        private RelayCommand _markReadUnreadCommand;
        private bool _isSelectionVisible;
        private RelayCommand _deleteSelectionCommand;

        public MainViewModel Main =>
            SimpleIoc.Default.GetInstance<MainViewModel>();

        private INavigationService Nav =>
            SimpleIoc.Default.GetInstance<INavigationService>();

        private NotificationStorage Storage =>
            SimpleIoc.Default.GetInstance<NotificationStorage>();

        public ChannelInfo Model
        {
            get;
        }

        public ObservableCollection<NotificationViewModel> Notifications
        {
            get;
            set;
        }

        public bool IsAllNotifications { get; }

        public RelayCommand<bool> DeleteCommand
        {
            get => _deleteCommand
                ?? (_deleteCommand = new RelayCommand<bool>(
                async navigateBack =>
                {
                    await Main.DeleteChannel(this);

                    if (navigateBack)
                    {
                        Nav.GoBack();
                    }
                }));
        }

        public RelayCommand MarkReadUnreadCommand
        {
            get => _markReadUnreadCommand
                ?? (_markReadUnreadCommand = new RelayCommand(
                () =>
                {
                    if (IsUnread)
                    {
                        foreach (var notification in Notifications)
                        {
                            notification.MarkReadUnread(false);
                        }
                    }
                    else
                    {
                        foreach (var notification in Notifications)
                        {
                            notification.MarkReadUnread(true);
                        }
                    }

                    RaisePropertyChanged(() => IsUnread);
                }));
        }

        public DateTime LastReceived
        {
            get
            {
                if (Notifications.Count == 0)
                {
                    return DateTime.MinValue;
                }

                return Notifications.First().Model.ReceivedTimeUtc;
            }
        }

        public bool IsSelectionVisible
        {
            get => _isSelectionVisible;
            set
            {
                if (Set(ref _isSelectionVisible, value))
                {
                    foreach (var notification in Notifications)
                    {
                        notification.IsSelectVisible = value;

                        if (!value)
                        {
                            notification.IsSelected = false;
                        }
                    }
                }
            }
        }

        public RelayCommand DeleteSelectionCommand
        {
            get => _deleteSelectionCommand
                ?? (_deleteSelectionCommand = new RelayCommand(
                () =>
                {
                    // TODO Ask for confirmation (based on settings)

                    var list = Notifications.Where(n => n.IsSelected).ToList();

                    foreach (var notification in list)
                    {
                        notification.MustDelete = true;
                    }
                },
                () => Notifications.FirstOrDefault(n => n.IsSelected) != null));
        }

        public double DeleteButtonOpacity
        {
            get => Notifications.FirstOrDefault(n => n.IsSelected) == null ? 0.5 : 1.0;
        }

        public int NumberOfNotifications => Notifications.Count;

        public bool IsLastReceivedVisible => LastReceived > DateTime.MinValue;

        public bool IsUnread => Notifications.FirstOrDefault(n => n.Model.IsUnread) != null;

        public ChannelInfoViewModel(
            ChannelInfo model, 
            bool isAllNotifications = false)
        {
            Model = model;
            IsAllNotifications = isAllNotifications;
            Notifications = new ObservableCollection<NotificationViewModel>();
        }

        public void AddNewNotification(NotificationViewModel notification)
        {
            Notifications.Insert(0, notification);

            notification.PropertyChanged += NotificationPropertyChanged;

            RaisePropertyChanged(() => NumberOfNotifications);
            RaisePropertyChanged(() => IsLastReceivedVisible);
            RaisePropertyChanged(() => LastReceived);
            RaisePropertyChanged(() => IsUnread);
        }

        private async void NotificationPropertyChanged(
            object sender, 
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NotificationViewModel.IsSelected))
            {
                DeleteSelectionCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(() => DeleteButtonOpacity);
            }
            else if (e.PropertyName == nameof(NotificationViewModel.MustDelete))
            {
                // TODO If settings say that deletion are synchronized with server,
                // set the corresponding property to IsDeleted and save.

                var notification = (NotificationViewModel)sender;

                if (Notifications.Contains(notification))
                {
                    Notifications.Remove(notification);
                    NotificationDeleted?.Invoke(this, new NotificationDeletedEventArgs
                    {
                        Notification = notification
                    });

                    await Storage.Delete(notification.Model);

                    RaisePropertyChanged(() => NumberOfNotifications);
                    RaisePropertyChanged(() => IsLastReceivedVisible);
                    RaisePropertyChanged(() => LastReceived);
                    RaisePropertyChanged(() => IsUnread);
                    IsSelectionVisible = false;
                    RaisePropertyChanged(() => DeleteButtonOpacity);
                }
            }
        }

        internal void Remove(NotificationViewModel notification)
        {
            if (Notifications.Contains(notification))
            {
                Notifications.Remove(notification);

                RaisePropertyChanged(() => NumberOfNotifications);
                RaisePropertyChanged(() => IsLastReceivedVisible);
                RaisePropertyChanged(() => LastReceived);
                RaisePropertyChanged(() => IsUnread);
            }
        }

        public void UnselectAll()
        {
            foreach (var notification in Notifications)
            {
                notification.IsSelected = false;
            }

            IsSelectionVisible = false;
        }
    }
}
