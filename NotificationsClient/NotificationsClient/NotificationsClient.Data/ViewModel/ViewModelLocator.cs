using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Model;

namespace NotificationsClient.ViewModel
{
    public class ViewModelLocator
    {
#if DEBUG
        public const bool UseDesignData = true;
#endif

        public static readonly string MainPageKey = "MainPage";
        public static readonly string ChannelPageKey = "ChannelPage";
        public static readonly string NotificationsPageKey = "NotificationPage";
        public static readonly string SettingsPageKey = "SettingsPage";

        static ViewModelLocator()
        {
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ConfigurationClient>();
            SimpleIoc.Default.Register<NotificationStorage>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get => SimpleIoc.Default.GetInstance<MainViewModel>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public SettingsViewModel Settings
        {
            get => SimpleIoc.Default.GetInstance<SettingsViewModel>();
        }
    }
}
