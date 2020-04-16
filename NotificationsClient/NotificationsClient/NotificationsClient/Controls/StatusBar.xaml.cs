using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NotificationsClient.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StatusBar : ContentView
    {
        public static readonly BindableProperty IsBlinkingProperty =
          BindableProperty.Create(
              "IsBlinking", 
              typeof(bool), 
              typeof(StatusBar),
              false,
              propertyChanged: (s, o, n) =>
              {
                  var bar = (StatusBar)s;

                  if ((bool)n)
                  {
                      bar.StartBlinking();
                  }
                  else
                  {
                      bar._isBlinking = false;
                  }
              });

        public static readonly BindableProperty StatusTextProperty =
          BindableProperty.Create(
              "StatusText", 
              typeof(string), 
              typeof(StatusBar), 
              string.Empty,
              propertyChanged: (s, o, n) =>
              {
                  var bar = (StatusBar)s;
                  bar.ErrorLabel.Text = (string)n;
                  bar.NormalLabel.Text = (string)n;
              });

        private bool _isBlinking;

        public bool IsBlinking
        {
            get => (bool)GetValue(IsBlinkingProperty);
            set
            {
                SetValue(IsBlinkingProperty, value);
                _isBlinking = false;
            }
        }

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public StatusBar()
        {
            InitializeComponent();
        }

        public async void StartBlinking()
        {
            if (_isBlinking)
            {
                return;
            }

            _isBlinking = true;

            while (_isBlinking)
            {
                await NormalStatusFrame.FadeTo(0, 200);
                await NormalStatusFrame.FadeTo(1, 200);
                await Task.Delay(800);
            }
        }

        private void StatusBarClicked(object sender, System.EventArgs e)
        {
            IsBlinking = false;
        }
    }
}