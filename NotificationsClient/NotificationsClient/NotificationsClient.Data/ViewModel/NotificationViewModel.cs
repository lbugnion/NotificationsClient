using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Helpers;
using NotificationsClient.Model;
using System;

namespace NotificationsClient.ViewModel
{
    public class NotificationViewModel : ViewModelBase
    {
        private bool _isSelected;
        private bool _isSelectVisible;
        private bool _mustDelete = false;

        private NotificationStorage Storage =>
            SimpleIoc.Default.GetInstance<NotificationStorage>();

        private INavigationService Nav =>
            SimpleIoc.Default.GetInstance<INavigationService>();

        public Notification Model
        {
            get;
        }

        private RelayCommand _markReadUnreadCommand;

        public RelayCommand MarkReadUnreadCommand
        {
            get => _markReadUnreadCommand
                ?? (_markReadUnreadCommand = new RelayCommand(
                () => Model.IsUnread = !Model.IsUnread));
        }

        private RelayCommand _deleteCommand;

        public RelayCommand DeleteCommand
        {
            get => _deleteCommand
                ?? (_deleteCommand = new RelayCommand(
                () =>
                {
                    MustDelete = true;
                    Nav.GoBack();
                }));
        }

        public bool IsSelectVisible
        {
            get => _isSelectVisible;
            set => Set(ref _isSelectVisible, value);
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
