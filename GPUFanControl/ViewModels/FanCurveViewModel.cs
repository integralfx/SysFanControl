using GPUFanControl.Models;
using OpenHardwareMonitor.Hardware;
using System.ComponentModel;

namespace GPUFanControl.ViewModels
{
    public class FanCurveViewModel : FanViewModel
    {
        private bool curveEnabled = false;

        public FanCurveViewModel(ISensor fanSensor) : base(fanSensor)
        {

        }

        public bool CurveEnabled
        {
            get => curveEnabled;
            set => SetProperty(ref curveEnabled, value);
        }
        public BindingList<FanCurvePoint> Points { get; } = new BindingList<FanCurvePoint>
        {
            new FanCurvePoint{ Temperature = 40, Percent = 50 },
            new FanCurvePoint{ Temperature = 50, Percent = 70 },
            new FanCurvePoint{ Temperature = 60, Percent = 100 }
        };
    }
}
