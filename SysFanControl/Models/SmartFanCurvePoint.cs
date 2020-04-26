using System;

namespace SysFanControl.Models
{
    /// <summary>
    /// An extension of <see cref="FanCurvePoint"/> that ensures that the current point is between 
    /// <see cref="PreviousPoint"/> and <see cref="NextPoint"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the current point is not between <see cref="PreviousPoint"/> and <see cref="NextPoint"/>.
    /// </exception>
    public class SmartFanCurvePoint : FanCurvePoint
    {
        private FanCurvePoint previousPoint, nextPoint;

        public SmartFanCurvePoint(FanCurvePoint previousPoint, FanCurvePoint point)
        {
            if (previousPoint != null && previousPoint > point)
            {
                throw new ArgumentException("Previous point must be less than point.");
            }

            Value = point.Value;
            Percent = point.Percent;
            PreviousPoint = previousPoint;
        }

        public new int Value
        {
            get => base.Value;
            set
            {
                // Current value must be greater than previous value.
                if (value <= PreviousPoint?.Value)
                {
                    return;
                }
                // Current value must be less than next value.
                if (value >= NextPoint?.Value)
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
                if (value < PreviousPoint?.Percent)
                {
                    return;
                }
                // Current percent must be less than or equal to next percent.
                if (value > NextPoint?.Percent)
                {
                    return;
                }

                base.Percent = value;
            }
        }

        public FanCurvePoint PreviousPoint
        {
            get => previousPoint;
            set
            {
                if (!SetPreviousPoint(value))
                {
                    throw new ArgumentException("Invalid previous point.");
                }
            }
        }
        public FanCurvePoint NextPoint
        {
            get => nextPoint;
            set
            {
                if (!SetNextPoint(value))
                {
                    throw new ArgumentException("Invalid next point.");
                }
            }
        }

        private bool SetPreviousPoint(FanCurvePoint point)
        {
            var previousPoint = PreviousPoint ?? point;
            if (previousPoint == null)
            {
                return true;
            }

            if (previousPoint > this)
            {
                return false;
            }

            SetProperty(ref this.previousPoint, point, nameof(PreviousPoint));
            return true;
        }
        private bool SetNextPoint(FanCurvePoint point)
        {
            var nextPoint = NextPoint ?? point;
            if (nextPoint == null)
            {
                return true;
            }

            if (nextPoint < this)
            {
                return false;
            }

            SetProperty(ref this.nextPoint, point, nameof(NextPoint));
            return true;
        }
    }
}
