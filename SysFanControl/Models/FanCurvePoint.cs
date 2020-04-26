namespace SysFanControl.Models
{
    public class FanCurvePoint : BaseNotifyPropertyChanged
    {
        private int value = 0, percent = 0;

        public int Value
        {
            get => value;
            set => SetProperty(ref this.value, value);
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
