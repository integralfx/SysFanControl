using GPUFanControl.Models;
using OpenHardwareMonitor.Hardware;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace GPUFanControl.ViewModels
{
    public class MainWindowViewModel : BaseNotifyPropertyChanged, IDisposable
    {
        private readonly Computer computer = new Computer
        {
            MainboardEnabled = true,
            GPUEnabled = true,
            FanControllerEnabled = true
        };
        private FanCurve selectedFanCurve, fanCurveToEdit;
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0)
        };
        private bool disposed = false;

        public GPU GPU { get; }
        public BindingList<FanCurve> FanCurves { get; } = new BindingList<FanCurve>();
        public FanCurve SelectedFanCurve
        {
            get => selectedFanCurve;
            set
            {
                SetProperty(ref selectedFanCurve, value);
                UpdateFanCurveToEdit();
            }
        }
        public FanCurve FanCurveToEdit
        {
            get => fanCurveToEdit;
            private set => SetProperty(ref fanCurveToEdit, value);
        }

        public MainWindowViewModel()
        {
            computer.Open();

            GPU = new GPU(computer.Hardware[1]);

            var superIO = computer.Hardware[0].SubHardware[0];
            superIO.Update();

            var superIOFans = superIO.Sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
            foreach (var fanSensor in superIOFans)
            {
                FanCurves.Add(new FanCurve(fanSensor, GPU, OnEnabledChanged));
            }

            timer.Tick += timer_Tick;
            timer.Start();
        }

        ~MainWindowViewModel()
        {
            Dispose(false);
        }

        private void UpdateFanCurveToEdit()
        {
            if (selectedFanCurve == null)
            {
                return;
            }

            FanCurveToEdit = selectedFanCurve.Enabled ? selectedFanCurve : null;
        }

        private void OnEnabledChanged()
        {
            UpdateFanCurveToEdit();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            GPU.Update();
            foreach (var fanCurve in FanCurves)
            {
                fanCurve.Update();
            }
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
                if (disposing)
                {
                    foreach (var fanCurve in FanCurves)
                    {
                        fanCurve.Dispose();
                    }
                }

                timer.Stop();
                computer.Close();

                disposed = true;
            }
        }
    }
}
