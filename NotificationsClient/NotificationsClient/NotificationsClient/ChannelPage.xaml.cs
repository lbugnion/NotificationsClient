using NotificationsClient.Model;
using NotificationsClient.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NotificationsClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChannelPage : ContentPage
    {
        public ChannelInfoViewModel Vm => (ChannelInfoViewModel)BindingContext;

        public ChannelPage(ChannelInfoViewModel selectedChannel)
        {
            InitializeComponent();
            BindingContext = selectedChannel;
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void NotificationTapped(object sender, System.EventArgs e)
        {
            var item = (Cell)sender;
            Vm.NavigateToNotificationCommand.Execute(item.BindingContext);
        }
    }
}