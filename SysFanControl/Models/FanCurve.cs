using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SysFanControl.Models
{
    public class FanCurve : Fan
    {
        private FanCurveSource source;
        private bool enabled = false;
        public delegate void OnEnabledChanged(FanCurve sender);
        private readonly OnEnabledChanged onEnabledChanged;

        public FanCurve(ISensor fanSensor, OnEnabledChanged onEnabledChanged) : 
            base(fanSensor)
        {
            this.onEnabledChanged = onEnabledChanged;

            var points = new List<FanCurvePoint>
            {
                new FanCurvePoint{ Value = 40.0M, Percent = 50 },
                new FanCurvePoint{ Value = 50.0M, Percent = 75 },
                new FanCurvePoint{ Value = 60.0M, Percent = 100 }
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
                    onEnabledChanged(this);
                }
            }
        }
        public List<SmartFanCurvePoint> Points { get; } = new List<SmartFanCurvePoint>();
        public FanCurveSource Source
        {
            get => source;
            set => SetProperty(ref source, value);
        }

        public override void Update()
        {
            base.Update();
            if (Source != null)
            {
                Source.Update();
                PropertyUpdated(nameof(Source));
            }

            if (!enabled)
            {
                return;
            }

            if (Source != null)
            {
                Percent = CalculateFanPercent(Source.Value);
            }
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
            var current = new SmartFanCurvePoint(previous, point);
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
            var sourceVal = (decimal)sourceValue;

            if (sourceVal < Points.First().Value)
            {
                return Points.First().Percent;
            }
            if (sourceVal > Points.Last().Value)
            {
                return Points.Last().Percent;
            }

            // Find which points sourceValue falls between.
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var current = Points[i];
                var next = Points[i + 1];
                if (sourceVal >= current.Value && (decimal)source.Value <= next.Value)
                {
                    // Interpolate between the points.
                    var scale = (next.Percent - current.Percent) / (next.Value - current.Value);
                    return (int)Math.Round(scale * (sourceVal - current.Value) + current.Percent);
                }
            }

            // Should never get here.
            return Percent;
        }
    }
}
