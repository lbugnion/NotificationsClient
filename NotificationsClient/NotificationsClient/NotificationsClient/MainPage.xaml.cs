using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using NotificationsClient.Model;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NotificationsClient
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage, IMessageHandler
    {
        public MainPage()
        {
            InitializeComponent();
            SimpleIoc.Default.Register<IMessageHandler>(() => this);

            var client = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
            client.NotificationReceived += ClientNotificationReceived;
            client.Initialize();
        }

        private void ClientNotificationReceived(object sender, string message)
        {
            HandleMessage(message);
        }

        private void HandleMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() => {
                TestLabel.Text = message;
            });
        }

        public void ShowError(string errorMessage)
        {
            MainLabel.TextColor = Color.Red;
            MainLabel.Text = errorMessage;
        }

        public void ShowInfo(string message)
        {
            MainLabel.TextColor = Color.Black;
            MainLabel.Text = message;
        }
    }
}
