using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GPUFanControl.ViewModels
{
    public class FanCurvesControl : BaseViewModel, IDisposable
    {
        private readonly Computer computer = new Computer
        {
            MainboardEnabled = true,
            GPUEnabled = true,
            FanControllerEnabled = true
        };
        private readonly IHardware superIO, gpu;
        private readonly List<ISensor> superIOFans, superIOControls;
        private BindingList<FanCurveViewModel> fanCurves = new BindingList<FanCurveViewModel>();
        private FanCurveViewModel selectedFanCurve;
        private bool disposed = false;
        private float temperature = 0;

        public FanCurvesControl()
        {
            computer.Open();

            superIO = computer.Hardware[0].SubHardware[0];
            superIO.Update();

            superIOFans = superIO.Sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
            foreach (var fan in superIOFans)
            {
                fanCurves.Add(new FanCurveViewModel(fan));
            }

            superIOControls = superIO.Sensors.Where(s => s.SensorType == SensorType.Control).ToList();

            gpu = computer.Hardware[1];
        }

        ~FanCurvesControl()
        {
            Dispose(false);
        }

        public BindingList<FanCurveViewModel> FanCurves
        {
            get => fanCurves;
            set => SetProperty(ref fanCurves, value);
        }
        public FanCurveViewModel SelectedFanCurve
        {
            get => selectedFanCurve != null && selectedFanCurve.CurveEnabled ? selectedFanCurve : null;
            set => SetProperty(ref selectedFanCurve, value);
        }
        public float Temperature
        {
            get => temperature;
            set => SetProperty(ref temperature, value);
        }

        public void Update()
        {
            superIO.Update();
            foreach (var fanCurve in fanCurves)
            {
                fanCurve.Speed = (int?)superIOFans.Find(f => f.Index == fanCurve.Index).Value ?? 0;
            }

            gpu.Update();
            Temperature = gpu.Sensors[0].Value ?? 0;
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
