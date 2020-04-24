using GPUFanControl.ViewModels;
using OpenHardwareMonitor.Hardware;
using System;
using System.Linq;

namespace GPUFanControl.Models
{
    public class Fan : HardwareNotifyPropertyChanged, IDisposable
    {
        private readonly ISensor fanSensor, fanControlSensor;
        private int speed = 0, percent = 50;
        private bool disposed = false;

        public Fan(ISensor fanSensor)
        {
            if (fanSensor.SensorType != SensorType.Fan)
            {
                throw new ArgumentException("Argument isn't a fan sensor.");
            }

            this.fanSensor = fanSensor;
            fanControlSensor = fanSensor.Hardware.Sensors
                .Where(s => s.SensorType == SensorType.Control && s.Index == fanSensor.Index)
                .First();
            percent = (int)fanControlSensor.Control.SoftwareValue;
        }

        ~Fan()
        {
            Dispose(false);
        }

        public int Index { get => fanSensor.Index; }
        public int Speed
        {
            get => speed;
            private set => SetProperty(ref speed, value);
        }
        public int Percent
        {
            get => percent;
            set
            {
                fanControlSensor.Control.SetSoftware(value);
                SetProperty(ref percent, value);
                PropertyUpdated(nameof(Speed));
            }
        }

        public override void Update()
        {
            fanSensor.Hardware.Update();
            Speed = (int?)fanSensor.Value ?? Speed;
        }

        public void SetDefault()
        {
            fanControlSensor.Control.SetDefault();
            Update();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                SetDefault();

                disposed = true;
            }
        }
    }
}
