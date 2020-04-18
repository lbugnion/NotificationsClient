using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using NotificationsClient.Helpers;
using NotificationsClient.Model;
using System;

namespace NotificationsClient.ViewModel
{
    public class NotificationViewModel : ViewModelBase
    {
        private NotificationStorage Storage =>
            SimpleIoc.Default.GetInstance<NotificationStorage>();

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
