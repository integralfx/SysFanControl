namespace GPUFanControl.Models
{
    public class SmartFanCurvePoint : FanCurvePoint
    {
        public new int Temperature
        {
            get => base.Temperature;
            set
            {
                if (PreviousPoint != null && value - 1 < PreviousPoint.Temperature)
                {
                    return;
                }
                if (NextPoint != null && value + 1 > NextPoint.Temperature)
                {
                    return;
                }

                base.Temperature = value;
            }
        }
        public new int Percent
        {
            get => base.Percent;
            set
            {
                if (PreviousPoint != null && value < PreviousPoint.Percent)
                {
                    return;
                }
                if (NextPoint != null && value > NextPoint.Percent)
                {
                    return;
                }

                base.Percent = value;
            }
        }

        public FanCurvePoint PreviousPoint { get; set; }
        public FanCurvePoint NextPoint { get; set; }
    }
}
