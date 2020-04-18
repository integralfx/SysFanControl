using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using OpenHardwareMonitor.Hardware;
using System.ComponentModel;
using GPUFanControl.Models;

namespace GPUFanControl
{
    public partial class MainWindow : MetroWindow
    {
        private readonly Computer computer = new Computer
        {
            MainboardEnabled = true,
            GPUEnabled = true,
            FanControllerEnabled = true,
        };
        private readonly List<ISensor> superIOFans, superIOControls;
        private readonly IHardware superIO;
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0)
        };

        public FanCurvesControl FanCurvesCtrl { get; set; } = new FanCurvesControl
        {
            FanCurves = new BindingList<FanCurve>()
        };

        public MainWindow()
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
                FanCurvesCtrl.FanCurves.Add(new FanCurve { Sensor = fan, Points = points });
            }
            superIOControls = superIO.Sensors.Where(s => s.SensorType == SensorType.Control).ToList();

            InitializeComponent();
            DataContext = this;

            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            timer.Stop();   
            foreach (var control in superIOControls)
            {
                control.Control.SetDefault();
            }
            computer?.Close();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            var gpuTemp = computer.Hardware[1].Sensors[0].Value;
            if (gpuTemp.HasValue)
            {
                labelTemperature.Content = $"{gpuTemp.Value.ToString()}°C";
            }

            superIO.Update();
            listViewFans.Items.Refresh();
        }
    }
}
