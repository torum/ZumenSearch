using System;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using ZumenSearch.Models;
using ZumenSearch.Views;
using ZumenSearch.Common;
using ZumenSearch.ViewModels.Classes;

namespace ZumenSearch.ViewModels
{
    /// <summary>
    /// 賃貸住居用 部屋 のViewModel
    /// RentLivingRoomEditは直接弄らないで、Window生成時に値を展開し、保存時に各値をセットする。
    /// </summary>
    public class RentLivingRoomViewModel : ViewModelBase
    {
        // 賃貸住居用物件の部屋ID（Window識別用）（Winodow生成時に設定される）
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        // 元の賃貸住居用物件の部屋オブジェクトを保持。（Winodow生成時に設定される）
        private RentLivingRoom _rentLivingRoomEdit;
        public RentLivingRoom RentLivingRoomEdit 
        {
            get
            {
                return _rentLivingRoomEdit;
            }
            set
            {
                if (_rentLivingRoomEdit == value)
                    return;

                _rentLivingRoomEdit = value;
                NotifyPropertyChanged(nameof(RentLivingRoomEdit));

                // 編集画面用のプロパティにそれぞれの値をポピュレイトする

                // Tmp画像の編集用リストを設定。(保存時に差し替える)
                RentLivingRoomPicturesTmp = new ObservableCollection<RentLivingRoomPicture>(RentLivingRoomEdit.RentLivingRoomPictures);

                // Tmp図面の編集用リストを設定。(保存時に差し替える)
                RentLivingRoomPdfsTmp = new ObservableCollection<RentLivingRoomPdf>(RentLivingRoomEdit.RentLivingRoomPdfs);

                RoomNumber = _rentLivingRoomEdit.RentLivingRoomRoomNumber;

                // TODO: ..


                // 編集画面用の変更フラグをリセット。
                IsDirty = false;

            }
        }

        // 元の賃貸住居用物件の部屋リストを保持。（Winodow生成時に設定される）
        public ObservableCollection<RentLivingRoom> RentLivingRooms { get; set; }

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
                NotifyPropertyChanged(nameof(IsDirty));
                NotifyPropertyChanged(nameof(StatusIsDirty));
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

        // 変更通知受け取り。（画像リストなど、変更があった場合にイベント経由でコードビハインドから呼ばれる
        public bool SetIsDirty
        {
            set
            {
                if (value)
                {
                    IsDirty = value;
                }
            }
        }

        #region == 編集用のプロパティ ==

        // 部屋番号
        private string _roomNumber;
        public string RoomNumber
        {
            get
            {
                return _roomNumber;
            }
            set
            {
                if (_roomNumber == value)
                    return;

                _roomNumber = value;
                NotifyPropertyChanged(nameof(RoomNumber));

                // 変更フラグを立てる
                IsDirty = true;
            }
        }

        #endregion

        #region == 画像 ==

        // Tmp画像の編集用リスト。(保存時に差し替える)
        public ObservableCollection<RentLivingRoomPicture> RentLivingRoomPicturesTmp { get; set; }

        // 画像リストの選択項目を保持
        private RentLivingRoomPicture _picturesSelectedItem;
        public RentLivingRoomPicture PicturesSelectedItem
        {
            get
            {
                return _picturesSelectedItem;
            }
            set
            {
                if (_picturesSelectedItem == value)
                    return;

                _picturesSelectedItem = value;
                this.NotifyPropertyChanged(nameof(PicturesSelectedItem));
            }
        }

        #endregion

        #region == PDF ==

        // TmpPDFの編集用リスト。(保存時に差し替える)
        public ObservableCollection<RentLivingRoomPdf> RentLivingRoomPdfsTmp { get; set; }

        // PDFリストの選択項目を保持
        private RentLivingRoomPdf _pdfsSelectedItem;
        public RentLivingRoomPdf PdfsSelectedItem
        {
            get
            {
                return _pdfsSelectedItem;
            }
            set
            {
                if (_pdfsSelectedItem == value)
                    return;

                _pdfsSelectedItem = value;
                this.NotifyPropertyChanged(nameof(PdfsSelectedItem));
            }
        }

        #endregion


        #region == エラー通知関連のプロパティ ==

