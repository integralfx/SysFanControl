using OpenHardwareMonitor.Hardware;
using SysFanControl.ViewModels;
using System;

namespace SysFanControl.Models
{
    public class FanCurveSource : HardwareNotifyPropertyChanged
    {
        private readonly ISensor sensor;
        private float value = 0.0f;

        public FanCurveSource(ISensor sensor)
        {
            if (!IsSensorAllowed(sensor))
            {
                throw new ArgumentException("Sensor type must be temperature or power.");
            }

            this.sensor = sensor;
            Update();
        }

        public string Name { get => sensor.Name; }
        public float Value
        {
            get => value;
            private set => SetProperty(ref this.value, value);
        }
        public SensorType Type { get => sensor.SensorType; }

        public override void Update()
        {
            sensor.Hardware.Update();
            var newValue = sensor.Value;
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
