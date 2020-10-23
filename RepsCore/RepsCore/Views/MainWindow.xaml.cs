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
using Microsoft.Win32;
using RepsCore.ViewModels;
using RepsCore.Views;

namespace RepsCore.Views
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
            //this.Topmost = true;
            //this.Topmost = false;
            this.Focus();
        }

        /*
        /// <summary>
        /// OpenFileDialog. Microsoft.Win32;名前空間のOpenFileDialogを使う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOpenPictureFileAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true; 
            openFileDialog.Filter = "イメージファイル (*.jpg;*.png;*.gif;*.jpeg)|*.png;*.jpg;*.gif;*.jpeg|写真ファイル (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg|画像ファイル(*.gif;*.png)|*.gif;*.png"; // 外観ならJPGかPNGのみ。間取りならGIFかPNG。
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); // or MyDocuments
            //this.openFileDialog.Title = "My Image Browser";

            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName.Trim()))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.DecodePixelWidth = 200; // Note: To save significant application memory, set the DecodePixelWidth or DecodePixelHeight of the BitmapImage value of the image source to the desired height and width of the rendered image. In order to preserve aspect ratio, only set either DecodePixelWidth or DecodePixelHeight but not both.
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();

                    //ImagePreviewPictureAdd.Source = bitmap;
                }
            }
        }
        */

    }
}
