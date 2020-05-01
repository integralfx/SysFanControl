using OpenHardwareMonitor.Hardware;
using SysFanControl.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SysFanControl
{
    [ValueConversion(typeof(SensorEx), typeof(string))]
    public class SensorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var source = value as SensorEx;
            switch (source.Sensor.SensorType)
            {
                case SensorType.Temperature:
                    return $"{source.Sensor.Value}°C";
                case SensorType.Power:
                    return $"{source.Sensor.Value:0.00}W";
                default:
                    return source.Sensor.Value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
