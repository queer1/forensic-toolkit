namespace GrayHills.ForensicToolkit.Common.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Globalization;

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (value is bool))
            {
                bool source = (bool)value;
                if (Invert) source = !source;

                if (source)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (value is Visibility))
            {
                Visibility source = (Visibility)value;
                bool ret = false;

                if (source == Visibility.Visible)
                {
                    ret = true;
                }

                if (Invert) ret = !ret;

                return ret;
            }

            throw new NotSupportedException();
        }
    }
}
