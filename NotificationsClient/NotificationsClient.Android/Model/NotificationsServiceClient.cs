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

        public (bool result, string errorMessage) AreOnlineServicesAvailable()
        {
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(_context);
            string errorMessage = null;

            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    errorMessage = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                }
            }

            return (resultCode == ConnectionResult.Success, errorMessage);
        }
    }
}