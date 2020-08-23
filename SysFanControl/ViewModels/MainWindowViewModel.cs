using SysFanControl.Models;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;

namespace SysFanControl.ViewModels
{
    public class MainWindowViewModel : BaseNotifyPropertyChanged, IDisposable
    {
        private const string settingsFile = "SysFanControl.json";
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
        private readonly DispatcherTimer timer;
        private bool disposed = false;
        private readonly Dictionary<IHardware, ObservableCollection<SensorEx>> hardwareSensorsMapping =
            new Dictionary<IHardware, ObservableCollection<SensorEx>>();
        private double pollingInterval;

        public string Version { get => "0.5.0"; }
        public string Title { get => $"SFC v{Version}"; }
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
                // Update the selected hardware and selected sensor with the selected fan curve.
                if (SelectedFanCurve != null)
                {
                    // Changing SelectedHardware causes Source.Sensor to change to the first sensor.
                    var sourceSensor = SelectedFanCurve.Source.Sensor;
                    SelectedHardware = SelectedFanCurve.Source.Sensor.Hardware;
                    SelectedSensor = hardwareSensorsMapping[SelectedHardware]
                        .Where(s => s.Sensor == sourceSensor)
                        .First();
                }
            }
        }
        public double PollingInterval
        { 
            get => pollingInterval; 
            set 
            {
                SetProperty(ref pollingInterval, value);

                timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(value);
                timer.Start();
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

            var gpuHardware = computer.Hardware
                .Where(h => h.HardwareType == HardwareType.GpuAti || h.HardwareType == HardwareType.GpuNvidia);
            if (gpuHardware.Count() == 0)
            {
                throw new HardwareNotDetectedException("No GPU detected.");
            }

            superIO = superIOHardware.First();
            superIO.Update();

            var superIOFans = superIO.Sensors.Where(s => s.SensorType == SensorType.Fan).ToList();
            foreach (var fanSensor in superIOFans)
            {
                FanCurves.Add(new FanCurve(fanSensor, OnEnabledChanged));
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

            SelectedHardware = gpuHardware.First();
            SelectedHardwareSensors = hardwareSensorsMapping[SelectedHardware];
            SelectedSensor = SelectedHardwareSensors.First();

            LoadSettings();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(PollingInterval)
            };
            timer.Tick += timer_Tick;
            timer.Start();
        }

        ~MainWindowViewModel()
        {
            Dispose(false);
        }

        private void SaveSettings()
        {
            var root = new JObject
            {
                ["pollingInterval"] = PollingInterval
            };
            foreach (var fanCurve in FanCurves)
            {
                root[fanCurve.Sensor.Identifier.ToString()] = FanCurveJSON.Serialise(fanCurve);
            }

            try
            {
                File.WriteAllText(settingsFile, root.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    $"{e.Message}:\n{e.StackTrace}",
                    "Error: Failed to save settings",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void LoadSettings()
        {
            if (!File.Exists(settingsFile))
            {
                return;
            }

            try
            {
                var root = JObject.Parse(File.ReadAllText(settingsFile));

                if (root.ContainsKey("pollingInterval"))
                {
                    pollingInterval = root["pollingInterval"].ToObject<double>();
                }
                else
                {
                    pollingInterval = 2.0;
                }

                // Load the fan curves.
                var list = root.Properties().Join(
                    FanCurves, 
                    p => p.Name, 
                    f => f.Sensor.Identifier.ToString(), 
                    (p, f) => new Tuple<JProperty, FanCurve>(p, f)
                );
                foreach (var obj in list)
                {
                    FanCurveJSON.UpdateFromJSON((JObject)obj.Item1.Value, obj.Item2, Hardware.ToList());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    $"{e.Message}:\n{e.StackTrace}",
                    "Error: Failed to parse settings",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
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

                SaveSettings();
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
