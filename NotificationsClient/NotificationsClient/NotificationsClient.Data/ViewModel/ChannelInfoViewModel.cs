using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Model;

namespace NotificationsClient.ViewModel
{
    public class ChannelInfoViewModel : ViewModelBase
    {
        private MainViewModel Main =>
            SimpleIoc.Default.GetInstance<MainViewModel>();

        private INavigationService Nav =>
            SimpleIoc.Default.GetInstance<INavigationService>();

        public ChannelInfo Model
        {
            get;
        }

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

        public int NumberOfNotifications
        {
            get => Model.Notifications.Count;
        }

        public ChannelInfoViewModel(ChannelInfo model)
        {
            Model = model;
        }

        public void RemoveNotification(Notification notification)
        {
            if (Model.Notifications.Contains(notification))
            {
                Model.Notifications.Remove(notification);
                RaisePropertyChanged(() => NumberOfNotifications);
            }
        }

        public void AddNotification(Notification notification)
        {
            Model.Notifications.Add(notification);
            RaisePropertyChanged(() => NumberOfNotifications);
        }

        public void Clear()
        {
            Model.Notifications.Clear();
            RaisePropertyChanged(() => NumberOfNotifications);
        }
    }
}
