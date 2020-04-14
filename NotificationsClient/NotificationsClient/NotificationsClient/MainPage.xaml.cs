using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using NotificationsClient.ViewModel;
using System;
using System.ComponentModel;
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
    }
}
