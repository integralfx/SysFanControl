namespace GPUFanControl.Models
{
    public class FanCurvePoint : ViewModelBase
    {
        private int _temperature, _percent;

        public int Temperature
        {
            get => _temperature;
            set => SetProperty(ref _temperature, value);
        }
        public int Percent
        {
            get => _percent;
            set => SetProperty(ref _percent, value);
        }
    }
}
