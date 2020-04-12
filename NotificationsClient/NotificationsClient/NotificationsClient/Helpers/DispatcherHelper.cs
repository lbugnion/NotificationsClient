using System;
using Xamarin.Forms;

namespace NotificationsClient.Helpers
{
    public class DispatcherHelper : IDispatcherHelper
    {
        public void CheckBeginInvokeOnUI(Action action)
        {
            Device.BeginInvokeOnMainThread(action);
        }
    }
}
