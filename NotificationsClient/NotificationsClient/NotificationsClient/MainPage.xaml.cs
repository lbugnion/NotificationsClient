using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.ViewModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NotificationsClient
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public INavigationService NavService => 
            SimpleIoc.Default.GetInstance<INavigationService>();

        public MainViewModel Vm
        {
            get => (MainViewModel)BindingContext;
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = App.Loc.Main;
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void SettingsButtonClicked(object sender, EventArgs e)
        {
            NavService.NavigateTo(ViewModelLocator.SettingsPageKey);
        }

        protected override void OnAppearing()
        {
        }

        private void ChannelTapped(object sender, EventArgs e)
        {
            var item = (Cell)sender;
            Vm.NavigateToChannelCommand.Execute(item.BindingContext);
        }

        private async void ListViewRefreshing(object sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(1000);
                ((ListView)sender).IsRefreshing = false;
            });
        }
    }
}
