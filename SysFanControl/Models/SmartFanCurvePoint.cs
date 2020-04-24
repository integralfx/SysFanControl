namespace SysFanControl.Models
{
    public class SmartFanCurvePoint : FanCurvePoint
    {
        public new int Temperature
        {
            get => base.Temperature;
            set
            {
                // Current temperature must be at least 1 greater than the previous temperature.
                if (PreviousPoint != null && value < PreviousPoint.Temperature + 1)
                {
                    return;
                }
                // Current temperature must be at least 1 less than the next temperature.
                if (NextPoint != null && value > NextPoint.Temperature - 1)
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
                // Current percent must be greater than or equal to previous percent.
                if (PreviousPoint != null && value < PreviousPoint.Percent)
                {
                    return;
                }
                // Current percent must be less than or equal to next percent.
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
