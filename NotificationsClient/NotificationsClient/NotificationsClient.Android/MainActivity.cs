using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using NotificationsClient.Droid.Model;
using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Model;

namespace NotificationsClient.Droid
{
    [Activity(Label = "NotificationsClient", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Context
        {
            get;
            private set;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Context = this;

            INotificationsServiceClient notificationsServiceClient;

            if (!SimpleIoc.Default.IsRegistered<INotificationsServiceClient>())
            {
                notificationsServiceClient
                    = new NotificationsServiceClient(this);

                SimpleIoc.Default.Register<INotificationsServiceClient>(
                    () => notificationsServiceClient);
            }
            else
            {
                notificationsServiceClient = SimpleIoc.Default.GetInstance<INotificationsServiceClient>();
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

                var title = string.Empty;
                var body = string.Empty;
                var channel = string.Empty;

                if (Intent.Extras.ContainsKey("title"))
                {
                    title = Intent.Extras.GetString("title");
                }
                if (Intent.Extras.ContainsKey("body"))
                {
                    body = Intent.Extras.GetString("body");
                }
                if (Intent.Extras.ContainsKey("channel"))
                {
                    channel = Intent.Extras.GetString("channel");
                }

                notificationsServiceClient.RaiseNotificationReceived(new NotificationsClient.Model.Notification
                {
                    Title = title,
                    Body = body,
                    Channel = channel
                });
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}