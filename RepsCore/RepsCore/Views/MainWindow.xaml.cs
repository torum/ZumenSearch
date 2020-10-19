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

        /// <summary>
        /// OpenFileDialog. Microsoft.Win32;名前空間のOpenFileDialogを使う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOpenPictureFileAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "画像ファイル (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"; // 外観なので、JPGかPNGのみ。間取りならGIF。
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); // or MyDocuments

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
                    // まぁ裏技というかむりやり。
                    (this.DataContext as MainViewModel).RentLivingNew.Picture = bitmap;

                    // まぁ裏技というかむりやり。
                    (this.DataContext as MainViewModel).RentLivingNew.PictureFilePath = openFileDialog.FileName;
                }
                else
                {
                    // TODO 
                    (this.DataContext as MainViewModel).RentLivingNew.PictureFilePath = "";
                }

            }
        }

        private void ButtonOpenPictureFileEdit_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "画像ファイル (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"; // 外観なので、JPGかPNGのみ。間取りならGIF。
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); // or MyDocuments

            if (openFileDialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName.Trim()))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.DecodePixelWidth = 200; // Note: To save significant application memory, set the DecodePixelWidth or DecodePixelHeight of the BitmapImage value of the image source to the desired height and width of the rendered image. In order to preserve aspect ratio, only set either DecodePixelWidth or DecodePixelHeight but not both.
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.EndInit();

                    //ImagePreviewPictureEdit.Source = bitmap;
                    // まぁ裏技というかむりやり。
                    (this.DataContext as MainViewModel).RentLivingEdit.Picture = bitmap;

                    // ファイルのパスを追加してビューモデルに通知
                    //ButtonOpenPictureFile.Tag = openFileDialog.FileName;
                    // まぁ裏技というかむりやり。
                    (this.DataContext as MainViewModel).RentLivingEdit.PictureFilePath = openFileDialog.FileName;
                }
                else
                {
                    // TODO 
                    (this.DataContext as MainViewModel).RentLivingEdit.PictureFilePath = "";
                }

            }
        }

    }
}
