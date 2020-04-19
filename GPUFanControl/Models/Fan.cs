using OpenHardwareMonitor.Hardware;
using System;

namespace GPUFanControl.Models
{
    public class Fan
    {
        private readonly ISensor sensor;

        public Fan(ISensor fanSensor)
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
            get
            {
                sensor.Hardware.Update();
                return (int?)sensor.Value ?? 0;
            }
        }
    }
}
