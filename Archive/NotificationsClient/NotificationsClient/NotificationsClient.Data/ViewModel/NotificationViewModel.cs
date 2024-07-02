using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Helpers;
using NotificationsClient.Model;
using NotificationsClient.Resources;
using System;

namespace NotificationsClient.ViewModel
{
    public class NotificationViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _mustDelete = false;
        private RelayCommand _markReadUnreadCommand;
        private RelayCommand _deleteCommand;

        public MainViewModel Main =>
            SimpleIoc.Default.GetInstance<MainViewModel>();

        private NotificationStorage Storage =>
            SimpleIoc.Default.GetInstance<NotificationStorage>();

        private Settings Settings =>
            SimpleIoc.Default.GetInstance<Settings>();

        private INavigationService Nav =>
            SimpleIoc.Default.GetInstance<INavigationService>();

        private IDialogService Dialog =>
            SimpleIoc.Default.GetInstance<IDialogService>();

        public Notification Model
        {
            get;
        }

        public RelayCommand MarkReadUnreadCommand
        {
            get => _markReadUnreadCommand
                ?? (_markReadUnreadCommand = new RelayCommand(
                () => Model.IsUnread = !Model.IsUnread));
        }

        public RelayCommand DeleteCommand
        {
            get => _deleteCommand
                ?? (_deleteCommand = new RelayCommand(
                async () =>
                {
                    if (Settings.ConfirmManyDelete)
                    {
                        if (!await Dialog.ShowMessage(
                            Texts.DeletingOneNotification,
                            Texts.AreYouSure,
                            "Yes",
                            "No",
                            null))
                        {
                            return;
                        }
                    }

                    MustDelete = true;
                    Nav.GoBack();
                }));
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public bool MustDelete
        {
            get => _mustDelete;
            set => Set(ref _mustDelete, value);
        }

        public DateTime SentTimeLocal => Model.SentTimeUtc.ToLocalTime();
        public DateTime ReceivedTimeLocal => Model.ReceivedTimeUtc.ToLocalTime();

        public NotificationViewModel(Notification model)
        {
            Model = model;
            Model.PropertyChanged += ModelPropertyChanged;
        }

        private void ModelPropertyChanged(
            object sender, 
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.IsUnread))
            {
                Messenger.Default.Send(new ReadUnreadMessage(this));
                Storage.SaveNotification(Model);
            }
        }

        internal void MarkReadUnread(bool unread)
        {
            Model.IsUnread = unread;
        }
    }
}