        // エラー通知表示フラグ
        private bool _showErrorDialog = false;
        public bool ShowErrorDialog
        {
            get
            {
                return _showErrorDialog;
            }
            set
            {
                if (_showErrorDialog == value) return;

                _showErrorDialog = value;
                this.NotifyPropertyChanged(nameof(ShowErrorDialog));
            }
        }

        // エラーテキスト
        private StringBuilder _errorText = new StringBuilder();
        public string ErrorText
        {
            get
            {
                return _errorText.ToString();
            }
            set
            {
                _errorText.Insert(0, value + Environment.NewLine);
                this.NotifyPropertyChanged(nameof(ErrorText));
            }
        }


        #endregion

        #region == ダイアログ（サービス） ==

        // サービスのインジェクションは・・・とりあえずしない。
        //private IOpenDialogService openDialogService;
        private OpenDialogService _openDialogService = new OpenDialogService();

        #endregion

        #region == イベント ==

        //public delegate void MyErrorEvent(ErrorObject err);
        //public event MyErrorEvent ErrorOccured;

        // 親画面（賃貸住居用物件）に、（コードビハインド経由で）変更通知を送るイベント。
        public delegate void IsDirtyEventHandler();
        public event IsDirtyEventHandler RentLivingIsDirty;

        // 画像編集画面を開く
        public event EventHandler<OpenRentLivingRoomImageWindowEventArgs> OpenRentLivingRoomImageWindow;
        // 図面編集画面を開く
        public event EventHandler<OpenRentLivingRoomPdfWindowEventArgs> OpenRentLivingRoomPdfWindow;

        #endregion

        public RentLivingRoomViewModel(string id)
        {
            _id = id;

            #region == コマンド初期化 ==

            // 部屋情報の保存・更新
            SectionSaveCommand = new RelayCommand(SectionSaveCommand_Execute, SectionSaveCommand_CanExecute);

            // 画像の追加
            PictureAddCommand = new RelayCommand(PictureAddCommand_Execute, PictureAddCommand_CanExecute);
            // 画像の編集
            PictureEditCommand = new RelayCommand(PictureEditCommand_Execute, PictureEditCommand_CanExecute);
            // 画像の表示
            PictureShowCommand = new RelayCommand(PictureShowCommand_Execute, PictureShowCommand_CanExecute);
            // 画像の削除
            PictureDeleteCommand = new RelayCommand(PictureDeleteCommand_Execute, PictureDeleteCommand_CanExecute);
            // 画像ListViewのエンター・ダブルクリック
            PictureEnterKeyCommand = new RelayCommand(PictureEnterKeyCommand_Execute, PictureEnterKeyCommand_CanExecute);

            // PDFの追加
            PdfAddCommand = new RelayCommand(PdfAddCommand_Execute, PdfAddCommand_CanExecute);
            // PDFの編集
            PdfEditCommand = new RelayCommand(PdfEditCommand_Execute, PdfEditCommand_CanExecute);
            // PDFの削除
            PdfDeleteCommand = new RelayCommand(PdfDeleteCommand_Execute, PdfDeleteCommand_CanExecute);
            // PDFの表示
            PdfShowCommand = new RelayCommand(PdfShowCommand_Execute, PdfShowCommand_CanExecute);
            // PDFListViewのエンター・ダブルクリック
            PdfEnterKeyCommand = new RelayCommand(PdfEnterKeyCommand_Execute, PdfEnterKeyCommand_CanExecute);

            // エラー通知画面を閉じる
            CloseErrorCommand = new RelayCommand(CloseErrorCommand_Execute, CloseErrorCommand_CanExecute);

            #endregion

            //ErrorOccured += new MyErrorEvent(OnError);

        }

        #region == イベントの実装 ==

        // エラーイベントの実装
        private void OnError(ErrorObject err)
        {
            if (err == null) { return; }

            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                // リストに追加。TODO：あとあとログ保存等
                //_errors.Insert(0, err);

                ErrorText = String.Format("エラー：{3}、エラー内容 {4}、 タイプ {1}、発生箇所 {2}、発生時刻 {0}", err.ErrDatetime.ToString(), err.ErrType, err.ErrPlace, err.ErrText, err.ErrDescription);

            });

