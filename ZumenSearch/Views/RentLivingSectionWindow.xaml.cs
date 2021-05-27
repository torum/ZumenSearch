using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;

namespace ZumenSearch.Views
{
    /// <summary>
    /// RentLivingRoom.xaml の相互作用ロジック
    /// </summary>
    public partial class RentLivingSectionWindow : Window
    {
        public RentLivingSectionWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;

            RestoreButton.Visibility = Visibility.Collapsed;
            MaxButton.Visibility = Visibility.Visible;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load window possition.
            if ((Properties.Settings.Default.RentLivingSectionWindow_Left != 0)
                && (Properties.Settings.Default.RentLivingSectionWindow_Top != 0)
                && (Properties.Settings.Default.RentLivingSectionWindow_Width != 0)
                && (Properties.Settings.Default.RentLivingSectionWindow_Height != 0)
                )
            {
                this.Left = Properties.Settings.Default.RentLivingSectionWindow_Left;
                this.Top = Properties.Settings.Default.RentLivingSectionWindow_Top;
                this.Width = Properties.Settings.Default.RentLivingSectionWindow_Width;
                this.Height = Properties.Settings.Default.RentLivingSectionWindow_Height;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App app = App.Current as App;

            app.RemoveEditWindow(this);

            // Save window pos.
            if (WindowState == WindowState.Normal && Visibility == Visibility.Visible)
            {
                Properties.Settings.Default.RentLivingSectionWindow_Left = this.Left;
                Properties.Settings.Default.RentLivingSectionWindow_Top = this.Top;
                Properties.Settings.Default.RentLivingSectionWindow_Height = this.Height;
                Properties.Settings.Default.RentLivingSectionWindow_Width = this.Width;

                Properties.Settings.Default.Save();
            }

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                RestoreButton.Visibility = Visibility.Collapsed;
                MaxButton.Visibility = Visibility.Visible;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaxButton.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
