using OpenHardwareMonitor.Hardware;
using System;

namespace GPUFanControl.Models
{
    class GPU
    {
        private readonly IHardware gpu;

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
                return (int?)gpu.Sensors[0].Value ?? 0;
            }
        }

        public string Name
        {
            get => gpu.Name;
        }
    }
}
