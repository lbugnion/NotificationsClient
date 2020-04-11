using NotificationsClient.ViewModel;
using Xamarin.Forms;

namespace NotificationsClient
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);
            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
