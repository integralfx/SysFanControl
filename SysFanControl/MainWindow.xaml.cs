using MahApps.Metro.Controls;
using SysFanControl.ViewModels;
using System;
using System.Windows;

namespace SysFanControl
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                DataContext = new MainWindowViewModel();
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
    }
}
