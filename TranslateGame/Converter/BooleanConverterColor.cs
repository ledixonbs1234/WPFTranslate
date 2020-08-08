using System;
using System.Windows.Data;

namespace TranslateGame.Converter
{
    public class BooleanConverterColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return "Green";
                else
                    return "White";
            }
            return "White";
                    }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}