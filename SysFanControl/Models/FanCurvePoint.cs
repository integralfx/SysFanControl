namespace SysFanControl.Models
{
    public class FanCurvePoint : BaseNotifyPropertyChanged
    {
        protected static readonly int decimalPlaces = 1;

        private decimal value = 0;
        private int percent = 0;

        public decimal Value
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

        public static bool operator<(FanCurvePoint lhs, FanCurvePoint rhs)
        {
            return lhs.Value < rhs.Value && lhs.Percent <= rhs.Percent;
        }
        public static bool operator>(FanCurvePoint lhs, FanCurvePoint rhs)
        {
            return lhs.Value > rhs.Value && lhs.Percent >= rhs.Percent;
        }
    }
}
