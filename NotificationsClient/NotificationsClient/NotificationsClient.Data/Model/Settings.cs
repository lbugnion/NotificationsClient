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

        private bool _isRegisteredSuccessfully = false;

        public bool IsRegisteredSuccessfully
        {
            get => _isRegisteredSuccessfully;
            set => Set(ref _isRegisteredSuccessfully, value);
        }

        private string _token = string.Empty;

        public string Token
        {
            get => _token;
            set => Set(ref _token, value);
        }

        public void Set(Settings settings)
        {
            FunctionCode = settings.FunctionCode;
            FunctionsAppName = settings.FunctionsAppName;
            IsRegisteredSuccessfully = settings.IsRegisteredSuccessfully;
            Token = settings.Token;
        }
    }
}
