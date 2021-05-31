using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.ObjectModel;
using ZumenSearch.Models;
using ZumenSearch.Views;
using ZumenSearch.Common;
using System.IO;

namespace ZumenSearch.ViewModels
{
    // TODO: RentLivingZumenPDF> RentLivingPDF


    /// <summary>
    /// 賃貸住居用物件のPDF編集用ViewModel
    /// </summary>
    public class RentLivingPdfViewModel : ViewModelBase
    {
        // 賃貸住居用物件のPDF ID（Window識別用）（Winodow生成時に設定される）
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        // 元の賃貸住居用物件のPDFオブジェクトを保持。（Winodow生成時に設定される）
        private RentLivingZumenPDF _rentLivingPdfEdit;
        public RentLivingZumenPDF RentLivingPdfEdit
        {
            get
            {
                return _rentLivingPdfEdit;
            }
            set
            {
                if (_rentLivingPdfEdit == value)
                    return;

                _rentLivingPdfEdit = value;
                NotifyPropertyChanged("RentLivingPdfEdit");

                // 値の設定時に、編集画面用のプロパティにそれぞれの値をポピュレイトする
                //Picture = _rentLivingPictureEdit.Picture;
                //SelectedPictureType = _rentLivingPictureEdit.PictureType;
                //PictureDescription = _rentLivingPictureEdit.PictureDescription;
                //PictureIsMain = _rentLivingPictureEdit.PictureIsMain;

                // 変更フラグをクリアする（ユーザーの入力で変更・編集された訳ではないので）
                IsDirty = false;
            }
        }

        // 元の賃貸住居用物件のPDFリストを保持。（Winodow生成時に設定される）
        public ObservableCollection<RentLivingZumenPDF> RentLivingZumenPDFs { get; set; }

        // PDFの画像プレビュー
        private ImageSource _picture;
        public ImageSource Picture
        {
            get
            {
                return _picture;
            }
            set
            {
                if (_picture == value) return;

                _picture = value;
                this.NotifyPropertyChanged("Picture");

                // 変更フラグを立てる
                //IsDirty = true;
            }
        }

        #region == 編集用のプロパティ ==
        // 変更（データ入力）があったかどうかのフラグ。（画面終了時の確認）
        private bool _isDirty = false;
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                if (_isDirty == value) return;

                _isDirty = value;
                NotifyPropertyChanged("IsDirty");
                NotifyPropertyChanged("StatusIsDirty");
            }
        }

        // 変更フラグの表示用テキスト
        public string StatusIsDirty
        {
            get
            {
                if (IsDirty)
                    return "変更";
                else
                    return "";
            }
        }
        #endregion


        private Windows.Data.Pdf.PdfDocument pdfDocument;

        public RentLivingPdfViewModel(string id)
        {
            _id = id;

            #region == コマンド初期化 ==

            // 画像を保存
            //PictureSaveCommand = new RelayCommand(PictureSaveCommand_Execute, PictureSaveCommand_CanExecute);

            #endregion

        }

        private async void btnPdf_Click()
        {

            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(@"C:\Users\xxxx\Downloads\zumen.pdf");

            try
            {
                // PDFファイルを読み込む
                pdfDocument = await Windows.Data.Pdf.PdfDocument.LoadFromFileAsync(file);
            }
            catch
            {

            }

            if (pdfDocument != null)
            {
                // 1ページ目を読み込む
                using (Windows.Data.Pdf.PdfPage page = pdfDocument.GetPage(0))
                {
                    BitmapImage image = new BitmapImage();

                    using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                    {
                        await page.RenderToStreamAsync(stream);

                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream.AsStream();
                        image.EndInit();
                    }

                    Picture = image;
                }
            }


        }
    }
}
