using GPUFanControl.Models;
using OpenHardwareMonitor.Hardware;
using System.Collections.Generic;
using System.Linq;

namespace GPUFanControl.ViewModels
{
    public class MainWindowViewModel : HardwareViewModel
    {
        private readonly Computer computer = new Computer
        {
            MainboardEnabled = true,
            GPUEnabled = true,
            FanControllerEnabled = true
        };
        private readonly List<ISensor> superIOControls;
        private FanCurve selectedFanCurve;

        public GPUViewModel GPUViewModel { get; }
        public FanCurvesViewModel FanCurvesViewModel { get; }
        public FanCurve SelectedFanCurve
        {
            get => selectedFanCurve;
            set => SetProperty(ref selectedFanCurve, value.Enabled ? value : null);
        }

        public MainWindowViewModel()
        {
            computer.Open();

            GPUViewModel = new GPUViewModel(computer.Hardware[1]);

            var superIO = computer.Hardware[0].SubHardware[0];
            superIO.Update();

            FanCurvesViewModel = new FanCurvesViewModel(superIO);

            superIOControls = superIO.Sensors.Where(s => s.SensorType == SensorType.Control).ToList();
        }

        public override void Update()
        {
            GPUViewModel.Update();
            FanCurvesViewModel.Update();
        }
    }
}
