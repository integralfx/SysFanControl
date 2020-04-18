using GPUFanControl.Models;
using OpenHardwareMonitor.Hardware;
using System;

namespace GPUFanControl.ViewModels
{
    public class FanViewModel : BaseViewModel
    {
        private readonly Fan fan = new Fan();

        public FanViewModel(ISensor fanSensor)
        {
            if (fanSensor.SensorType != SensorType.Fan)
            {
                throw new ArgumentException("fanSensor");
            }

            fan.index = fanSensor.Index;
            fan.speed = (int?)fanSensor.Value ?? 0;
        }

        public int Index { get => fan.index; }
        public int Speed
        {
            get => fan.speed;
            set => SetProperty(ref fan.speed, value);
        }
    }
}
