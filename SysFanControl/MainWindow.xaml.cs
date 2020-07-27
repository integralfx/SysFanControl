using MahApps.Metro.Controls;
using SysFanControl.ViewModels;
using System;
using System.Windows;

namespace SysFanControl
{
    public partial class MainWindow : MetroWindow
    {
        private readonly System.Windows.Forms.NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            var menuStrip = new System.Windows.Forms.ContextMenuStrip();
            var menuItemOpen = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "Open"
            };
            menuItemOpen.Click += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
            };
            menuStrip.Items.Add(menuItemOpen);
            var menuItemClose = new System.Windows.Forms.ToolStripMenuItem
            {
                Text = "Close"
            };
            menuItemClose.Click += (s, e) => Close();
            menuStrip.Items.Add(menuItemClose);

            notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                ContextMenuStrip = menuStrip,
                Icon = System.Drawing.Icon.FromHandle(Properties.Resources.speedfan.Handle),
                Text = "SysFanControl",
                Visible = true
            };

#if DEBUG
            DataContext = new MainWindowViewModel();
#else
            try
            {
                DataContext = new MainWindowViewModel();
            }
            catch (Exception e)
            {
                MessageBox.Show($"{e.Message}\n{e.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
#endif
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Minimized:
                    notifyIcon.Visible = true;
                    Hide();
                    break;

                case WindowState.Normal:
                    notifyIcon.Visible = false;
                    break;
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
