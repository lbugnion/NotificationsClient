﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using NotificationsClient.Droid.Model;
using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Model;
using GalaSoft.MvvmLight.Threading;

namespace NotificationsClient.Droid
{
    [Activity(Label = "NotificationsClient", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (!SimpleIoc.Default.IsRegistered<INotificationsServiceClient>())
            {
                var notificationsServiceClient
                    = new NotificationsServiceClient(this);

                SimpleIoc.Default.Register<INotificationsServiceClient>(
                    () => notificationsServiceClient);
            }

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            if (Intent.Extras != null)
            {
                var message = string.Empty;

                foreach (var key in Intent.Extras.KeySet())
                {
                    var value = Intent.Extras.GetString(key);
                    message += $"{key}:{value} |";
                }

                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(message);
            }

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}