using GPUFanControl.Models;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace GPUFanControl.ViewModels
{
    public class MainWindowViewModel : BaseNotifyPropertyChanged
    {
        private readonly Computer computer = new Computer
        {
            MainboardEnabled = true,
            GPUEnabled = true,
            FanControllerEnabled = true
        };
        private readonly List<ISensor> superIOControls;
        private FanCurve selectedFanCurve;
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0)
        };

        public GPU GPU { get; }
        public BindingList<FanCurve> FanCurves { get; } = new BindingList<FanCurve>();
        public FanCurve SelectedFanCurve
        {
            get => selectedFanCurve != null && selectedFanCurve.Enabled ? selectedFanCurve : null;
            set => SetProperty(ref selectedFanCurve, value);
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

            superIOControls = superIO.Sensors.Where(s => s.SensorType == SensorType.Control).ToList();

            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void OnEnabledChanged(bool newValue)
        {
            if (selectedFanCurve == null)
            {
                return;
            }

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
    }
}
