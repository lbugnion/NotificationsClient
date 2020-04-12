﻿using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NotificationsClient
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = App.Loc.Settings;
        }

        protected override bool OnBackButtonPressed()
        {
            App.Loc.Main.Initialize();
            return base.OnBackButtonPressed();
        }
    }
}