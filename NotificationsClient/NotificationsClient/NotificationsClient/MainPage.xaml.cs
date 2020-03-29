using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Model;
using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace NotificationsClient
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.NotificationReceived += ClientNotificationReceived;
            client.ErrorHappened += ClientErrorHappened;
            client.StatusChanged += ClientStatusChanged;
            client.Initialize();
        }

        private void ClientStatusChanged(object sender, NotificationStatus e)
        {
            switch (e)
            {
                case NotificationStatus.Initializing:
                    ShowInfo("Initializing...");
                    break;

                case NotificationStatus.Ready:
                    ShowInfo("Ready to receive notifications");
                    break;
            }
        }

        private void ClientErrorHappened(object sender, string message)
        {
            ShowError(message);
        }

        private void ClientNotificationReceived(object sender, Notification notification)
        {
            ShowInfo("Notification received at " + DateTime.Now);

            // TODO Show notification

            //ShowNotification(message);
        }

        private void ShowNotification(string message)
        {
            Device.BeginInvokeOnMainThread(() => {
                TestLabel.Text = message;
            });
        }

        public void ShowError(string errorMessage)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                MainLabel.TextColor = Color.Red;
                MainLabel.Text = errorMessage;
            });
        }

        public void ShowInfo(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                MainLabel.TextColor = Color.Black;
                MainLabel.Text = message;
            });
        }
    }
}
