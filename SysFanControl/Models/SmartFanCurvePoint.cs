namespace SysFanControl.Models
{
    public class SmartFanCurvePoint : FanCurvePoint
    {
        public new int Value
        {
            get => base.Value;
            set
            {
                // Current source value must be at least 1 greater than the previous source value.
                if (PreviousPoint != null && value < PreviousPoint.Value + 1)
                {
                    return;
                }
                // Current source value must be at least 1 less than the next source value.
                if (NextPoint != null && value > NextPoint.Value - 1)
                {
                    return;
                }

                base.Value = value;
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
