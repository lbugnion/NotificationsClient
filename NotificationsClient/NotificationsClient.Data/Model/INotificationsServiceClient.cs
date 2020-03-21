using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationsClient.Model
{
    public interface INotificationsServiceClient
    {
        bool IsOnlineServicesAvailable();
    }
}
