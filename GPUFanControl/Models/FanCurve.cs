using GPUFanControl.ViewModels;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;

namespace GPUFanControl.Models
{
    public class FanCurve : HardwareViewModel
    {
        private readonly FanViewModel fan;
        private bool enabled = false;

        public FanCurve(ISensor fanSensor)
        {
            fan = new FanViewModel(fanSensor);
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
            set => SetProperty(ref enabled, value);
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
