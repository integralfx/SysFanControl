using OpenHardwareMonitor.Hardware;
using System.ComponentModel;

namespace GPUFanControl.Models
{
    public class FanCurve : ViewModelBase
    {
        private ISensor _sensor;
        private bool _curveEnabled;
        private BindingList<FanCurvePoint> _points;

        public ISensor Sensor
        {
            get => _sensor;
            set => SetProperty(ref _sensor, value);
        }
        public bool CurveEnabled
        {
            get => _curveEnabled;
            set => SetProperty(ref _curveEnabled, value);
        }
        public BindingList<FanCurvePoint> Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }
    }
}
