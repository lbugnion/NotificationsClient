using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using NotificationsClient.Droid.Model;
using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Model;
using Notifications;

namespace NotificationsClient.Droid
{
    [Activity(
        Label = "Notifications", 
        Icon = "@mipmap/icon", 
        Theme = "@style/MainTheme", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
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

            if (Intent.Extras != null)
            {
                var uniqueId = string.Empty;
                var title = string.Empty;
                var body = string.Empty;
                var sentTimeUtc = string.Empty;
                var channel = string.Empty;

                if (Intent.Extras.ContainsKey(FunctionConstants.UniqueId))
                {
                    uniqueId = Intent.Extras.GetString(FunctionConstants.UniqueId);
                }
                if (Intent.Extras.ContainsKey(FunctionConstants.Title))
                {
                    title = Intent.Extras.GetString(FunctionConstants.Title);
                }
                if (Intent.Extras.ContainsKey(FunctionConstants.Body))
                {
                    body = Intent.Extras.GetString(FunctionConstants.Body);
                }
                if (Intent.Extras.ContainsKey(FunctionConstants.SentTimeUtc))
                {
                    sentTimeUtc = Intent.Extras.GetString(FunctionConstants.SentTimeUtc);
                }
                if (Intent.Extras.ContainsKey(FunctionConstants.Channel))
                {
                    channel = Intent.Extras.GetString(FunctionConstants.Channel);
                }

                var argument = FunctionConstants.UwpArgumentTemplate
                    .Replace(FunctionConstants.UniqueId, uniqueId)
                    .Replace(FunctionConstants.Title, title)
                    .Replace(FunctionConstants.Body, body)
                    .Replace(FunctionConstants.SentTimeUtc, sentTimeUtc)
                    .Replace(FunctionConstants.Channel, channel);

                var notification = NotificationsClient.Model.Notification.Parse(argument);
                notificationsServiceClient.RaiseNotificationReceived(notification, true);
            }

            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}