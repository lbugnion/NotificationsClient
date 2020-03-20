namespace NotificationsClient.Model
{
    public interface INotificationsServiceClient
    {
        (bool result, string errorMessage) AreOnlineServicesAvailable();
        void Initialize();
    }
}
