using SysFanControl.Models;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace SysFanControl.ViewModels
{
    public class MainWindowViewModel : BaseNotifyPropertyChanged, IDisposable
    {
        private readonly Computer computer = new Computer
        {
            MainboardEnabled = true,
            CPUEnabled = true,
            GPUEnabled = true,
            FanControllerEnabled = true
        };
        private readonly IHardware superIO;
        private IHardware selectedHardware;
        private ObservableCollection<ISensor> selectedHardwareSensors;
        private ISensor selectedSensor;
        private FanCurve selectedFanCurve;
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0)
        };
        private bool disposed = false;

        public string Version { get => "0.2.0"; }
        public string Title { get => $"Sys Fan Control v{Version}"; }
        public ObservableCollection<IHardware> Hardware { get; }
        public IHardware SelectedHardware
        {
            get => selectedHardware;
            set
            {
                SetProperty(ref selectedHardware, value);
                if (selectedHardware != null)
                {
                    if (selectedHardware.HardwareType == HardwareType.Mainboard)
                    {
                        SelectedHardwareSensors = new ObservableCollection<ISensor>(
                            superIO.Sensors.Where(FanCurveSource.IsSensorAllowed)
                        );
                    }
                    else
                    {
                        SelectedHardwareSensors = new ObservableCollection<ISensor>(
                            selectedHardware.Sensors.Where(FanCurveSource.IsSensorAllowed)
                        );
                    }
                }
            }
        }
        public ObservableCollection<ISensor> SelectedHardwareSensors
        {
            get => selectedHardwareSensors;
            private set => SetProperty(ref selectedHardwareSensors, value);
        }
        public ISensor SelectedSensor
        {
            get => selectedSensor;
            set
            {
                SetProperty(ref selectedSensor, value);
                if (SelectedFanCurve != null)
                {
                    SelectedFanCurve.Source = selectedSensor != null ? new FanCurveSource(selectedSensor) : null;
                }
            }
        }
        public BindingList<FanCurve> FanCurves { get; } = new BindingList<FanCurve>();
        public FanCurve SelectedFanCurve
        {
            get => selectedFanCurve != null && selectedFanCurve.Enabled ? selectedFanCurve : null;
            set
            {
                SetProperty(ref selectedFanCurve, value);
                if (SelectedFanCurve != null)
                {
                    SelectedSensor = SelectedFanCurve.Source.Sensor;
                    SelectedHardware = SelectedSensor.Hardware;
                }
            }
        }

        /// <summary>
        /// MainWindowViewModel constructor.
        /// </summary>
        /// <exception cref="HardwareNotDetectedException">
        /// Thrown when CPU, GPU, motherboard or SuperIO could not be detected.
        /// </exception>
        public MainWindowViewModel()
        {
            computer.Open();

            if (computer.Hardware.Length == 0)
            {
                throw new HardwareNotDetectedException("No hardware detected. Try running as admin.");
            }

            Hardware = new ObservableCollection<IHardware>(computer.Hardware);

            var gpuHardware = computer.Hardware
                .Where(h => h.HardwareType == HardwareType.GpuAti || h.HardwareType == HardwareType.GpuNvidia);
            if (gpuHardware.Count() == 0)
            {
                throw new HardwareNotDetectedException("No GPU detected.");
            }
            SelectedHardware = gpuHardware.First();
            SelectedSensor = SelectedHardware.Sensors.First();

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
            superIO = superIOHardware.First();
            superIO.Update();

            var superIOFans = superIO.Sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
            foreach (var fanSensor in superIOFans)
            {
                FanCurves.Add(
                    new FanCurve(fanSensor, OnEnabledChanged)
                    {
                        Source = new FanCurveSource(SelectedSensor)
                    }
                );
            }

            timer.Tick += timer_Tick;
            timer.Start();
        }

        ~MainWindowViewModel()
        {
            Dispose(false);
        }

        private void OnEnabledChanged(FanCurve sender)
        {
            // Checked curve, so update the source with the currently selected sensor.
            if (sender.Enabled)
            {
                sender.Source = SelectedSensor != null ? new FanCurveSource(SelectedSensor) : null;
            }

            if (sender == SelectedFanCurve)
            {
                PropertyUpdated(nameof(SelectedFanCurve));
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
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
