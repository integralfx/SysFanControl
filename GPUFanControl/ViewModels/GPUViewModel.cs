using GPUFanControl.Models;
using OpenHardwareMonitor.Hardware;

namespace GPUFanControl.ViewModels
{
    public class GPUViewModel : HardwareViewModel
    {
        private readonly GPU gpu;

        public GPUViewModel(IHardware gpu)
        {
            this.gpu = new GPU(gpu);
        }

        public int Temperature { get => gpu.Temperature; }
        public string Name { get => gpu.Name; }

        public override void Update()
        {
            //PropertyUpdated(nameof(gpu.Temperature));
        }
    }
}
