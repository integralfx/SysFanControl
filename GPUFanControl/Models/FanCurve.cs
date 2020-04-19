using GPUFanControl.ViewModels;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;

namespace GPUFanControl.Models
{
    public class FanCurve : HardwareNotifyPropertyChanged
    {
        private readonly Fan fan;
        private bool enabled = false;
        public delegate void OnEnabledChanged(bool newValue);
        private OnEnabledChanged onEnabledChanged;

        public FanCurve(ISensor fanSensor, OnEnabledChanged onEnabledChanged)
        {
            fan = new Fan(fanSensor);
            this.onEnabledChanged = onEnabledChanged;
        }

        public int Index
        {
            get => fan.Index;
        }
        public int Speed
        {
            get => fan.Speed;
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

        public override void Update()
        {
            fan.Update();
            PropertyUpdated(nameof(Speed));
        }

        public bool SetPoint(int index, FanCurvePoint newPoint)
        {
            if (index < 0 || index > Points.Count - 1)
            {
                return false;
            }

            try
            {
                Points[index].Temperature = newPoint.Temperature;
                Points[index].Percent = newPoint.Percent;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}
