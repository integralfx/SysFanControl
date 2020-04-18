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
            new FanCurvePoint(40, 50),
            new FanCurvePoint(50, 70),
            new FanCurvePoint(60, 100)
        };
    }
}
