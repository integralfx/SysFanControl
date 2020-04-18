using System.ComponentModel;

namespace GPUFanControl.Models
{
    public class FanCurvesControl : ViewModelBase
    {
        private BindingList<FanCurve> _fanCurves;
        private FanCurve _selectedFanCurve;

        public BindingList<FanCurve> FanCurves
        {
            get => _fanCurves;
            set => SetProperty(ref _fanCurves, value);
        }
        public FanCurve SelectedFanCurve
        {
            get => _selectedFanCurve;
            set => SetProperty(ref _selectedFanCurve, value);
        }
    }
}
