using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace WinRTNewsReader.Win8App.Common
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(Object value, Type type, Object parameter, String language)
        {
            if (value is bool)
            {
                var boolValue = (bool)value;
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility)
            {
                var vis = (Visibility)value;
                return vis == Visibility.Visible;
            }

            return false;
        }
    }
}