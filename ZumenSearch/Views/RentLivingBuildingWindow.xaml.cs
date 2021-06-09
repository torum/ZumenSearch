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
    /// RentLivingBuildingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class RentLivingBuildingWindow : Window
    {
        public RentLivingBuildingWindow()
        {
            InitializeComponent();

            Loaded += Window_Loaded;

            RestoreButton.Visibility = Visibility.Collapsed;
            //MaxButton.Visibility = Visibility.Visible;
            MaxButton.Visibility = Visibility.Collapsed;
        }

        // 部屋編集画面の表示
        public void OpenRentLivingRoomWindow(OpenRentLivingRoomWindowEventArgs arg)
        {
            if (arg == null)
                return;

            if (String.IsNullOrEmpty(arg.Id))
                return;

            if (arg.RentLivingRoomObject == null)
                return;

            string id = arg.Id;

            App app = App.Current as App;

            foreach (var w in app.WindowList)
            {
                if (!(w is RentLivingRoomWindow))
                    continue;

                if ((w as RentLivingRoomWindow).DataContext == null)
                    continue;

                if (!((w as RentLivingRoomWindow).DataContext is RentLivingRoomViewModel))
                    continue;

                if (id == ((w as RentLivingRoomWindow).DataContext as RentLivingRoomViewModel).Id)
                {
                    //w.Activate();

                    if ((w as RentLivingRoomWindow).WindowState == WindowState.Minimized || (w as Window).Visibility == Visibility.Hidden)
                    {
                        //w.Show();
                        (w as RentLivingRoomWindow).Visibility = Visibility.Visible;
                        (w as RentLivingRoomWindow).WindowState = WindowState.Normal;
                    }

                    (w as RentLivingRoomWindow).Activate();
                    //(w as EditorWindow).Topmost = true;
                    //(w as EditorWindow).Topmost = false;
                    (w as RentLivingRoomWindow).Focus();

                    return;
                }
            }

            // Create new window.

            if (arg.RentLivingRoomObject is RentLivingRoom)
            {
                // Windowを生成
                var win = new RentLivingRoomWindow();
                // VMをセット
                win.DataContext = new RentLivingRoomViewModel(id);
                
                var vm = (win.DataContext as RentLivingRoomViewModel);
                // VMにデータを渡す
                vm.RentLivingRoomEdit = arg.RentLivingRoomObject;
                vm.RentLivingRooms = arg.RentLivingRooms;

                // 画像編集画面からの変更通知を受け取る
                vm.RentLivingIsDirty += () => OnRentLivingIsDirty();

                // Windowリストへ追加。
                app.WindowList.Add(win);

                // モーダルで編集画面を開く
                win.Owner = this;
                //win.Show();
                win.ShowDialog();

                //win.ShowInTaskbar = true;
                //win.ShowActivated = true;
                //win.Visibility = Visibility.Visible;
                //win.Activate();
            }
        }

        // 画像編集画面の表示
        public void OpenRentLivingImageWindow(OpenRentLivingImageWindowEventArgs arg)
        {
            if (arg == null)
                return;

            if (String.IsNullOrEmpty(arg.Id))
                return;

            if (arg.RentLivingPictures == null)
                return;

            string id = arg.Id;

            App app = App.Current as App;

            foreach (var w in app.WindowList)
            {
                if (!(w is RentLivingImageWindow))
                    continue;

                if ((w as RentLivingImageWindow).DataContext == null)
                    continue;

                if (!((w as RentLivingImageWindow).DataContext is RentLivingImageViewModel))
                    continue;

                if (id == ((w as RentLivingImageWindow).DataContext as RentLivingImageViewModel).Id)
                {
                    //w.Activate();

                    if ((w as RentLivingImageWindow).WindowState == WindowState.Minimized || (w as Window).Visibility == Visibility.Hidden)
                    {
                        //w.Show();
                        (w as RentLivingImageWindow).Visibility = Visibility.Visible;
                        (w as RentLivingImageWindow).WindowState = WindowState.Normal;
                    }

                    (w as RentLivingImageWindow).Activate();
                    //(w as EditorWindow).Topmost = true;
                    //(w as EditorWindow).Topmost = false;
                    (w as RentLivingImageWindow).Focus();

                    return;
                }
            }

            // Create a new window.
            var win = new RentLivingImageWindow();
            // VMをセット
            win.DataContext = new RentLivingImageViewModel(id);
            
            var vm = (win.DataContext as RentLivingImageViewModel);
            // VMにデータを渡す
            vm.RentLivingPictureEdit = arg.RentLivingPictureObject;
            vm.RentLivingPictures = arg.RentLivingPictures;
            vm.IsDirty = !arg.IsEdit;

            // 画像編集画面からの変更通知を受け取る
            vm.RentLivingIsDirty += () => OnRentLivingIsDirty();

            // Windowリストへ追加。
            app.WindowList.Add(win);

            // モーダルで編集画面を開く
            win.Owner = this;
            win.ShowDialog();

        }

        // PDF編集画面の表示
        public void OpenRentLivingPdfWindow(OpenRentLivingPdfWindowEventArgs arg)
        {
            if (arg == null)
                return;

            if (String.IsNullOrEmpty(arg.Id))
                return;

            if (arg.RentLivingPdfs == null)
                return;

            string id = arg.Id;

            App app = App.Current as App;

            foreach (var w in app.WindowList)
            {
                if (!(w is RentLivingImageWindow))
                    continue;

                if ((w as RentLivingImageWindow).DataContext == null)
                    continue;

                if (!((w as RentLivingImageWindow).DataContext is RentLivingImageViewModel))
                    continue;

                if (id == ((w as RentLivingImageWindow).DataContext as RentLivingImageViewModel).Id)
                {
                    //w.Activate();

                    if ((w as RentLivingImageWindow).WindowState == WindowState.Minimized || (w as Window).Visibility == Visibility.Hidden)
                    {
                        //w.Show();
                        (w as RentLivingImageWindow).Visibility = Visibility.Visible;
                        (w as RentLivingImageWindow).WindowState = WindowState.Normal;
                    }

                    (w as RentLivingImageWindow).Activate();
                    //(w as EditorWindow).Topmost = true;
                    //(w as EditorWindow).Topmost = false;
                    (w as RentLivingImageWindow).Focus();

                    return;
                }
            }

            // Create a new window.
            var win = new RentLivingPdfWindow();
            // VMをセット
            win.DataContext = new RentLivingPdfViewModel(id);

            var vm = (win.DataContext as RentLivingPdfViewModel);
            // VMにデータを渡す
            vm.RentLivingPdfEdit = arg.RentLivingPdfObject;
            vm.RentLivingPdfs = arg.RentLivingPdfs;
            vm.IsDirty = !arg.IsEdit;

            // 画像編集画面からの変更通知を受け取る
            vm.RentLivingIsDirty += () => OnRentLivingIsDirty();

            // Windowリストへ追加。
            app.WindowList.Add(win);

            // モーダルで編集画面を開く
            win.Owner = this;
            win.ShowDialog();

        }

        public void OnRentLivingIsDirty()
        {
            if (this.DataContext == null)
                return;

            if (this.DataContext is not RentLivingBuildingViewModel)
                return;

            (this.DataContext as RentLivingBuildingViewModel).SetIsDirty = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load window possition.
            if ((Properties.Settings.Default.RentLivingBuildingWindow_Width >= 1024))
            {
                this.Width = Properties.Settings.Default.RentLivingBuildingWindow_Width;
            }

            if ( (Properties.Settings.Default.RentLivingBuildingWindow_Height >= 680))
            {
                this.Height = Properties.Settings.Default.RentLivingBuildingWindow_Height;
            }

            if ((Properties.Settings.Default.RentLivingBuildingWindow_Left > 0) )
            {
                this.Left = Properties.Settings.Default.RentLivingBuildingWindow_Left;
            }
            if (this.Left < 0)
                this.Left = 0;

            if ((Properties.Settings.Default.RentLivingBuildingWindow_Top > 0))
            {
                this.Top = Properties.Settings.Default.RentLivingBuildingWindow_Top;
            }
            if (this.Top < 0)
                this.Top = 0;

            if (Properties.Settings.Default.RentLivingBuildingWindow_Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 変更フラグが立っている時に、保存するか確認し、キャンセル可能にする。
            if (this.DataContext != null)
            {
                if (this.DataContext is RentLivingBuildingViewModel)
                {
                    if ((this.DataContext as RentLivingBuildingViewModel).RentLivingEdit.IsDirty)
                    {
                        try
                        {
                            // 変更の保存確認ダイアログを表示。
                            var result = MessageBox.Show("変更を保存して閉じますか？", "未保存の変更があります", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                // 「はい」ボタンを押した場合の処理
                                var vm = (this.DataContext as RentLivingBuildingViewModel);
                                if (vm.Save() == false)
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
                                Debug.WriteLine("InnerException:'" + ex.InnerException.Message + " @RentLivingWindow:Window_Closing");
                            }
                            else
                            {
                                Debug.WriteLine("Exception:'" + ex.Message + "' @RentLivingWindow:Window_Closing");
                            }
                        }
                        
                    }
                }
            }

            // Windowの位置を保存
            if (this.WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.RentLivingBuildingWindow_Left = this.Left;
                Properties.Settings.Default.RentLivingBuildingWindow_Top = this.Top;
                Properties.Settings.Default.RentLivingBuildingWindow_Height = this.Height;
                Properties.Settings.Default.RentLivingBuildingWindow_Width = this.Width;

                Properties.Settings.Default.RentLivingBuildingWindow_Maximized = false;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.RentLivingBuildingWindow_Maximized = true;
            }

            Properties.Settings.Default.Save();

            // Windowの一覧から自らを削除
            App app = App.Current as App;
            app.RemoveEditWindow(this);

        }

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
                    mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top) + 4; // + 4付加した分の補正。
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
