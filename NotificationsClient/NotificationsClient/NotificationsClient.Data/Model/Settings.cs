using GalaSoft.MvvmLight;
using System;
using System.IO;

namespace NotificationsClient.Model
{
    public class Settings : ObservableObject
    {
        private const string AppFolderName = "GalaSoft.NotificationsClient";

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

        private bool _confirmOneDelete = false;

        public bool ConfirmOneDelete
        {
            get => _confirmOneDelete;
            set => Set(ref _confirmOneDelete, value);
        }

        private bool _confirmManyDelete = true;

        public bool ConfirmManyDelete
        {
            get => _confirmManyDelete;
            set => Set(ref _confirmManyDelete, value);
        }

        private bool _confirmChannelReadUnread = true;

        public bool ConfirmChannelReadUnread
        {
            get => _confirmChannelReadUnread;
            set => Set(ref _confirmChannelReadUnread, value);
        }

        private bool _confirmChannelDelete = true;

        public bool ConfirmChannelDelete
        {
            get => _confirmChannelDelete;
            set => Set(ref _confirmChannelDelete, value);
        }

        public void Set(Settings settings)
        {
            FunctionCode = settings.FunctionCode;
            FunctionsAppName = settings.FunctionsAppName;
            IsRegisteredSuccessfully = settings.IsRegisteredSuccessfully;
            Token = settings.Token;
            ConfirmChannelDelete = settings.ConfirmChannelDelete;
            ConfirmChannelReadUnread = settings.ConfirmChannelReadUnread;
            ConfirmManyDelete = settings.ConfirmManyDelete;
            ConfirmOneDelete = settings.ConfirmOneDelete;

#if DEBUG
            FunctionCode = DebugSettings.FunctionCode;
            FunctionsAppName = DebugSettings.FunctionsAppName;
#endif
        }

#if DEBUG
        public Settings()
        {
            FunctionCode = DebugSettings.FunctionCode;
            FunctionsAppName = DebugSettings.FunctionsAppName;
        }
#endif

        public DirectoryInfo GetAppFolder()
        {
            var rootFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var configFolder = new DirectoryInfo(Path.Combine(rootFolderPath, AppFolderName));

            if (!configFolder.Exists)
            {
                configFolder.Create();
            }

            return configFolder;
        }
    }
}
