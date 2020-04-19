using GPUFanControl.Models;
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
                FanCurves.Add(new FanCurve(fan));
            }

            superIOControls = superIO.Sensors.Where(s => s.SensorType == SensorType.Control).ToList();

            gpu = computer.Hardware[1];
        }

        ~FanCurvesControl()
        {
            Dispose(false);
        }

        public BindingList<FanCurve> FanCurves { get; } = new BindingList<FanCurve>();
        public FanCurve SelectedFanCurve
        {
            get => selectedFanCurve != null && selectedFanCurve.Enabled ? selectedFanCurve : null;
            set => SetProperty(ref selectedFanCurve, value);
        }
        public float Temperature
        {
            get => gpu.Sensors[0].Value ?? 0;
        }

        public void Update()
        {
            superIO.Update();
            PropertyUpdated(nameof(FanCurves));

            gpu.Update();
            PropertyUpdated(nameof(Temperature));
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
