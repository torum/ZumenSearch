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
using System.Windows.Input;

namespace ZumenSearch.ViewModels
{
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
        private RentLivingPdf _rentLivingPdfEdit;
        public RentLivingPdf RentLivingPdfEdit
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
                Picture = _rentLivingPdfEdit.Picture;
                SelectedPdfType = _rentLivingPdfEdit.PdfType;
                PdfDescription = _rentLivingPdfEdit.PdfDescription;
                PdfIsMain = _rentLivingPdfEdit.PdfIsMain;

            }
        }

        // 元の賃貸住居用物件のPDFリストを保持。（Winodow生成時に設定される）
        public ObservableCollection<RentLivingPdf> RentLivingPdfs { get; set; }

        #region == 編集用のプロパティ ==

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

        // PDFの種類
        private string _selectedPdfType;
        public string SelectedPdfType
        {
            get
            {
                return _selectedPdfType;
            }
            set
            {
                if (_selectedPdfType == value)
                    return;

                _selectedPdfType = value;
                NotifyPropertyChanged("SelectedPdfType");

                // 変更フラグを立てる
                IsDirty = true;
            }
        }

        // PDFのメインPDFフラグ
        private bool _pdfIsMain;
        public bool PdfIsMain
        {
            get
            {
                return _pdfIsMain;
            }
            set
            {
                if (_pdfIsMain == value)
                    return;

                _pdfIsMain = value;
                NotifyPropertyChanged("PdfIsMain");

                // 変更フラグを立てる
                IsDirty = true;
            }
        }

        // PDF説明・コメント
        private string _pdfDescription;
        public string PdfDescription
        {
            get
            {
                return _pdfDescription;
            }
            set
            {
                if (_pdfDescription == value)
                    return;

                _pdfDescription = value;
                NotifyPropertyChanged("PdfDescription");

                // 変更フラグを立てる
                IsDirty = true;
            }
        }

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


        #region == イベント ==

        // 親画面（賃貸住居用物件）に、（コードビハインド経由で）変更通知を送るイベント。
        public delegate void IsDirtyEventHandler();
        public event IsDirtyEventHandler RentLivingIsDirty;

        #endregion

        public RentLivingPdfViewModel(string id)
        {
            _id = id;

            #region == コマンド初期化 ==

            // PDFを保存
            PdfSaveCommand = new RelayCommand(PdfSaveCommand_Execute, PdfSaveCommand_CanExecute);

            #endregion

        }


        #region == イベントの実装 ==


        #endregion

        #region == コマンドの実装 ==

        // 画像の保存（追加または更新）
        public ICommand PdfSaveCommand { get; }
        public bool PdfSaveCommand_CanExecute()
        {
            if (RentLivingPdfEdit == null)
                return false;

            if (RentLivingPdfs == null)
                return false;

            if (IsDirty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void PdfSaveCommand_Execute()
        {
            PdfSave();
        }

        // PDFの保存（追加または更新）メソッド（コードビハインドから保存確認ダイアログでも呼ばれる）
        public bool PdfSave()
        {
            if (RentLivingPdfEdit == null)
                return false;

            if (RentLivingPdfs == null)
                return false;

            if (IsDirty == false)
                return true;

            // TODO: 入力チェック

            // 各値の更新
            RentLivingPdfEdit.PdfDescription = PdfDescription;
            RentLivingPdfEdit.PdfType = SelectedPdfType;

            // IsMainフラグだった場合、
            if (PdfIsMain)
            {
                // 一旦画像リスト内の全てのIsMainフラグをクリアする
                foreach (var hoge in RentLivingPdfs)
                {
                    if (hoge.PdfIsMain)
                    {
                        hoge.PdfIsMain = false;

                        // 変更フラグを立ててDBに保存されるように
                        hoge.IsModified = true;
                    }
                }
            }
            RentLivingPdfEdit.PdfIsMain = PdfIsMain;


            // 画像リストから該当オブジェクトを見つける
            var found = RentLivingPdfs.FirstOrDefault(x => x.RentPdfId == RentLivingPdfEdit.RentPdfId);
            if (found == null)
            {
                // 追加
                RentLivingPdfs.Add(RentLivingPdfEdit);
            }
            else
            {
                // 更新
                found = RentLivingPdfEdit;
            }

            // 新規フラグをクリア
            //RentLivingPictureEdit.IsNew = false;

            // 変更フラグをクリア
            IsDirty = false;

            // DB更新用のフラグを立てる
            RentLivingPdfEdit.IsModified = true;
            // 触らない >RentLivingPictureEdit.IsNew

            // 親画面（賃貸住居用物件）に、変更通知を送る。
            RentLivingIsDirty?.Invoke();

            return true;
        }

        #endregion


    }
}
