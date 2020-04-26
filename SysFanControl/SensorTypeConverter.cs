using OpenHardwareMonitor.Hardware;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SysFanControl
{
    [ValueConversion(typeof(ISensor), typeof(string))]
    public class SensorTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var source = value as ISensor;
            switch (source.SensorType)
            {
                case SensorType.Temperature:
                    return $"{source.Value}°C";
                case SensorType.Power:
                    return $"{source.Value:0.00}W";
                default:
                    return source.Value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
