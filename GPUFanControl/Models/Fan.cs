using GPUFanControl.ViewModels;
using OpenHardwareMonitor.Hardware;
using System;
using System.Linq;

namespace GPUFanControl.Models
{
    public class Fan : HardwareNotifyPropertyChanged
    {
        private readonly ISensor fanSensor, fanControlSensor;
        private int speed = 0;

        public Fan(ISensor fanSensor)
        {
            if (fanSensor.SensorType != SensorType.Fan)
            {
                throw new ArgumentException("fanSensor");
            }

            this.fanSensor = fanSensor;
            fanControlSensor = fanSensor.Hardware.Sensors
                .Where(s => s.SensorType == SensorType.Control && s.Index == fanSensor.Index).First();
        }

        public int Index { get => fanSensor.Index; }
        public int Speed
        {
            get => speed;
            private set => SetProperty(ref speed, value);
        }
        public int Percent
        {
            get => (int)fanControlSensor.Control.SoftwareValue;
            set
            {
                fanControlSensor.Control.SetSoftware(value);
                PropertyUpdated();
                PropertyUpdated(nameof(Speed));
            }
        }

        public override void Update()
        {
            fanSensor.Hardware.Update();
            var newSpeed = fanSensor.Value;
            if (newSpeed.HasValue)
            {
                Speed = (int)newSpeed.Value;
            }
        }

        public void SetDefault()
        {
            fanControlSensor.Control.SetDefault();
            Update();
        }
    }
}
