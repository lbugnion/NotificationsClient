using NotificationsClient.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NotificationsClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationPage : ContentPage
    {
        public NotificationPage(NotificationViewModel selectedNotification)
        {
            InitializeComponent();
            BindingContext = selectedNotification;
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}