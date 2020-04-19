using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPUFanControl.Models
{
    public class FanCurve : Fan
    {
        private GPU gpu;
        private bool enabled = false;
        public delegate void OnEnabledChanged(bool newValue);
        private OnEnabledChanged onEnabledChanged;

        public FanCurve(ISensor fanSensor, GPU gpu, OnEnabledChanged onEnabledChanged) : 
            base(fanSensor)
        {
            this.gpu = gpu;
            this.onEnabledChanged = onEnabledChanged;
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
                    onEnabledChanged(value);
                }
            }
        }
        public List<FanCurvePoint> Points { get; } = new List<FanCurvePoint>
        {
            new FanCurvePoint{ Temperature = 40, Percent = 50 },
            new FanCurvePoint{ Temperature = 50, Percent = 75 },
            new FanCurvePoint{ Temperature = 60, Percent = 100 }
        };

        public override void Update()
        {
            base.Update();

            if (!enabled)
            {
                return;
            }

            var temperature = gpu.Temperature;
            if (temperature < Points.First().Temperature)
            {
                Percent = Points.First().Percent;
            }
            else if (temperature > Points.Last().Temperature)
            {
                Percent = Points.Last().Percent;
            }
            else
            {
                for (int i = 0; i < Points.Count - 1; i++)
                {
                    var current = Points[i];
                    var next = Points[i + 1];
                    if (temperature >= current.Temperature && gpu.Temperature <= next.Temperature)
                    {
                        Percent = (int)Math.Round(1.0 * temperature / current.Temperature * current.Percent);
                        break;
                    }
                }
            }
        }
    }
}
