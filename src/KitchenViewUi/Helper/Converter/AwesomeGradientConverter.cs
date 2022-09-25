using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace kitchenview.Helper.Converter
{
    public class AwesomeGradientConverter : IValueConverter
    {
        public static readonly AwesomeGradientConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string color)
            {
                if (color.Contains(';'))
                {
                    string[] colors = color.Split(";");
                    var gradient = new LinearGradientBrush
                    {
                        GradientStops = new GradientStops
                        {
                            new GradientStop(Color.Parse(colors[0]), 0),
                            new GradientStop(Color.Parse(colors[1]), 1)
                        },
                        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative)
                    };

                    return gradient;
                }
                else
                {
                    return new SolidColorBrush(Color.Parse(color));
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