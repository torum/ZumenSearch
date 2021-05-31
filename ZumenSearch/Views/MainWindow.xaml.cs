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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
using ZumenSearch.ViewModels;
using ZumenSearch.Views;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Models;

namespace ZumenSearch.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (this.DataContext as MainViewModel).OnWindowLoaded;
            Closing += (this.DataContext as MainViewModel).OnWindowClosing;

            RestoreButton.Visibility = Visibility.Collapsed;
            MaxButton.Visibility = Visibility.Visible;

            //
            if (this.DataContext is MainViewModel vm)
            {
                if (vm != null)
                {
                    App app = App.Current as App;
                    if (app != null)
                    {
                        vm.OpenRentLivingBuildingWindow += (sender, arg) => { this.OpenRentLivingBuildingWindow(arg); };

                    }
                }
            }
        }

        // 二重起動防止処理からの復帰
        public void BringToForeground()
        {
            if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }

            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();
        }

        // Windowロード時の処理
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if ((Properties.Settings.Default.MainWindow_Width != 0))
            {
                this.Width = Properties.Settings.Default.MainWindow_Width;
            }

            if ((Properties.Settings.Default.MainWindow_Height != 0))
            {
                this.Height = Properties.Settings.Default.MainWindow_Height;
            }

            if ((Properties.Settings.Default.MainWindow_Left >= 0))
            {
                this.Left = Properties.Settings.Default.MainWindow_Left;
            }
            else
            {
                this.Left = 0;
            }

            if ((Properties.Settings.Default.MainWindow_Top >= 0))
            {
                this.Top = Properties.Settings.Default.MainWindow_Top;
            }
            else
            {
                this.Top = 0;
            }

            if (Properties.Settings.Default.MainWindow_Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        // Windowクローズ時の処理
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.MainWindow_Left = this.Left;
                Properties.Settings.Default.MainWindow_Top = this.Top;
                Properties.Settings.Default.MainWindow_Height = this.Height;
                Properties.Settings.Default.MainWindow_Width = this.Width;

                Properties.Settings.Default.MainWindow_Maximized = false;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.MainWindow_Maximized = true;
            }

            Properties.Settings.Default.Save();
        }

        // 建物 Windowの表示
        public void OpenRentLivingBuildingWindow(OpenRentLivingBuildingWindowEventArgs arg)
        {
            if (arg == null)
                return;

            if (String.IsNullOrEmpty(arg.Id))
                return;

            if (arg.RentLivingObject == null)
                return;

            string id = arg.Id;

            App app = App.Current as App;

            foreach (var w in app.WindowList)
            {
                if (!(w is RentLivingBuildingWindow))
                    continue;

                if ((w as RentLivingBuildingWindow).DataContext == null)
                    continue;

                if (!((w as RentLivingBuildingWindow).DataContext is RentLivingBuildingViewModel))
                    continue;

                if (id == ((w as RentLivingBuildingWindow).DataContext as RentLivingBuildingViewModel).Id)
                {
                    //w.Activate();

                    if ((w as RentLivingBuildingWindow).WindowState == WindowState.Minimized || (w as Window).Visibility == Visibility.Hidden)
                    {
                        //w.Show();
                        (w as RentLivingBuildingWindow).Visibility = Visibility.Visible;
                        (w as RentLivingBuildingWindow).WindowState = WindowState.Normal;
                    }

                    (w as RentLivingBuildingWindow).Activate();
                    //(w as EditorWindow).Topmost = true;
                    //(w as EditorWindow).Topmost = false;
                    (w as RentLivingBuildingWindow).Focus();

                    return;
                }
            }

            // Create a new window.
            if (arg.RentLivingObject is RentLiving)
            {
                // Windowを生成
                var win = new RentLivingBuildingWindow();

                // ViewModelを設定
                win.DataContext = new RentLivingBuildingViewModel(id);

                // 設定したViewModelを変数に代入
                var vm = win.DataContext as RentLivingBuildingViewModel;

                // 編集用の賃貸住居用物件を設定
                vm.RentLivingEdit = (arg.RentLivingObject as RentLiving);

                // データアクセスのモジュールを編集画面に渡す。
                vm.DataAccessModule = arg.DataAccessModule;

                // 部屋編集用のWindowを表示させるイベントをサブスクライブする設定
                vm.OpenRentLivingSectionWindow += (sender, arg) => { win.OpenRentLivingSectionWindow(arg); };
                // 画像編集用のWindowを表示させるイベントをサブスクライブする設定
                vm.OpenRentLivingImagesWindow += (sender, arg) => { win.OpenRentLivingImagesWindow(arg); };

                // 編集画面終了時の保存確認後の閉じるアクションを設定
                //vm.CloseAction = new Action(win.Close);

                // Windowリストに追加。
                app.WindowList.Add(win);

                // Windowの親設定
                win.Owner = this;

                // モーダルで編集画面を表示する。
                win.ShowDialog();//win.Show();

                //win.ShowInTaskbar = true;
                //win.ShowActivated = true;
                //win.Visibility = Visibility.Visible;
                //win.Activate();
            }
        }

        #region == デフォのウィンドウ関連 ==

        private void Window_StateChanged(object sender, EventArgs e)
        {
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

        #endregion

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

        private void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender != null)
            {
                if (sender is TextBox)
                {
                    if ((sender as TextBox).Visibility == Visibility.Visible)
                    {
                        (sender as TextBox).Focus();
                        Keyboard.Focus((sender as TextBox));
                    }
                }
            }
        }

    }
}
