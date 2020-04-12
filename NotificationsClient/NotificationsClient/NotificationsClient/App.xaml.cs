using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Helpers;
using NotificationsClient.ViewModel;
using Xamarin.Forms;

namespace NotificationsClient
{
    public partial class App : Application
    {
        // Force initialization of ViewModelLocator.
        // In Windows XAML this would be done through data binding.
        private static ViewModelLocator _locator;

        public static ViewModelLocator Loc
        {
            get
            {
                if (_locator == null)
                {
                    _locator = new ViewModelLocator();
                }

                return _locator;
            }
        }

        public App()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);
            var navPage = new NavigationPage(new MainPage());
            MainPage = navPage;

            var navService = new NavigationService();
            navService.Initialize(navPage);

            navService.Configure(ViewModelLocator.MainPageKey, typeof(MainPage));
            navService.Configure(ViewModelLocator.SettingsPageKey, typeof(SettingsPage));

            SimpleIoc.Default.Register<INavigationService>(() => navService);
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
