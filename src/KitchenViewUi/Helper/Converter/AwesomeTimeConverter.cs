using System;
using System.Runtime;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace kitchenview.Helper.Converter
{
    public class AwesomeTimeConverter : IValueConverter
    {
        public static readonly AwesomeTimeConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TimeSpan time)
            {
                if (time.Ticks == 0)
                {
                    return "";
                }
                else
                {
                    return time.ToString(@"hh\:mm", new CultureInfo("de-DE"));
                }
            }

            // converter used for the wrong type
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}