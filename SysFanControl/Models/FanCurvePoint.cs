namespace SysFanControl.Models
{
    public class FanCurvePoint : BaseNotifyPropertyChanged
    {
        private int temperature = 0, percent = 0;

        public int Temperature
        {
            get => temperature;
            set => SetProperty(ref temperature, value);
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
