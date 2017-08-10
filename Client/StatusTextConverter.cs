using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows;

    [ValueConversion(typeof(object), typeof(string))]
    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string input = value as string;
            switch (input)
            {
                case "correct":
                    return "Корректно";
                case "confirmed":
                    return "Подтверждено";
                case "incorrect":
                    return "Некорректно";
                case "unconfirmed":
                    return "Не подтверждено";
                case "pending":
                    return "Ожидается";
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
