using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows;

    [ValueConversion(typeof(object), typeof(string))]
    public class StatusBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string input = value as string;
            switch (input)
            {
                case "correct":
                case "confirmed":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB3FFC9"));
                case "incorrect":
                case "unconfirmed":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFB3B3"));
                case "pending":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFF4B3"));
                default:
                    return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
