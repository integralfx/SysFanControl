using OpenHardwareMonitor.Hardware;
using System.Collections.Generic;

namespace GPUFanControl.Models
{
    public class FanCurve : Fan
    {
        private bool enabled = false;
        public delegate void OnEnabledChanged(bool newValue);
        private OnEnabledChanged onEnabledChanged;

        public FanCurve(ISensor fanSensor, OnEnabledChanged onEnabledChanged) : base(fanSensor)
        {
            this.onEnabledChanged = onEnabledChanged;
        }

        public bool Enabled
        {
            get => enabled;
            set
            {
                var previousValue = enabled;
                SetProperty(ref enabled, value);

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
    }
}
