using OpenHardwareMonitor.Hardware;
using System;

namespace GPUFanControl.Models
{
    class GPU : BaseNotifyPropertyChanged
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
            get
            {
                gpu.Update();
                var newTemperature = gpu.Sensors[0].Value;
                if (newTemperature.HasValue)
                {
                    SetProperty(ref temperature, (int)newTemperature.Value);
                }

                return temperature;
            }
        }

        public string Name
        {
            get => gpu.Name;
        }
    }
}
