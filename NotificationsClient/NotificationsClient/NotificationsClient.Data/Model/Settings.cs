using GalaSoft.MvvmLight;

namespace NotificationsClient.Model
{
    public class Settings : ObservableObject
    {
        private string _functionCode = string.Empty;

        public string FunctionCode
        {
            get => _functionCode;
            set => Set(ref _functionCode, value);
        }

        private string _functionsAppName = string.Empty;

        public string FunctionsAppName
        {
            get => _functionsAppName;
            set => Set(ref _functionsAppName, value);
        }
    }
}
