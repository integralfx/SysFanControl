using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GPUFanControl.Models
{
    public class FanCurvesControl : ViewModelBase, IDisposable
    {
        private readonly Computer computer = new Computer
        {
            MainboardEnabled = true,
            GPUEnabled = true,
            FanControllerEnabled = true
        };
        private readonly IHardware superIO;
        private readonly List<ISensor> superIOFans, superIOControls;
        private BindingList<FanCurve> fanCurves = new BindingList<FanCurve>();
        private FanCurve selectedFanCurve;
        private bool disposed = false;

        public FanCurvesControl()
        {
            computer.Open();

            superIO = computer.Hardware[0].SubHardware[0];
            superIO.Update();

            superIOFans = superIO.Sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
            foreach (var fan in superIOFans)
            {
                var points = new BindingList<FanCurvePoint>
                {
                    new FanCurvePoint { Temperature = 40, Percent = 50 },
                    new FanCurvePoint { Temperature = 50, Percent = 70 },
                    new FanCurvePoint { Temperature = 60, Percent = 100 }
                };
                fanCurves.Add(new FanCurve { Sensor = fan, Points = points });
            }

            superIOControls = superIO.Sensors.Where(s => s.SensorType == SensorType.Control).ToList();
        }

        ~FanCurvesControl()
        {
            Dispose(false);
        }

        public BindingList<FanCurve> FanCurves
        {
            get => fanCurves;
            set => SetProperty(ref fanCurves, value);
        }
        public FanCurve SelectedFanCurve
        {
            get => selectedFanCurve;
            set => SetProperty(ref selectedFanCurve, value);
        }
        public float Temperature
        {
            get => computer.Hardware[1].Sensors[0].Value ?? 0;
        }

        public void UpdateFans()
        {
            superIO.Update();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                foreach (var control in superIOControls)
                {
                    control.Control.SetDefault();
                }

                computer.Close();

                disposed = true;
            }
        }
    }
}
