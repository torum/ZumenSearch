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
using ZumenSearch.ViewModels;
using ZumenSearch.Views;
using ZumenSearch.Models;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Models.Classes;

namespace ZumenSearch.Views
{
    /// <summary>
    /// RentBuilding.xaml の相互作用ロジック
    /// </summary>
    public partial class RentLivingWindow : Window
    {
        public RentLivingWindow()
        {
            InitializeComponent();

            Loaded += Window_Loaded;

            RestoreButton.Visibility = Visibility.Collapsed;
            MaxButton.Visibility = Visibility.Visible;
        }

        // 編集画面の表示
        public void OpenRentLivingSectionWindow(OpenRentLivingSectionWindowEventArgs arg)
        {
            if (arg == null)
                return;

            if (String.IsNullOrEmpty(arg.Id))
                return;

            if (arg.EditObject == null)
                return;

            string id = arg.Id;

            App app = App.Current as App;

            foreach (var w in app.WindowList)
            {
                if (!(w is RentLivingSectionWindow))
                    continue;

                if ((w as RentLivingSectionWindow).DataContext == null)
                    continue;

                if (!((w as RentLivingSectionWindow).DataContext is RentLivingSectionViewModel))
                    continue;

                if (id == ((w as RentLivingSectionWindow).DataContext as RentLivingSectionViewModel).Id)
                {
                    //w.Activate();

                    if ((w as RentLivingSectionWindow).WindowState == WindowState.Minimized || (w as Window).Visibility == Visibility.Hidden)
                    {
                        //w.Show();
                        (w as RentLivingSectionWindow).Visibility = Visibility.Visible;
                        (w as RentLivingSectionWindow).WindowState = WindowState.Normal;
                    }

                    (w as RentLivingSectionWindow).Activate();
                    //(w as EditorWindow).Topmost = true;
                    //(w as EditorWindow).Topmost = false;
                    (w as RentLivingSectionWindow).Focus();

                    return;
                }
            }

            // Create new window.

            if (arg.EditObject is RentLivingSection)
            {
                var win = new RentLivingSectionWindow();

                win.DataContext = new RentLivingSectionViewModel(id);
                (win.DataContext as RentLivingSectionViewModel).RentLivingRoomEdit = (arg.EditObject as RentLivingSection);

                app.WindowList.Add(win);

                win.Owner = this;
                //win.Show();
                win.ShowDialog();

                //win.ShowInTaskbar = true;
                //win.ShowActivated = true;
                //win.Visibility = Visibility.Visible;
                //win.Activate();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load window possition.
            if ((Properties.Settings.Default.RentLivingWindow_Width >= 0) && (Properties.Settings.Default.RentLivingWindow_Height >= 0) )
            {
                this.Width = Properties.Settings.Default.RentLivingWindow_Width;
                this.Height = Properties.Settings.Default.RentLivingWindow_Height;
            }

            if ((Properties.Settings.Default.RentLivingWindow_Left >= 0) && (Properties.Settings.Default.RentLivingWindow_Top >= 0))
            {
                this.Left = Properties.Settings.Default.RentLivingWindow_Left;
                this.Top = Properties.Settings.Default.RentLivingWindow_Top;
            }
            else
            {
                this.Left = 0;
                this.Top = 0;
            }

            if (Properties.Settings.Default.RentLivingWindow_Maximized)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 変更フラグが立っている時に、保存するか確認し、キャンセル可能にする。
            if (this.DataContext != null)
            {
                if (this.DataContext is RentLivingViewModel)
                {
                    if ((this.DataContext as RentLivingViewModel).RentLivingEdit.IsDirty)
                    {
                        
                        try
                        {
                            // 変更の保存確認ダイアログを表示。
                            var result = MessageBox.Show("変更を保存して閉じますか？", "未保存の変更があります", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                // 「はい」ボタンを押した場合の処理
                                var vm = (this.DataContext as RentLivingViewModel);
                                if (vm.RentLivingEditSave() == false)
                                {
                                    // 保存時にエラー

                                    // TODO: エラーを表示。

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
                Properties.Settings.Default.RentLivingWindow_Left = this.Left;
                Properties.Settings.Default.RentLivingWindow_Top = this.Top;
                Properties.Settings.Default.RentLivingWindow_Height = this.Height;
                Properties.Settings.Default.RentLivingWindow_Width = this.Width;

                Properties.Settings.Default.RentLivingWindow_Maximized = false;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.RentLivingWindow_Maximized = true;
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

                BackgroundGrid.Margin = new Thickness(0, 0, 0, 0);
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaxButton.Visibility = Visibility.Collapsed;

                BackgroundGrid.Margin = new Thickness(3, 3, 3, 3);
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
