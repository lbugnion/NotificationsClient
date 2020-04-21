using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.Helpers;
using NotificationsClient.ViewModel;
using System.Diagnostics;
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

            NavigationService navigationService;

            if (!SimpleIoc.Default.IsRegistered<INavigationService>())
            {
                navigationService = new NavigationService();

                navigationService.Configure(ViewModelLocator.MainPageKey, typeof(MainPage));
                navigationService.Configure(ViewModelLocator.ChannelPageKey, typeof(ChannelPage));
                navigationService.Configure(ViewModelLocator.NotificationsPageKey, typeof(NotificationPage));
                navigationService.Configure(ViewModelLocator.SettingsPageKey, typeof(SettingsPage));

                SimpleIoc.Default.Register<INavigationService>(() => navigationService);
            }
            else
            {
                navigationService = (NavigationService)SimpleIoc.Default.GetInstance<INavigationService>();
            }

            DialogService dialogService;

            if (!SimpleIoc.Default.IsRegistered<IDialogService>())
            {
                dialogService = new DialogService();
                SimpleIoc.Default.Register<IDialogService>(() => dialogService);
            }
            else
            {
                dialogService = (DialogService)SimpleIoc.Default.GetInstance<IDialogService>();
            }

            if (!SimpleIoc.Default.IsRegistered<IDispatcherHelper>())
            {
                SimpleIoc.Default.Register<IDispatcherHelper, DispatcherHelper>();
            }

            Loc.Settings.LoadSettings();

            dialogService.Initialize(navPage);
            navigationService.Initialize(navPage);

            InitializeComponent();
            MainPage = navPage;
            Loc.Main.Initialize().SafeFireAndForget(false);
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
