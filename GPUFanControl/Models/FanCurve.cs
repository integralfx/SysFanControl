using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPUFanControl.Models
{
    public class FanCurve : Fan
    {
        private readonly GPU gpu;
        private bool enabled = false;
        public delegate void OnEnabledChanged();
        private readonly OnEnabledChanged onEnabledChanged;

        public FanCurve(ISensor fanSensor, GPU gpu, OnEnabledChanged onEnabledChanged) : 
            base(fanSensor)
        {
            this.gpu = gpu;
            this.onEnabledChanged = onEnabledChanged;

            var points = new List<FanCurvePoint>
            {
                new FanCurvePoint{ Temperature = 40, Percent = 50 },
                new FanCurvePoint{ Temperature = 50, Percent = 75 },
                new FanCurvePoint{ Temperature = 60, Percent = 100 }
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

            if (!enabled)
            {
                return;
            }

            Percent = CalculateFanPercent(gpu.Temperature);
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
                Temperature = point.Temperature,
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
        /// <param name="temperature">Temperature of the hardware that the fan curve is used for.</param>
        /// <returns>The fan percent.</returns>
        private int CalculateFanPercent(int temperature)
        {
            if (temperature < Points.First().Temperature)
            {
                return Points.First().Percent;
            }
            if (temperature > Points.Last().Temperature)
            {
                return Points.Last().Percent;
            }

            // Find which points the temperature falls between.
            for (int i = 0; i < Points.Count - 1; i++)
            {
                var current = Points[i];
                var next = Points[i + 1];
                if (temperature >= current.Temperature && gpu.Temperature <= next.Temperature)
                {
                    // Interpolate between the points.
                    var scale = 1.0 * (next.Percent - current.Percent) / (next.Temperature - current.Temperature);
                    return (int)Math.Round(scale * (temperature - current.Temperature) + current.Percent);
                }
            }

            // Should never get here.
            return Percent;
        }
    }
}
