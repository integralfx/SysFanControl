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
        public BindingList<FanCurve> FanCurves { get; } = new BindingList<FanCurve>();>
        public FanCurve SelectedFanCurve
        {
            get => selectedFanCurve;
            set
            {
                SetProperty(ref selectedFanCurve, value);
                UpdateFanCurveToEdit();
            }
        }
        // SelectedFanCurve must have the curve checkbox checked to be able to edit it.
        public FanCurve FanCurveToEdit
        {
            get => fanCurveToEdit;
            private set => SetProperty(ref fanCurveToEdit, value);
        }

        /// <summary>
        /// MainWindowViewModel constructor.
        /// </summary>
        /// <exception cref="HardwareNotFoundException">
        /// Thrown when GPU, motherboard or SuperIO could not be found.
        /// </exception>
        public MainWindowViewModel()
        {
            computer.Open();

            if (computer.Hardware.Length == 0)
            {
                throw new HardwareNotFoundException("No hardware found. Try running as admin.");
            }

            var gpuHardware = computer.Hardware
                .Where(h => h.HardwareType == HardwareType.GpuAti || h.HardwareType == HardwareType.GpuNvidia);
            if (gpuHardware.Count() == 0)
            {
                throw new HardwareNotFoundException("No GPU found.");
            }
            GPU = new GPU(gpuHardware.First());

            var moboHardware = computer.Hardware.Where(h => h.HardwareType == HardwareType.Mainboard);
            if (moboHardware.Count() == 0)
            {
                throw new HardwareNotFoundException("No motherboard found.");
            }
            var superIOHardware = moboHardware.First().SubHardware.Where(h => h.HardwareType == HardwareType.SuperIO);
            if (superIOHardware.Count() == 0)
            {
                throw new HardwareNotFoundException("No SuperIO found.");
            }
            var superIO = superIOHardware.First();
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

        public class HardwareNotFoundException : Exception
        {
            public HardwareNotFoundException(string message) : base(message)
            {

            }
        }
    }
}
