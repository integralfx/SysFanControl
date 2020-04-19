using GPUFanControl.Models;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace GPUFanControl.ViewModels
{
    public class FanCurvesViewModel : HardwareViewModel
    {
        private readonly IHardware superIO;

        public FanCurvesViewModel(IHardware superIO)
        {
            if (superIO.HardwareType != HardwareType.SuperIO)
            {
                throw new ArgumentException("superIO");
            }

            this.superIO = superIO;
            superIO.Update();
            foreach (var fanSensor in superIO.Sensors.Where(s => s.SensorType == SensorType.Fan))
            {
                FanCurves.Add(new FanCurve(fanSensor));
            }
        }

        public BindingList<FanCurve> FanCurves { get; } = new BindingList<FanCurve>();

        public override void Update()
        {
            foreach (var fan in FanCurves)
            {
                fan.Update();
            }
        }
    }
}
