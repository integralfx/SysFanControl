using MahApps.Metro.Controls;
using System;
using GPUFanControl.ViewModels;

namespace GPUFanControl
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
