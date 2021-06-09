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

namespace ZumenSearch.Views
{
    /// <summary>
    /// RentLivingPdfWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class RentLivingPdfWindow : Window
    {
        public RentLivingPdfWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 変更フラグが立っている時に、保存するか確認し、キャンセル可能にする。
            if (this.DataContext != null)
            {
                if (this.DataContext is RentLivingPdfViewModel)
                {
                    if ((this.DataContext as RentLivingPdfViewModel).IsDirty)
                    {

                        try
                        {
                            // 変更の保存確認ダイアログを表示。
                            var result = MessageBox.Show("変更を保存して閉じますか？", "未保存の変更があります", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                            {
                                // 「はい」ボタンを押した場合の処理
                                var vm = (this.DataContext as RentLivingPdfViewModel);
                                if (vm.PdfSave() == false)
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
                                Debug.WriteLine("InnerException:'" + ex.InnerException.Message + " @RentLivingPdfWindow:Window_Closing");
                            }
                            else
                            {
                                Debug.WriteLine("Exception:'" + ex.Message + "' @RentLivingPdfWindow:Window_Closing");
                            }
                        }

                    }
                }
            }

            // Save window pos.
            /*
            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.RentLivingImageWindow_Left = this.Left;
                Properties.Settings.Default.RentLivingImageWindow_Top = this.Top;
                Properties.Settings.Default.RentLivingImageWindow_Height = this.Height;
                Properties.Settings.Default.RentLivingImageWindow_Width = this.Width;

                Properties.Settings.Default.RentLivingImageWindow_Maximized = false;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.RentLivingImageWindow_Maximized = true;
            }

            Properties.Settings.Default.Save();
            */

            // Windowリストから自らを削除。
            App app = App.Current as App;
            app.RemoveEditWindow(this);

        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
