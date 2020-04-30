using SysFanControl.Models;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using System.Collections.Generic;

namespace SysFanControl.ViewModels
{
    public class MainWindowViewModel : BaseNotifyPropertyChanged, IDisposable
    {
        private readonly Computer computer = new Computer
        {
            MainboardEnabled = true,
            CPUEnabled = true,
            RAMEnabled = true,
            GPUEnabled = true,
            FanControllerEnabled = true
        };
        private readonly IHardware superIO;
        private IHardware selectedHardware;
        private ObservableCollection<SensorEx> selectedHardwareSensors;
        private SensorEx selectedSensor;
        private FanCurve selectedFanCurve;
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0)
        };
        private bool disposed = false;
        private readonly Dictionary<IHardware, ObservableCollection<SensorEx>> hardwareSensorsMapping =
            new Dictionary<IHardware, ObservableCollection<SensorEx>>();

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
                    SelectedHardwareSensors = hardwareSensorsMapping[selectedHardware];

                    if (SelectedSensor == null)
                    {
                        SelectedSensor = SelectedHardwareSensors.First();
                    }
                }
            }
        }
        public ObservableCollection<SensorEx> SelectedHardwareSensors
        {
            get => selectedHardwareSensors;
            private set => SetProperty(ref selectedHardwareSensors, value);
        }
        public SensorEx SelectedSensor
        {
            get => selectedSensor;
            set
            {
                SetProperty(ref selectedSensor, value);
                if (SelectedFanCurve != null)
                {
                    SelectedFanCurve.Source = selectedSensor != null ? new FanCurveSource(selectedSensor.Sensor) : null;
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
                    // Update the selected hardware and sensor with the currently selected fan curve.
                    SelectedHardware = SelectedFanCurve.Source.Sensor.Hardware;
                    SelectedSensor = hardwareSensorsMapping[SelectedHardware]
                        .Where(s => s.Sensor == SelectedFanCurve.Source.Sensor)
                        .First();
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

            Hardware = new ObservableCollection<IHardware>(
                // Don't show hardware with 0 allowed sensors.
                computer.Hardware.Where(h =>
                {
                    if (h.HardwareType == HardwareType.Mainboard)
                    {
                        return true;
                    }
                    return h.Sensors.Where(FanCurveSource.IsSensorAllowed).Count() > 0;
                })
            );

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
                FanCurves.Add(new FanCurve(fanSensor, OnEnabledChanged));
            }

            // Create the hardware to sensors mapping.
            foreach (var hw in Hardware)
            {
                var sensorCollection = new ObservableCollection<SensorEx>();
                var sensors = hw.Sensors.Where(FanCurveSource.IsSensorAllowed);
                if (hw.HardwareType == HardwareType.Mainboard)
                {
                    sensors = superIO.Sensors.Where(FanCurveSource.IsSensorAllowed);
                }
                foreach (var sensor in sensors)
                {
                    sensorCollection.Add(new SensorEx(sensor));
                }
                hardwareSensorsMapping.Add(hw, sensorCollection);
            }

            var gpuHardware = computer.Hardware
                .Where(h => h.HardwareType == HardwareType.GpuAti || h.HardwareType == HardwareType.GpuNvidia);
            if (gpuHardware.Count() == 0)
            {
                throw new HardwareNotDetectedException("No GPU detected.");
            }
            SelectedHardware = gpuHardware.First();
            SelectedHardwareSensors = hardwareSensorsMapping[SelectedHardware];
            SelectedSensor = SelectedHardwareSensors.First();

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
            if (sender.Enabled && sender.Source == null)
            {
                sender.Source = SelectedSensor != null ? new FanCurveSource(SelectedSensor.Sensor) : null;
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

            if (SelectedSensor != null)
            {
                SelectedSensor.Sensor.Hardware.Update();
                PropertyUpdated(nameof(SelectedSensor));
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
