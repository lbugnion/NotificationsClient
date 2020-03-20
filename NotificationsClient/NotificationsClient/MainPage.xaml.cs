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
            var availability = client.AreOnlineServicesAvailable();

            if (availability.result)
            {
                MainLabel.Text = "Available :)";
                client.Initialize();
            }
            else
            {
                if (availability.errorMessage == null)
                {
                    MainLabel.Text = "Not available :(";
                }
                else
                {
                    MainLabel.Text = availability.errorMessage;
                }
            }

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<string>(
                this,
                HandleMessage);
        }

        private void HandleMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() => {
                TestLabel.Text = message;
            });
        }

        private void LogTokenButtonClicked(object sender, System.EventArgs e)
        {
            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.ShowToken();
        }
    }
}
