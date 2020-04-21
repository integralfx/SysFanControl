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
        private FanCurve selectedFanCurve;
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0)
        };
        private bool disposed = false;

        public GPU GPU { get; }
        public BindingList<FanCurve> FanCurves { get; } = new BindingList<FanCurve>();
        public FanCurve SelectedFanCurve
        {
            get => selectedFanCurve != null && selectedFanCurve.Enabled ? selectedFanCurve : null;
            set
            {
                SetProperty(ref selectedFanCurve, value);
            }
        }

        /// <summary>
        /// MainWindowViewModel constructor.
        /// </summary>
        /// <exception cref="HardwareNotDetectedException">
        /// Thrown when GPU, motherboard or SuperIO could not be found.
        /// </exception>
        public MainWindowViewModel()
        {
            computer.Open();

            if (computer.Hardware.Length == 0)
            {
                throw new HardwareNotDetectedException("No hardware detected. Try running as admin.");
            }

            var gpuHardware = computer.Hardware
                .Where(h => h.HardwareType == HardwareType.GpuAti || h.HardwareType == HardwareType.GpuNvidia);
            if (gpuHardware.Count() == 0)
            {
                throw new HardwareNotDetectedException("No GPU detected.");
            }
            GPU = new GPU(gpuHardware.First());

            var moboHardware = computer.Hardware.Where(h => h.HardwareType == HardwareType.Mainboard);
            if (moboHardware.Count() == 0)
            {
                throw new HardwareNotDetectedException("No motherboard detected.");
            }
            var superIOHardware = moboHardware.First().SubHardware.Where(h => h.HardwareType == HardwareType.SuperIO);
            if (superIOHardware.Count() == 0)
            {
                throw new HardwareNotDetectedException("No SuperIO detected.");
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

        private void OnEnabledChanged()
        {
            PropertyUpdated(nameof(SelectedFanCurve));
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

        public class HardwareNotDetectedException : Exception
        {
            public HardwareNotDetectedException(string message) : base(message)
            {

            }
        }
    }
}
