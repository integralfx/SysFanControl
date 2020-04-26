using OpenHardwareMonitor.Hardware;
using SysFanControl.ViewModels;
using System;

namespace SysFanControl.Models
{
    public class FanCurveSource : HardwareNotifyPropertyChanged
    {
        private float value = 0.0f;

        public FanCurveSource(ISensor sensor)
        {
            if (!IsSensorAllowed(sensor))
            {
                throw new ArgumentException("Sensor type must be temperature or power.");
            }

            Sensor = sensor;
            Update();
        }

        public string Name { get => Sensor.Name; }
        public float Value
        {
            get => value;
            private set => SetProperty(ref this.value, value);
        }
        public SensorType Type { get => Sensor.SensorType; }
        public ISensor Sensor { get; }

        public override void Update()
        {
            Sensor.Hardware.Update();
            var newValue = Sensor.Value;
            if (newValue.HasValue)
            {
                Value = newValue.Value;
            }
        }

        public static bool IsSensorAllowed(ISensor sensor)
        {
            return sensor.SensorType == SensorType.Temperature || sensor.SensorType == SensorType.Power;
        }
    }
}
