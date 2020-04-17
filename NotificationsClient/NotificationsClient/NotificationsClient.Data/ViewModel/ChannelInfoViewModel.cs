using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Model;
using System;

namespace NotificationsClient.ViewModel
{
    public class ChannelInfoViewModel : ViewModelBase
    {
        public MainViewModel Main =>
            SimpleIoc.Default.GetInstance<MainViewModel>();

        private INavigationService Nav =>
            SimpleIoc.Default.GetInstance<INavigationService>();

        public ChannelInfo Model
        {
            get;
        }

        public bool IsAllNotifications { get; }

        private RelayCommand<bool> _deleteCommand;

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

        public int NumberOfNotifications => Model.Notifications.Count;

        public bool IsLastReceivedVisible => Model.LastReceived > DateTime.MinValue;

        public ChannelInfoViewModel(ChannelInfo model, bool isAllNotifications = false)
        {
            Model = model;
            IsAllNotifications = isAllNotifications;
        }

        public void RemoveNotification(Notification notification)
        {
            if (Model.Notifications.Contains(notification))
            {
                Model.Notifications.Remove(notification);
                RaisePropertyChanged(() => NumberOfNotifications);
            }
        }

        public void AddNewNotification(Notification notification)
        {
            Model.Notifications.Insert(0, notification);
            RaisePropertyChanged(() => NumberOfNotifications);
            RaisePropertyChanged(() => IsLastReceivedVisible);
            Model.RaisePropertyChanged(() => Model.LastReceived);
        }

        public void Clear()
        {
            Model.Notifications.Clear();
            RaisePropertyChanged(() => NumberOfNotifications);
        }
    }
}
