using System;

namespace NotificationsClient.Helpers
{
    public interface IDispatcherHelper
    {
        void CheckBeginInvokeOnUI(Action action);
    }
}
