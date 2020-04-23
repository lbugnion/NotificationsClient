using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Model;
using NotificationsClient.Resources;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NotificationsClient.ViewModel
{
    public class ChannelInfoViewModel : ViewModelBase
    {
        public event EventHandler<NotificationDeletedEventArgs> NotificationDeleted;

        private RelayCommand _deleteCommand;
        private RelayCommand _markReadUnreadCommand;
        private RelayCommand _deleteSelectionCommand;
        private bool _mustDelete;

        public MainViewModel Main =>
            SimpleIoc.Default.GetInstance<MainViewModel>();

        private INavigationService Nav =>
            SimpleIoc.Default.GetInstance<INavigationService>();

        private IDialogService Dialog =>
            SimpleIoc.Default.GetInstance<IDialogService>();

        private NotificationStorage Storage =>
            SimpleIoc.Default.GetInstance<NotificationStorage>();

        private Settings Settings =>
            SimpleIoc.Default.GetInstance<Settings>();

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

        public RelayCommand DeleteCommand
        {
            get => _deleteCommand
                ?? (_deleteCommand = new RelayCommand(
                async () =>
                {
                    if (Settings.ConfirmChannelDelete)
                    {
                        if (!await Dialog.ShowMessage(
                            Texts.DeletingChannel,
                            Texts.AreYouSure,
                            "Yes",
                            "No",
                            null))
                        {
                            return;
                        }
                    }

                    foreach (var notification in Notifications)
                    {
                        notification.PropertyChanged -= NotificationPropertyChanged;
                        await Storage.Delete(notification.Model);
                    }

                    await Storage.Delete(Model);
                    MustDelete = true;
                }));
        }

        public bool MustDelete
        {
            get => _mustDelete;
            set => Set(ref _mustDelete, value);
        }

        public RelayCommand MarkReadUnreadCommand
        {
            get => _markReadUnreadCommand
                ?? (_markReadUnreadCommand = new RelayCommand(
                async () =>
                {
                    if (Settings.ConfirmChannelReadUnread)
                    {
                        if (!await Dialog.ShowMessage(
                            string.Format(Texts.MarkingChannel, IsUnread ? Texts.Read : Texts.Unread),
                            Texts.AreYouSure,
                            "Yes",
                            "No",
                            null))
                        {
                            return;
                        }
                    }

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

                return Notifications.First().ReceivedTimeLocal;
            }
        }

        public RelayCommand DeleteSelectionCommand
        {
            get => _deleteSelectionCommand
                ?? (_deleteSelectionCommand = new RelayCommand(
                async () =>
                {
                    if (Settings.ConfirmManyDelete)
                    {
                        if (!await Dialog.ShowMessage(
                            Texts.DeletingManyNotifications,
                            Texts.AreYouSure,
                            "Yes",
                            "No",
                            null))
                        {
                            return;
                        }
                    }

                    var list = Notifications.Where(n => n.IsSelected).ToList();

                    foreach (var notification in list)
                    {
                        notification.MustDelete = true;
                    }
                },
                () => Notifications.FirstOrDefault(n => n.IsSelected) != null));
        }

        private RelayCommand<NotificationViewModel> _navigateToNotificationCommand;

        public RelayCommand<NotificationViewModel> NavigateToNotificationCommand
        {
            get => _navigateToNotificationCommand
                ?? (_navigateToNotificationCommand = new RelayCommand<NotificationViewModel>(
                notification =>
                {
                    Nav.NavigateTo(ViewModelLocator.NotificationsPageKey, notification);
                }));
        }

        public double DeleteButtonOpacity 
            => Notifications.FirstOrDefault(n => n.IsSelected) == null ? 0.5 : 1.0;

        public int NumberOfNotifications 
            => Notifications.Count;

        public bool IsLastReceivedVisible 
            => LastReceived > DateTime.MinValue;

        public bool IsUnread 
            => Notifications.FirstOrDefault(n => n.Model.IsUnread) != null;

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
                    RaisePropertyChanged(() => DeleteButtonOpacity);
                }
            }
        }

        internal void Remove(NotificationViewModel notification)
        {
            if (Notifications.Contains(notification))
            {
                notification.PropertyChanged -= NotificationPropertyChanged;
                Notifications.Remove(notification);

                RaisePropertyChanged(() => NumberOfNotifications);
                RaisePropertyChanged(() => IsLastReceivedVisible);
                RaisePropertyChanged(() => LastReceived);
                RaisePropertyChanged(() => IsUnread);
            }
        }
    }
}
