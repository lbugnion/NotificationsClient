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
        private RelayCommand<bool> _deleteCommand;
        private RelayCommand _markReadUnreadCommand;

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

        public void RemoveNotification(NotificationViewModel notification)
        {
            if (Notifications.Contains(notification))
            {
                notification.PropertyChanged -= NotificationPropertyChanged;

                // TODO Save
                Notifications.Remove(notification);
                RaisePropertyChanged(() => NumberOfNotifications);
                RaisePropertyChanged(() => IsLastReceivedVisible);
                RaisePropertyChanged(() => LastReceived);
                RaisePropertyChanged(() => IsUnread);
            }
        }

        public void Clear()
        {
            foreach (var notification in Notifications)
            {
                notification.PropertyChanged -= NotificationPropertyChanged;
            }

            // TODO Save
            Notifications.Clear();
            RaisePropertyChanged(() => NumberOfNotifications);
            RaisePropertyChanged(() => IsLastReceivedVisible);
            RaisePropertyChanged(() => LastReceived);
            RaisePropertyChanged(() => IsUnread);
        }

        private void NotificationPropertyChanged(
            object sender, 
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Notification.IsUnread))
            {
                RaisePropertyChanged(() => IsUnread);
            }
        }
    }
}
