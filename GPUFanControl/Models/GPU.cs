using GPUFanControl.ViewModels;
using OpenHardwareMonitor.Hardware;
using System;

namespace GPUFanControl.Models
{
    public class GPU : HardwareNotifyPropertyChanged
    {
        private readonly IHardware gpu;
        private int temperature = 0;

        public GPU(IHardware gpu)
        {
            if (gpu.HardwareType != HardwareType.GpuAti)
            {
                throw new ArgumentException("gpu");
            }

            this.gpu = gpu;
        }

        public int Temperature
        {
            get => temperature;
            private set => SetProperty(ref temperature, value);
        }

        public string Name
        {
            get => gpu.Name;
        }

        public override void Update()
        {
            gpu.Update();
            var newTemperature = gpu.Sensors[0].Value;
            if (newTemperature.HasValue)
            {
                Temperature = (int)newTemperature.Value;
            }
        }
    }
}
