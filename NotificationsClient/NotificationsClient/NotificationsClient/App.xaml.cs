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
            var navPage = new NavigationPage(new MainPage());

            if (!SimpleIoc.Default.IsRegistered<INavigationService>())
            {
                var navService = new NavigationService();
                navService.Initialize(navPage);

                navService.Configure(ViewModelLocator.MainPageKey, typeof(MainPage));
                navService.Configure(ViewModelLocator.ChannelPageKey, typeof(ChannelPage));
                navService.Configure(ViewModelLocator.SettingsPageKey, typeof(SettingsPage));

                SimpleIoc.Default.Register<INavigationService>(() => navService);
            }

            if (!SimpleIoc.Default.IsRegistered<IDialogService>())
            {
                var dialogService = new DialogService();
                dialogService.Initialize(navPage);

                SimpleIoc.Default.Register<IDialogService>(() => dialogService);
            }

            if (!SimpleIoc.Default.IsRegistered<IDispatcherHelper>())
            {
                SimpleIoc.Default.Register<IDispatcherHelper, DispatcherHelper>();
            }

            Loc.Settings.LoadSettings();
            Loc.Main.Initialize().SafeFireAndForget(false);

            InitializeComponent();

            MainPage = navPage;
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
