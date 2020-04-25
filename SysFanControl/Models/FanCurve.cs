using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SysFanControl.Models
{
    public class FanCurve : Fan
    {
        private readonly FanCurveSource source;
        private bool enabled = false;
        public delegate void OnEnabledChanged();
        private readonly OnEnabledChanged onEnabledChanged;

        public FanCurve(ISensor fanSensor, FanCurveSource source, OnEnabledChanged onEnabledChanged) : 
            base(fanSensor)
        {
            this.source = source;
            this.onEnabledChanged = onEnabledChanged;

            var points = new List<FanCurvePoint>
            {
                new FanCurvePoint{ SourceValue = 40, Percent = 50 },
                new FanCurvePoint{ SourceValue = 50, Percent = 75 },
                new FanCurvePoint{ SourceValue = 60, Percent = 100 }
            };
            foreach (var point in points)
            {
                AddPoint(point);
            }
        }

        public bool Enabled
        {
            get => enabled;
            set
            {
                var previousValue = enabled;
                SetProperty(ref enabled, value);

                if (!value)
                {
                    SetDefault();
                }

                if (previousValue != value)
                {
                    onEnabledChanged();
                }
            }
        }
        public List<SmartFanCurvePoint> Points { get; } = new List<SmartFanCurvePoint>();

        public override void Update()
        {
            base.Update();
            source.Update();

            if (!enabled)
            {
                return;
            }

            Percent = CalculateFanPercent(source.Value);
        }

        /// <summary>
        /// Add a point.
        /// </summary>
        /// <param name="point">
        /// The point to add. Should be greater than all the points in <see cref="Points"/>.
        /// </param>
        private void AddPoint(FanCurvePoint point)
        {
            var previous = Points.Count() > 0 ? Points.Last() : null;
            var current = new SmartFanCurvePoint
            {
                SourceValue = point.SourceValue,
                Percent = point.Percent,
                PreviousPoint = previous
            };
            Points.Add(current);
            if (previous != null)
            {
                previous.NextPoint = current;
            }
        }

        /// <summary>
        /// Calculate the speed (in percent) that the fan should be at.
        /// </summary>
        /// <param name="sourceValue">Value of the sensor that the fan curve is used for.</param>
        /// <returns>The fan percent.</returns>
        private int CalculateFanPercent(float sourceValue)
        {
            if (sourceValue < Points.First().SourceValue)
            {
                return Points.First().Percent;
            }
            if (sourceValue > Points.Last().SourceValue)
            {
                return Points.Last().Percent;
            }

            // Find which points the temperature falls between.
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var current = Points[i];
                var next = Points[i + 1];
                if (sourceValue >= current.SourceValue && source.Value <= next.SourceValue)
                {
                    // Interpolate between the points.
                    var scale = 1.0 * (next.Percent - current.Percent) / (next.SourceValue - current.SourceValue);
                    return (int)Math.Round(scale * (sourceValue - current.SourceValue) + current.Percent);
                }
            }

            // Should never get here.
            return Percent;
        }
    }
}
