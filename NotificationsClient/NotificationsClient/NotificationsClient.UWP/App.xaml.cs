using GalaSoft.MvvmLight.Ioc;
using NotificationsClient.Model;
using NotificationsClient.UWP.Model;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NotificationsClient.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            StartApp(e, e.Arguments);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            var toastArgs = args as ToastNotificationActivatedEventArgs;

            if (toastArgs != null)
            {
                Debug.WriteLine(toastArgs.Argument);
                StartApp(args, toastArgs.Argument);
            }
            else
            {
                StartApp(args, string.Empty);
            }
        }

        private void StartApp(IActivatedEventArgs e, string arguments)
        {
            NotificationsServiceClient notificationsServiceClient;

            if (!SimpleIoc.Default.IsRegistered<INotificationsServiceClient>())
            {
                notificationsServiceClient
                    = new NotificationsServiceClient();

                SimpleIoc.Default.Register<INotificationsServiceClient>(
                    () => notificationsServiceClient);
            }
            else
            {
                notificationsServiceClient = (NotificationsServiceClient)SimpleIoc
                    .Default
                    .GetInstance<INotificationsServiceClient>();
            }

            if (!string.IsNullOrEmpty(arguments))
            {
                notificationsServiceClient.RaiseNotificationReceived(arguments);
            }

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                Xamarin.Forms.Forms.Init(e);

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
                Window.Current.VisibilityChanged += CurrentWindowVisibilityChanged;
                Window.Current.Activated += CurrentWindowActivated;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void CurrentWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            IsWindowActive = !(e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated);
        }

        public bool IsWindowVisible
        {
            get;
            private set;
        }

        public bool IsWindowActive
        {
            get;
            private set;
        }

        private void CurrentWindowVisibilityChanged(
            object sender, 
            Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            IsWindowVisible = e.Visible;
        }

        public void MakeVisible()
        {
            Window.Current.Activate();
        }
    }
}
