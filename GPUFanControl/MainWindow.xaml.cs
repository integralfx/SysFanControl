using MahApps.Metro.Controls;
using System;
using System.Windows.Threading;
using GPUFanControl.ViewModels;

namespace GPUFanControl
{
    public partial class MainWindow : MetroWindow
    {
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.0)
        };

        public MainWindowViewModel MainWindowViewModel { get; set; } = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = MainWindowViewModel;

            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            MainWindowViewModel.Update();
        }
    }
}
