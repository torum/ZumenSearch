using System;
using System.Diagnostics;
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
using System.Windows.Interop;
using System.Runtime.InteropServices;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;
using ZumenSearch.Models;
using ZumenSearch.ViewModels.Classes;

namespace ZumenSearch.Views
{
    /// <summary>
    /// RentLivingImagesWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class RentLivingImageWindow : Window
    {
        public RentLivingImageWindow()
        {
            InitializeComponent();

            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load window possition.
            if ((Properties.Settings.Default.RentLivingImagesWindow_Width >= 0) && (Properties.Settings.Default.RentLivingImagesWindow_Height >= 0))
            {
                this.Width = Properties.Settings.Default.RentLivingImagesWindow_Width;
                this.Height = Properties.Settings.Default.RentLivingImagesWindow_Height;
            }

            if ((Properties.Settings.Default.RentLivingImagesWindow_Left >= 0))
            {
                this.Left = Properties.Settings.Default.RentLivingImagesWindow_Left;
            }
            else
            {
                this.Left = 0;
            }

            if ( (Properties.Settings.Default.RentLivingImagesWindow_Top >= 0))
            {
                this.Top = Properties.Settings.Default.RentLivingImagesWindow_Top;
            }
            else
            {
                this.Top = 0;
            }

            if (Properties.Settings.Default.RentLivingImagesWindow_Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 変更フラグが立っている時に、保存するか確認し、キャンセル可能にする。
            if (this.DataContext != null)
            {
                if (this.DataContext is RentLivingImageViewModel)
                {
                    if ((this.DataContext as RentLivingImageViewModel).IsDirty)
                    {

                        try
                        {
                            // 変更の保存確認ダイアログを表示。
                            var result = MessageBox.Show("変更を保存して閉じますか？", "未保存の変更があります", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                // 「はい」ボタンを押した場合の処理
                                var vm = (this.DataContext as RentLivingImageViewModel);
                                if (vm.PictureSave() == false)
                                {
                                    e.Cancel = true;

                                    return;
                                }
                            }
                            else if (result == MessageBoxResult.Cancel)
                            {
                                // 「キャンセル」ボタンを押した場合の処理
                                e.Cancel = true;

                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null)
                            {
                                Debug.WriteLine("InnerException:'" + ex.InnerException.Message + " @RentLivingImagesWindow:Window_Closing");
                            }
                            else
                            {
                                Debug.WriteLine("Exception:'" + ex.Message + "' @RentLivingImagesWindow:Window_Closing");
                            }
                        }

                    }
                }
            }

            // Save window pos.
            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.RentLivingImagesWindow_Left = this.Left;
                Properties.Settings.Default.RentLivingImagesWindow_Top = this.Top;
                Properties.Settings.Default.RentLivingImagesWindow_Height = this.Height;
                Properties.Settings.Default.RentLivingImagesWindow_Width = this.Width;

                Properties.Settings.Default.RentLivingImagesWindow_Maximized = false;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.RentLivingImagesWindow_Maximized = true;
            }

            Properties.Settings.Default.Save();

            // Windowリストから自らを削除。
            App app = App.Current as App;
            app.RemoveEditWindow(this);

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
