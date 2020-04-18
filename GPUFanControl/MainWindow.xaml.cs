using MahApps.Metro.Controls;
using System;
using System.Windows.Threading;
using GPUFanControl.Models;

namespace GPUFanControl
{
    public partial class MainWindow : MetroWindow
    {
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0)
        };

        public FanCurvesControl FanCurvesCtrl { get; set; } = new FanCurvesControl();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            FanCurvesCtrl.UpdateFans();
            listViewFans.Items.Refresh();
        }
    }
}
