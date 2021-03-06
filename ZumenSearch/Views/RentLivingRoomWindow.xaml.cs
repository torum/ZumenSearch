﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.ComponentModel;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;

namespace ZumenSearch.Views
{
    /// <summary>
    /// RentLivingRoom.xaml の相互作用ロジック
    /// </summary>
    public partial class RentLivingRoomWindow : Window
    {
        public RentLivingRoomWindow()
        {
            InitializeComponent();

            Loaded += Window_Loaded;

            //RestoreButton.Visibility = Visibility.Collapsed;
            //MaxButton.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            // Load window possition.
            if ((Properties.Settings.Default.RentLivingRoomWindow_Width >= 1024))
            {
                this.Width = Properties.Settings.Default.RentLivingRoomWindow_Width;
            }

            if ((Properties.Settings.Default.RentLivingRoomWindow_Height >= 680))
            {
                this.Height = Properties.Settings.Default.RentLivingRoomWindow_Height;
            }

            if ((Properties.Settings.Default.RentLivingRoomWindow_Left > 0) )
            {
                this.Left = Properties.Settings.Default.RentLivingRoomWindow_Left;
            }
            if (this.Left < 0)
                this.Left = 0;

            if ((Properties.Settings.Default.RentLivingRoomWindow_Top > 0))
            {
                this.Top = Properties.Settings.Default.RentLivingRoomWindow_Top;
            }
            if (this.Top < 0)
                this.Top = 0;

            if (Properties.Settings.Default.RentLivingRoomWindow_Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
            */
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App app = App.Current as App;

            app.RemoveEditWindow(this);
            
            /*
            // Save window pos.
            if (WindowState == WindowState.Normal && Visibility == Visibility.Visible)
            {
                Properties.Settings.Default.RentLivingRoomWindow_Left = this.Left;
                Properties.Settings.Default.RentLivingRoomWindow_Top = this.Top;
                Properties.Settings.Default.RentLivingRoomWindow_Height = this.Height;
                Properties.Settings.Default.RentLivingRoomWindow_Width = this.Width;

                Properties.Settings.Default.RentLivingRoomWindow_Maximized = false;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.RentLivingRoomWindow_Maximized = true;
            }

            Properties.Settings.Default.Save();
            */
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            /*
            if (this.WindowState == WindowState.Normal)
            {
                RestoreButton.Visibility = Visibility.Collapsed;
                MaxButton.Visibility = Visibility.Visible;

                //BackgroundGrid.Margin = new Thickness(0, 0, 0, 0);
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaxButton.Visibility = Visibility.Collapsed;

                //BackgroundGrid.Margin = new Thickness(3, 3, 3, 3);
            }
            */
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

        #region == MAXIMIZE時のタスクバー被りのFix ==

        // https://engy.us/blog/2020/01/01/implementing-a-custom-window-title-bar-in-wpf/

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(HookProc);
        }

        public static IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETMINMAXINFO)
            {
                // We need to tell the system what our size should be when maximized. Otherwise it will cover the whole screen,
                // including the task bar.
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

                // Adjust the maximized size and position to fit the work area of the correct monitor
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO();
                    monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    GetMonitorInfo(monitor, ref monitorInfo);
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;
                    mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top) - 4; // -4を付加した。てっぺんをクリックしても反応がなかったから。
                    mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top) + 4; // 付加した分の補正。
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }

            return IntPtr.Zero;
        }

        private const int WM_GETMINMAXINFO = 0x0024;

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }


        #endregion

    }
}
