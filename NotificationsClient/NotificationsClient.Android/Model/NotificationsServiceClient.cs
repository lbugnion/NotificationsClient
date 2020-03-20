using Android.Gms.Common;
using NotificationsClient.Model;

namespace NotificationsClient.Droid.Model
{
    public class NotificationsServiceClient : INotificationsServiceClient
    {
        private MainActivity _context;

        public NotificationsServiceClient(MainActivity activity)
        {
            _context = activity;
        }

        public bool IsOnlineServicesAvailable()
        {
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(_context);
            return resultCode == ConnectionResult.Success;

            //if (resultCode != ConnectionResult.Success)
            //{
            //    if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
            //        msgText.Text = GoogleApiAvailability.Instance.GetErrorString(resultCode);
            //    else
            //    {
            //        msgText.Text = "This device is not supported";
            //        Finish();
            //    }
            //    return false;
            //}
            //else
            //{
            //    msgText.Text = "Google Play Services is available.";
            //    return true;
            //}
        }
    }
}