            // エラーの表示
            ShowErrorDialog = true;
        }

        #endregion

        #region == メソッド ==

        // 部屋の保存（追加または更新）メソッド（コードビハインドから保存確認ダイアログでも呼ばれる）
        public bool SectionSave()
        {
            if (RentLivingRoomEdit == null)
                return false;

            if (RentLivingRooms == null)
                return false;

            // TODO: 入力チェック

            //
            RentLivingRoomEdit.RentLivingRoomPictures = RentLivingRoomPicturesTmp;
            //
            RentLivingRoomEdit.RentLivingRoomPdfs = RentLivingRoomPdfsTmp;

            // 各値の更新
            RentLivingRoomEdit.RentLivingRoomRoomNumber = RoomNumber;
            // ...

            // 部屋リストから該当オブジェクトを見つける
            var found = RentLivingRooms.FirstOrDefault(x => x.RentLivingRoomId == RentLivingRoomEdit.RentLivingRoomId);
            if (found == null)
            {
                // 追加
                RentLivingRooms.Add(RentLivingRoomEdit);
            }
            else
            {
                // 更新
                found = RentLivingRoomEdit;
            }

            // DB更新用のフラグを立てる
            RentLivingRoomEdit.IsDirty = true;

            // 変更フラグをクリア
            IsDirty = false;

            // 親画面（賃貸住居用物件）に、変更通知を送る。
            RentLivingIsDirty?.Invoke();

            return true;

        }

        // 画像編集メソッド（Listview内外のコマンドから呼ばれる）
        private void PictureEdit(RentLivingRoomPicture rlRoomPic)
        {
            OpenRentLivingRoomImageWindowEventArgs ag = new OpenRentLivingRoomImageWindowEventArgs
            {
                Id = rlRoomPic.RentSectionPictureId,
                RentLivingRoomPictureObject = rlRoomPic,
                RentLivingRoomPictures = RentLivingRoomPicturesTmp,
                IsEdit = true
            };

            // 画像編集Windowを開く
            OpenRentLivingRoomImageWindow?.Invoke(this, ag);
        }

        // 物件PDF編集メソッド（Listview内外のコマンドから呼ばれる）
        private void PdfEdit(RentLivingRoomPdf rlRoompdf)
        {
            // PDF編集Windowへ渡す為のArgをセット
            OpenRentLivingRoomPdfWindowEventArgs ag = new OpenRentLivingRoomPdfWindowEventArgs();
            ag.Id = rlRoompdf.RentSectionPdfId;
            ag.RentLivingRoomPdfObject = rlRoompdf;
            ag.RentLivingRoomPdfs = RentLivingRoomPdfsTmp;
            ag.IsEdit = true;

            // PDF編集Windowを開く
            OpenRentLivingRoomPdfWindow?.Invoke(this, ag);
        }

        #endregion

        #region == コマンドの実装 == 

        #region == 編集コマンド ==

        // 部屋の保存（追加または更新）
        public ICommand SectionSaveCommand { get; }
        public bool SectionSaveCommand_CanExecute()
        {
            if (RentLivingRoomEdit == null)
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
        public void SectionSaveCommand_Execute()
        {
            SectionSave();
        }

        #endregion

        #region == 画像編集コマンド ==

        // 物件画像追加
        public ICommand PictureAddCommand { get; }
        public bool PictureAddCommand_CanExecute()
        {
            return true;
        }
        public void PictureAddCommand_Execute()
        {
            if (RentLivingRoomEdit == null)
                return;

            var files = _openDialogService.GetOpenPictureFileDialog("部屋の写真追加");

            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 画像データの読み込み
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                            // Imageオブジェクトに読み込み。
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); 

                            // ByteArrayに変換
                            byte[] ImageData = Methods.ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = Methods.FixedSize(img, 130, 87);
                            // ByteArrayに変換
                            byte[] ImageThumbData = Methods.ImageToByteArray(thumbImg);

                            // RentLivingPictureオブジェクトを用意
                            RentLivingRoomPicture rlRoomPic = new RentLivingRoomPicture(RentLivingRoomEdit.RentId, RentLivingRoomEdit.RentLivingId, RentLivingRoomEdit.RentLivingRoomId, Guid.NewGuid().ToString());
                            rlRoomPic.PictureData = ImageData;
                            rlRoomPic.PictureThumbData = ImageThumbData;
                            rlRoomPic.PictureFileExt = fi.Extension;

                            // 画面閉じる際の確認用のフラグ。
                            rlRoomPic.IsNew = true;
                            // DBに保存する為のフラグ。
                            rlRoomPic.IsModified = true;

                            // ビットマップImageに変換（表示用）
                            rlRoomPic.Picture = Methods.BitmapImageFromImage(img, Methods.FileExtToImageFormat(rlRoomPic.PictureFileExt));
                            rlRoomPic.PictureThumb = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlRoomPic.PictureFileExt));

                            // 物件の画像リストに追加。
                            //RentLivingEdit.RentLivingPictures.Add(rlpic);

                            fs.Close();

                            // 画像編集Windowへ渡す為のArgをセット
                            OpenRentLivingRoomImageWindowEventArgs ag = new OpenRentLivingRoomImageWindowEventArgs();
                            ag.Id = rlRoomPic.RentSectionPictureId;
                            ag.RentLivingRoomPictureObject = rlRoomPic;
                            ag.RentLivingRoomPictures = RentLivingRoomPicturesTmp;// 仮リストを編集//RentLivingRoomEdit.RentLivingRoomPictures;
                            ag.IsEdit = false;

                            // 画像編集Windowを開く
                            OpenRentLivingRoomImageWindow?.Invoke(this, ag);

                        }
                    }
                }
            }

        }

        // 物件画像編集
        public ICommand PictureEditCommand { get; }
        public bool PictureEditCommand_CanExecute()
        {
            if (PicturesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void PictureEditCommand_Execute()
        {
            if (PicturesSelectedItem == null)
                return;

            PictureEdit(PicturesSelectedItem);
        }

        // 物件画像表示
        public ICommand PictureShowCommand { get; }
        public bool PictureShowCommand_CanExecute()
        {
            if (PicturesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void PictureShowCommand_Execute()
        {
            if (PicturesSelectedItem == null)
                return;

            // TODO:
        }

        // 物件PDFエンターキー・ダブルクリック
        public ICommand PictureEnterKeyCommand { get; }
        public bool PictureEnterKeyCommand_CanExecute()
        {
            if (PicturesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void PictureEnterKeyCommand_Execute()
        {
            if (PicturesSelectedItem == null)
                return;

            PictureEdit(PicturesSelectedItem);
        }

        // 物件画像削除
        public ICommand PictureDeleteCommand { get; }
        public bool PictureDeleteCommand_CanExecute()
        {
            if (PicturesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void PictureDeleteCommand_Execute()
        {
            if (PicturesSelectedItem == null)
                return;

            // TODO: "削除しますか？"確認


            // DBから削除するかどうか。
            if (PicturesSelectedItem.IsNew)
            {
                // 新規画像なので、DBにはまだ保存されていないはずなので、DBから削除は不要。
            }
            else
            {
                // DBからも削除するために、削除リストに追加（後で建物情報を保存する際に削除）
                RentLivingRoomEdit.RentLivingRoomPicturesToBeDeletedIDs.Add(PicturesSelectedItem.RentSectionPictureId);
            }

            // Tmp一覧から削除
            RentLivingRoomPicturesTmp.Remove(PicturesSelectedItem);
            // 一覧から削除 <don't
            //RentLivingRoomEdit.RentLivingRoomPictures.Remove(PicturesSelectedItem);

            // 変更フラグを立てる。
            SetIsDirty = true;

        }

        #endregion

        #region == PDF編集コマンド ==

        // 物件PDF追加
        public ICommand PdfAddCommand { get; }
        public bool PdfAddCommand_CanExecute()
        {
            return true;
        }
        public async void PdfAddCommand_Execute()
        {
            if (RentLivingRoomEdit == null) return;

            var files = _openDialogService.GetOpenZumenPdfFileDialog("図面の追加");
            
            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 図面ファイルのPDFデータの読み込み
                            byte[] PdfData;
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            long len = fs.Length;

                            // ByteArrayに変換
                            BinaryReader br = new BinaryReader(fs);
                            PdfData = br.ReadBytes((int)fs.Length);
                            br.Close();

                            fs.Close();

                            // RentLivingPdfオブジェクトの作成
                            RentLivingRoomPdf rlZumen = new RentLivingRoomPdf(RentLivingRoomEdit.RentId, RentLivingRoomEdit.RentLivingId, RentLivingRoomEdit.RentLivingRoomId, Guid.NewGuid().ToString());

                            rlZumen.PdfData = PdfData;
                            //rlZumen.FileSize = len;

                            // 画像を作成。
                            BitmapImage bitimg = await Methods.BitmapImageFromPdf(PdfData);
                            rlZumen.Picture = bitimg;

                            // ByteArrayに変換
                            byte[] ImageData = Methods.BitmapImageToByteArray(bitimg);
                            rlZumen.PdfThumbData = ImageData;

                            // TODO:
                            //rlZumen.DateTimeAdded = DateTime.Now;
                            rlZumen.DateTimePublished = DateTime.Now;
                            rlZumen.DateTimeVerified = DateTime.Now;

                            // 画面閉じる際の確認用のフラグ。
                            rlZumen.IsNew = true;
                            // DBに保存する為のフラグ。
                            rlZumen.IsModified = true;

                            // 物件のPDFリストに追加。
                            //RentLivingEdit.RentLivingPdfs.Add(rlZumen);


                            // PDF編集Windowへ渡す為のArgをセット
                            OpenRentLivingRoomPdfWindowEventArgs ag = new OpenRentLivingRoomPdfWindowEventArgs();
                            ag.Id = rlZumen.RentSectionPdfId;
                            ag.RentLivingRoomPdfObject = rlZumen;
                            ag.RentLivingRoomPdfs = RentLivingRoomPdfsTmp;//RentLivingRoomEdit.RentLivingRoomPdfs;
                            ag.IsEdit = false;

                            // PDF編集Windowを開く
                            OpenRentLivingRoomPdfWindow?.Invoke(this, ag);

                        }
                    }
                }
            }
        }

        // 物件PDF編集
        public ICommand PdfEditCommand { get; }
        public bool PdfEditCommand_CanExecute()
        {
            if (PdfsSelectedItem != null)
                return true;
            else
                return false;
        }
        public void PdfEditCommand_Execute()
        {
            if (PdfsSelectedItem == null)
                return;

            PdfEdit(PdfsSelectedItem);
        }

        // 物件PDF削除
        public ICommand PdfDeleteCommand { get; }
        public bool PdfDeleteCommand_CanExecute()
        {
            if (PdfsSelectedItem != null)
                return true;
            else
                return false;
        }
        public void PdfDeleteCommand_Execute()
        {
            if (PdfsSelectedItem == null)
                return;

            // TODO: "削除しますか？"確認
            /*
            // DBから削除するかどうか。
            if (PdfsSelectedItem.IsNew)
            {
                // 新規画像なので、DBにはまだ保存されていないはずなので、DBから削除は不要。
            }
            else
            {
                // DBからも削除するために、削除リストに追加（後で建物情報を保存する際に削除）
                RentLivingEdit.RentLivingPdfsToBeDeletedIDs.Add(PdfsSelectedItem.RentPdfId);
            }

            // 一覧から削除
            RentLivingEdit.RentLivingPdfs.Remove(PdfsSelectedItem);
            */
            // 変更フラグを立てる。
            SetIsDirty = true;
        }

        // 物件PDF表示
        public ICommand PdfShowCommand { get; }
        public bool PdfShowCommand_CanExecute()
        {
            if (PdfsSelectedItem != null)
                return true;
            else
                return false;
        }
        public void PdfShowCommand_Execute()
        {
            if (PdfsSelectedItem == null)
                return;

            // TODO:
        }

        // 物件PDFエンターキー・ダブルクリック
        public ICommand PdfEnterKeyCommand { get; }
        public bool PdfEnterKeyCommand_CanExecute()
        {
            if (PdfsSelectedItem != null)
                return true;
            else
                return false;
        }
        public void PdfEnterKeyCommand_Execute()
        {
            if (PdfsSelectedItem == null)
                return;

            PdfEdit(PdfsSelectedItem);
        }

        #endregion


        #region == 編集TODO ==
        /*
        // RL編集　新規部屋の画像追加
        public ICommand RentLivingEditSectionNewPictureAddCommand { get; }
        public bool RentLivingEditSectionNewPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionNewPictureAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;

            if (RentLivingEditSectionNew == null) return;

            var files = _openDialogService.GetOpenPictureFileDialog("部屋の写真追加");

            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 画像データの読み込み
                            byte[] ImageData;
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            //BinaryReader br = new BinaryReader(fs);
                            //ImageData = br.ReadBytes((int)fs.Length);
                            //br.Close();
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                            ImageData = Methods.ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = Methods.FixedSize(img, 200, 150);
                            byte[] ImageThumbData = Methods.ImageToByteArray(thumbImg);


                            RentLivingRoomPicture rlpic = new RentLivingRoomPicture(RentLivingEditSectionNew.Rent_ID, RentLivingEditSectionNew.RentLiving_ID, RentLivingEditSectionNew.RentLivingRoom_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsNew = true;
                            rlpic.IsModified = false;

                            rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingEditSectionNew.RentLivingRoomPictures.Add(rlpic);


                            fs.Close();
                        }
                        else
                        {
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "File";
                            er.ErrCode = 0;
                            er.ErrText = "「" + "File Does Not Exist" + "」";
                            er.ErrDescription = fileName + " ファイルが存在しません。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingEditSectionNewPictureAddCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }

        }

        // RL編集　新規部屋の画像削除
        public ICommand RentLivingEditSectionNewPictureDeleteCommand { get; }
        public bool RentLivingEditSectionNewPictureDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionNewPictureDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;

                        if (RentLivingEditSectionNew == null) return;

            // 選択アイテム保持用
            List<RentLivingRoomPicture> selectedList = new List<RentLivingRoomPicture>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingRoomPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingRoomPicture);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                RentLivingEditSectionNew.RentLivingRoomPictures.Remove(item);

                // 新規部屋なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
            }

        }

        // RL編集　編集部屋の画像追加
        public ICommand RentLivingEditSectionEditPictureAddCommand { get; }
        public bool RentLivingEditSectionEditPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionEditPictureAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionEdit == null) return;

            var files = _openDialogService.GetOpenPictureFileDialog("部屋の写真追加");

            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 画像データの読み込み
                            byte[] ImageData;
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            //BinaryReader br = new BinaryReader(fs);
                            //ImageData = br.ReadBytes((int)fs.Length);
                            //br.Close();
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                            ImageData = Methods.ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = Methods.FixedSize(img, 200, 150);
                            byte[] ImageThumbData = Methods.ImageToByteArray(thumbImg);


                            RentLivingRoomPicture rlpic = new RentLivingRoomPicture(RentLivingEditSectionEdit.RentId, RentLivingEditSectionEdit.RentLivingId, RentLivingEditSectionEdit.RentLivingRoomId, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsModified = false;
                            rlpic.IsNew = true;

                            rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingEditSectionEdit.RentLivingRoomPictures.Add(rlpic);


                            fs.Close();
                        }
                        else
                        {

                        }
                    }
                }
            }
        }

        // RL編集　編集部屋の画像削除
        public ICommand RentLivingEditSectionEditPictureDeleteCommand { get; }
        public bool RentLivingEditSectionEditPictureDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionEditPictureDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionEdit == null) return;

            // 選択アイテム保持用
            List<RentLivingRoomPicture> selectedList = new List<RentLivingRoomPicture>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingRoomPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingRoomPicture);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                if (item.IsNew)
                {
                    // 新規画像なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
                }
                else
                {
                    // DBからも削除するために、削除リストに追加（後で削除）
                    RentLivingEditSectionEdit.RentLivingRoomPicturesToBeDeletedIDs.Add(item.RentSectionPictureId);
                }

                // 一覧から削除
                RentLivingEditSectionEdit.RentLivingRoomPictures.Remove(item);
            }


        }

        */
        #endregion

        #region == エラー通知関連 ==

        // エラー通知画面のクローズコマンド
        public ICommand CloseErrorCommand { get; }
        public bool CloseErrorCommand_CanExecute()
        {
            return true;
        }
        public void CloseErrorCommand_Execute()
        {
            ShowErrorDialog = false;
        }

        #endregion

        #endregion

    }
}
