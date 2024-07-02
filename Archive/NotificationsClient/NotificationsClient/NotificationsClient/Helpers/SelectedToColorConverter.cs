using System;
using System.Globalization;
using Xamarin.Forms;

namespace NotificationsClient.Helpers
{
    public class SelectedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value)
            {
                return Application.Current.Resources["SelectedBackgroundColor"];
            }
            else
            {
                return Application.Current.Resources["UnselectedBackgroundColor"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
