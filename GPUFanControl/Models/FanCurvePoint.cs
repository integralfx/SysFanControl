namespace GPUFanControl.Models
{
    public class FanCurvePoint
    {
        public int temperature = 0, percent = 0;

        public FanCurvePoint(int temperature, int percent)
        {
            this.temperature = temperature;
            this.percent = percent;
        }

        public int Temperature
        {
            get => temperature;
            set => temperature = value;
        }
        public int Percent
        {
            get => percent;
            set => percent = value;
        }
    }
}
