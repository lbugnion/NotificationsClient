using NotificationsClient.Model;
using NotificationsClient.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NotificationsClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChannelPage : ContentPage
    {
        public ChannelInfoViewModel Vm
        {
            get => (ChannelInfoViewModel)BindingContext;
        }

        public ChannelPage(ChannelInfoViewModel selectedChannel)
        {
            InitializeComponent();
            BindingContext = selectedChannel;
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void NotificationTapped(object sender, ItemTappedEventArgs e)
        {
            //var notification = e.Item as NotificationViewModel;
            //Vm.Main.
        }

        protected override void OnDisappearing()
        {
            Vm.UnselectAll();

            base.OnDisappearing();
        }
    }
}