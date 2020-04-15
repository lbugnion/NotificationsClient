using NotificationsClient.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NotificationsClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChannelPage : ContentPage
    {
        public ChannelPage(ChannelInfoViewModel selectedChannel)
        {
            InitializeComponent();
            BindingContext = selectedChannel;
        }
    }
}