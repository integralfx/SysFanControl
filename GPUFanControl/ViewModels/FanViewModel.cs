using GPUFanControl.ViewModels;
using OpenHardwareMonitor.Hardware;
using System;

namespace GPUFanControl.Models
{
    public class FanViewModel : HardwareViewModel
    {
        private readonly ISensor sensor;
        private int speed;

        public FanViewModel(ISensor fanSensor)
        {
            if (fanSensor.SensorType != SensorType.Fan)
            {
                throw new ArgumentException("fanSensor");
            }

            sensor = fanSensor;
        }

        public int Index { get => sensor.Index; }
        public int Speed
        {
            get => speed;
            private set => SetProperty(ref speed, value);
        }

        public override void Update()
        {
            sensor.Hardware.Update();
            var newSpeed = sensor.Value;
            if (newSpeed.HasValue)
            {
                Speed = (int)newSpeed.Value;
            }
        }
    }
}
