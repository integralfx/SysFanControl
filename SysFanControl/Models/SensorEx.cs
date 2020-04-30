using OpenHardwareMonitor.Hardware;

namespace SysFanControl.Models
{
    public class SensorEx
    {
        public SensorEx(ISensor sensor)
        {
            Sensor = sensor;
        }

        public ISensor Sensor { get; }
        public string Name
        {
            get => Sensor.Name.ToLower().StartsWith(Sensor.SensorType.ToString().ToLower()) ?
                   Sensor.Name : $"{Sensor.Name} {Sensor.SensorType.ToString()}";
        }
        public float? Value { get => Sensor.Value; }
    }
}
