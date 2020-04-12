using GalaSoft.MvvmLight.Ioc;

namespace NotificationsClient.ViewModel
{
    public class ViewModelLocator
    {
        public static readonly string MainPageKey = "MainPage";
        public static readonly string SettingsPageKey = "SettingsPage";

        static ViewModelLocator()
        {
            SimpleIoc.Default.Register<MainViewModel>();
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
