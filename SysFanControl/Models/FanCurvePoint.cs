namespace SysFanControl.Models
{
    public class FanCurvePoint : BaseNotifyPropertyChanged
    {
        private int sourceValue = 0, percent = 0;

        public int Value
        {
            get => sourceValue;
            set => SetProperty(ref sourceValue, value);
        }
        public int Percent
        {
            get => percent;
            set
            {
                if (value >= 0 && value <= 100)
                {
                    SetProperty(ref percent, value);
                }
            }
        }
    }
}
