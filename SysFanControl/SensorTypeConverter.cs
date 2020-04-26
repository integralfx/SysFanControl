using OpenHardwareMonitor.Hardware;
using SysFanControl.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SysFanControl
{
    [ValueConversion(typeof(FanCurveSource), typeof(string))]
    public class SensorTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var source = value as FanCurveSource;
            switch (source.Type)
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
