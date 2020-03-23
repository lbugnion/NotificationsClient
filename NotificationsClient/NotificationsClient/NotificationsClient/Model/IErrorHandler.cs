using System.Threading.Tasks;

namespace NotificationsClient.Model
{
    public interface IMessageHandler
    {
        void ShowError(string errorMessage);
        void ShowInfo(string v);
    }
}
