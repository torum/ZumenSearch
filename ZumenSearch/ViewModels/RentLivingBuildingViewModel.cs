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
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;
using System.Windows.Media;

namespace ZumenSearch.ViewModels
{
    /// <summary>
    /// 賃貸住居用 建物 のViewModel
    /// </summary>
    public class RentLivingBuildingViewModel : ViewModelBase
    {
        // 賃貸住居用建物のID（Window識別用）（Winodow生成時に設定される）
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        // 賃貸住居用建物の編集用のオブジェクト（Winodow生成時に設定される）
        private RentLiving _rentLivingEdit;
        public RentLiving RentLivingEdit
        {
            get
            {
                return _rentLivingEdit;
            }
            set
            {
                if (_rentLivingEdit == value)
                    return;

                _rentLivingEdit = value;
                NotifyPropertyChanged("RentLivingEdit");

                // 値の設定時に、編集画面用のプロパティにそれぞれの値をポピュレイトする
                //

                // 変更フラグをクリアする（ユーザーの入力で変更・編集された訳ではないので）
                //IsDirty = false;

                // メイン画像を取得
                UpdateMainPicture();

            }
        }

        // データ編集用DBアクセスモジュール（Winodow生成時に設定される）
        public DataAccess DataAccessModule;

        // 画像リストの選択項目を保持
        private RentLivingPicture _picturesSelectedItem;
        public RentLivingPicture PicturesSelectedItem
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
                this.NotifyPropertyChanged("PicturesSelectedItem");
            }
        }

        // 部屋リストの選択項目を保持
        private RentLivingSection _sectionsSelectedItem;
        public RentLivingSection SectionsSelectedItem
        {
            get
            {
                return _sectionsSelectedItem;
            }
            set
            {
                if (_sectionsSelectedItem == value) return;

                _sectionsSelectedItem = value;
                this.NotifyPropertyChanged("SectionsSelectedItem");
            }
        }

        // 変更通知受け取り。（画像リストなど、変更があった場合にイベント経由でコードビハインドから呼ばれる
        public bool SetIsDirty
        {
            set
            {
                if (value)
                    if (RentLivingEdit != null)
                        RentLivingEdit.IsDirty = value;

                // メイン画像の更新
                // TODO:もっと効率の良い場所はないか・・・メイン画像が変更されたイベント等。
                UpdateMainPicture();
            }
        }

        // メイン画像
        private ImageSource _picture;
        public ImageSource Picture
        {
            get
            {
                return _picture;
            }
            set
            {
                if (_picture == value)
                    return;

                _picture = value;
                this.NotifyPropertyChanged("Picture");

            }
        }

        // メイン画像の更新メソッド
        private void UpdateMainPicture()
        {
            if (_rentLivingEdit == null)
                return;

            // メイン画像を取得
            var rlpic = _rentLivingEdit.RentLivingPictures.FirstOrDefault(RentPicture => RentPicture.PictureIsMain == true);
            if (rlpic != null)
                Picture = rlpic.Picture;
            else
                Picture = null;
        }

        #region == エラー通知画面周り ==

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
                this.NotifyPropertyChanged("ShowErrorDialog");
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
                this.NotifyPropertyChanged("ErrorText");
            }
        }


        #endregion

        #region == ダイアログ（サービス） ==

        // サービスのインジェクションは・・・とりあえずしない。
        //private IOpenDialogService openDialogService;
        private OpenDialogService _openDialogService = new OpenDialogService();

        #endregion

        #region == イベント ==

        // 部屋編集画面を開く
        public event EventHandler<OpenRentLivingSectionWindowEventArgs> OpenRentLivingSectionWindow;

        // 画像編集画面を開く
        public event EventHandler<OpenRentLivingImagesWindowEventArgs> OpenRentLivingImagesWindow;
        
        // エラー通知イベント
        public delegate void MyErrorEvent(ErrorObject err);
        public event MyErrorEvent ErrorOccured;

        #endregion

        #region == アクション ==

        //public Action CloseAction { get; set; }

        #endregion

        public RentLivingBuildingViewModel(string id)
        {
            _id = id;

            #region == コマンド初期化 ==

            // 建物情報の保存・更新
            SaveCommand = new RelayCommand(SaveCommand_Execute, SaveCommand_CanExecute);

            // 画像の追加
            PictureAddCommand = new RelayCommand(PictureAddCommand_Execute, PictureAddCommand_CanExecute);

            // 画像の編集
            PictureEditCommand = new RelayCommand(PictureEditCommand_Execute, PictureEditCommand_CanExecute);

            // 画像の編集（Listview）
            PictureEditListviewCommand = new GenericRelayCommand<object>(
                param => PictureEditListviewCommand_Execute(param),
                param => PictureEditListviewCommand_CanExecute());

            // 画像の削除
            PictureDeleteCommand = new RelayCommand(PictureDeleteCommand_Execute, PictureDeleteCommand_CanExecute);

            // 画像の削除（Listview）
            PictureDeleteListviewCommand = new GenericRelayCommand<object>(
                param => PictureDeleteListviewCommand_Execute(param),
                param => PictureDeleteListviewCommand_CanExecute());

            // 画像の差替え（Listview）
            PictureChangeListviewCommand = new GenericRelayCommand<object>(
                param => PictureChangeListviewCommand_Execute(param),
                param => PictureChangeListviewCommand_CanExecute());

            // PDFの追加
            RentLivingEditZumenPdfAddCommand = new RelayCommand(RentLivingEditZumenPdfAddCommand_Execute, RentLivingEditZumenPdfAddCommand_CanExecute);
            // PDFの削除
            RentLivingEditZumenPdfDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditZumenPdfDeleteCommand_Execute(param),
                param => RentLivingEditZumenPdfDeleteCommand_CanExecute());
            // PDFの表示
            RentLivingEditZumenPdfShowCommand = new GenericRelayCommand<object>(
                param => RentLivingEditZumenPdfShowCommand_Execute(param),
                param => RentLivingEditZumenPdfShowCommand_CanExecute());
            // PDFの
            RentLivingEditZumenPdfEnterCommand = new GenericRelayCommand<RentZumenPDF>(
                param => RentLivingEditZumenPdfEnterCommand_Execute(param),
                param => RentLivingEditZumenPdfEnterCommand_CanExecute());

            // 部屋の追加
            SectionNewCommand = new RelayCommand(SectionNewCommand_Execute, SectionNewCommand_CanExecute);
            // 部屋の編集
            SectionEditCommand = new RelayCommand(SectionEditCommand_Execute, SectionEditCommand_CanExecute);
            // 部屋の複製
            SectionDuplicateCommand = new RelayCommand(SectionDuplicateCommand_Execute, SectionDuplicateCommand_CanExecute);
            // 部屋の削除
            SectionDeleteCommand = new RelayCommand(SectionDeleteCommand_Execute, SectionDeleteCommand_CanExecute);

            // 
            /*
            // RL 管理新規　新規部屋の画像追加と削除
            RentLivingNewSectionNewPictureAddCommand = new RelayCommand(RentLivingNewSectionNewPictureAddCommand_Execute, RentLivingNewSectionNewPictureAddCommand_CanExecute);
            RentLivingNewSectionNewPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingNewSectionNewPictureDeleteCommand_Execute(param),
                param => RentLivingNewSectionNewPictureDeleteCommand_CanExecute());
            // RL 管理新規　編集部屋の画像追加と削除
            RentLivingNewSectionEditPictureAddCommand = new RelayCommand(RentLivingNewSectionEditPictureAddCommand_Execute, RentLivingNewSectionEditPictureAddCommand_CanExecute);
            RentLivingNewSectionEditPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingNewSectionEditPictureDeleteCommand_Execute(param),
                param => RentLivingNewSectionEditPictureDeleteCommand_CanExecute());
            */

            #endregion

            #region == イベントへのサブスクライブ ==

            ErrorOccured += new MyErrorEvent(OnError);

            #endregion

        }

        #region == イベントの実装 ==

        // エラーイベント
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

        #region == コマンドの実装 ==

        #region == 編集コマンド ==

        // 物件の保存（追加または更新）
        public ICommand SaveCommand { get; }
        public bool SaveCommand_CanExecute()
        {
            if (RentLivingEdit == null) 
                return false;
            if (DataAccessModule == null) 
                return false;

            if (RentLivingEdit.IsDirty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SaveCommand_Execute()
        {
            Save();
        }

        // 物件の保存（追加または更新）メソッド（コードビハインドから保存確認ダイアログでも呼ばれる）
        public bool Save()
        {
            if (RentLivingEdit == null)
                return false;
            if (DataAccessModule == null)
                return false;

            if (RentLivingEdit.IsDirty == false)
                return true;

            // TODO: 入力チェック

            if (RentLivingEdit.IsNew)
            {
                //物件追加（INSERT）
                ResultWrapper res = DataAccessModule.RentLivingInsert(RentLivingEdit);
                if (res.IsError)
                {
                    // エラー通知
                    ErrorOccured?.Invoke(res.Error);

                    return false;
                }
            }
            else
            {
                // 物件更新（UPDATE）
                ResultWrapper res = DataAccessModule.RentLivingUpdate(RentLivingEdit);
                if (res.IsError)
                {
                    // エラー通知
                    ErrorOccured?.Invoke(res.Error);

                    return false;
                }
            }

            RentLivingEdit.IsNew = false;
            RentLivingEdit.IsDirty = false;

            return true;
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
            if (RentLivingEdit == null) 
                return;

            var files = _openDialogService.GetOpenPictureFileDialog("物件写真の追加");

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
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                            // ByteArrayに変換
                            byte[] ImageData;
                            ImageData = Methods.ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = Methods.FixedSize(img, 130, 87);
                            // ByteArrayに変換
                            byte[] ImageThumbData = Methods.ImageToByteArray(thumbImg);

                            // RentLivingPictureオブジェクトを用意
                            RentLivingPicture rlpic = new RentLivingPicture(RentLivingEdit.RentId, RentLivingEdit.RentLivingId, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            // DBに保存する為のフラグ。
                            rlpic.IsNew = true;
                            // DBに保存する為のフラグ。
                            rlpic.IsModified = true;

                            // ビットマップImageに変換（表示用）
                            rlpic.Picture = Methods.BitmapImageFromImage(img, Methods.FileExtToImageFormat(rlpic.PictureFileExt));
                            rlpic.PictureThumb = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

                            // 物件の画像リストに追加。
                            //RentLivingEdit.RentLivingPictures.Add(rlpic);

                            fs.Close();

                            // 画像編集Windowへ渡す為のArgをセット
                            OpenRentLivingImagesWindowEventArgs ag = new OpenRentLivingImagesWindowEventArgs();
                            ag.Id = rlpic.RentPictureId;
                            ag.RentLivingPictureObject = rlpic;
                            ag.RentLivingPictures = RentLivingEdit.RentLivingPictures;

                            // 画像編集Windowを開く
                            OpenRentLivingImagesWindow?.Invoke(this, ag);

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

        // 物件画像編集（listview内）
        public ICommand PictureEditListviewCommand { get; }
        public bool PictureEditListviewCommand_CanExecute()
        {
            return true;
        }
        public void PictureEditListviewCommand_Execute(object obj)
        {
            if (obj == null) 
                return;

            if (RentLivingEdit == null) 
                return;

            System.Collections.IList items = (System.Collections.IList)obj;

            if (items.Count > 0)
            {
                RentLivingPicture rlpic = items.Cast<RentLivingPicture>().FirstOrDefault();

                if (rlpic == null)
                    return;

                PictureEdit(rlpic);
            }
        }

        // 物件画像編集メソッド（Listview内外のコマンドから呼ばれる）
        private void PictureEdit(RentLivingPicture rlpic)
        {
            // 画像編集Windowへ渡す為のArgをセット
            OpenRentLivingImagesWindowEventArgs ag = new OpenRentLivingImagesWindowEventArgs();
            ag.Id = rlpic.RentPictureId;
            ag.RentLivingPictureObject = rlpic;
            ag.RentLivingPictures = RentLivingEdit.RentLivingPictures;

            // 画像編集Windowを開く
            OpenRentLivingImagesWindow?.Invoke(this, ag);
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

            // DBから削除するかどうか。
            if (PicturesSelectedItem.IsNew)
            {
                // 新規画像なので、DBにはまだ保存されていないはずなので、DBから削除は不要。
            }
            else
            {
                // DBからも削除するために、削除リストに追加（後で建物情報を保存する際に削除）
                RentLivingEdit.RentLivingPicturesToBeDeletedIDs.Add(PicturesSelectedItem.RentPictureId);
            }

            // 一覧から削除
            RentLivingEdit.RentLivingPictures.Remove(PicturesSelectedItem);

            // 変更フラグを立てる。
            SetIsDirty = true;
        }

        // 物件画像削除（listview内）
        public ICommand PictureDeleteListviewCommand { get; }
        public bool PictureDeleteListviewCommand_CanExecute()
        {
            return true;
        }
        public void PictureDeleteListviewCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;

            // 選択アイテム保持用
            List<RentLivingPicture> selectedList = new List<RentLivingPicture>();
            // キャンセルする注文IDを保持
            //List<int> cancelIdList = new List<int>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingPicture);
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
                    RentLivingEdit.RentLivingPicturesToBeDeletedIDs.Add(item.RentPictureId);
                }

                // 一覧から削除
                RentLivingEdit.RentLivingPictures.Remove(item);
            }

            // 変更フラグを立てる。
            SetIsDirty = true;
        }

        // TODO: 画像Windowに移動
        // 物件画像差し替え更新
        public ICommand PictureChangeListviewCommand { get; }
        public bool PictureChangeListviewCommand_CanExecute()
        {
            if (PicturesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void PictureChangeListviewCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;

            System.Collections.IList items = (System.Collections.IList)obj;
            /*
            if (items.Count > 0)
            {
                RentLivingPicture rlpic = items.Cast<RentLivingPicture>().First();

                var files = _openDialogService.GetOpenPictureFileDialog("物件写真の差し替え", false);

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

                                // データ更新
                                rlpic.PictureData = ImageData;
                                rlpic.PictureThumbData = ImageThumbData;
                                rlpic.PictureFileExt = fi.Extension;
                                rlpic.IsDirty = true;
                                rlpic.IsNew = false;

                                rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

                                fs.Close();
                            }
                            else
                            {

                            }
                        }

                        // multi = false なので、
                        break;
                    }
                }
            }
            */


        }

        #endregion

        #region == 部屋編集コマンド ==

        // 部屋の追加（画面表示）
        public ICommand SectionNewCommand { get; }
        public bool SectionNewCommand_CanExecute()
        {
            return true;
        }
        public void SectionNewCommand_Execute()
        {
            if (RentLivingEdit == null) 
                return;

            // RentLivingSection Newオブジェクトを用意
            RentLivingSection rlsection = new RentLivingSection(RentLivingEdit.RentId, _id, Guid.NewGuid().ToString());
            rlsection.IsNew = true;

            OpenRentLivingSectionWindowEventArgs ag = new OpenRentLivingSectionWindowEventArgs();
            ag.Id = rlsection.RentLivingSectionId;
            ag.RentLivingSectionObject = rlsection;
            ag.RentLivingSections = RentLivingEdit.RentLivingSections;

            OpenRentLivingSectionWindow?.Invoke(this, ag);
        }

        // 部屋の編集（画面表示）
        public ICommand SectionEditCommand { get; }
        public bool SectionEditCommand_CanExecute()
        {
            if (SectionsSelectedItem != null)
                return true;
            else
                return false;
        }
        public void SectionEditCommand_Execute()
        {
            if (RentLivingEdit == null) 
                return;
            if (SectionsSelectedItem == null) 
                return;

            OpenRentLivingSectionWindowEventArgs ag = new OpenRentLivingSectionWindowEventArgs();
            ag.Id = SectionsSelectedItem.RentLivingSectionId;
            ag.RentLivingSectionObject = SectionsSelectedItem;
            ag.RentLivingSections = RentLivingEdit.RentLivingSections;

            OpenRentLivingSectionWindow?.Invoke(this, ag);
        }

        // 部屋を複製 TODO
        public ICommand SectionDuplicateCommand { get; }
        public bool SectionDuplicateCommand_CanExecute()
        {
            if (SectionsSelectedItem != null)
                return true;
            else
                return false;
        }
        public void SectionDuplicateCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (SectionsSelectedItem == null) return;
            /*
            
            //
            RentLivingEditSectionNew = new RentLivingSection(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Guid.NewGuid().ToString());
            RentLivingEditSectionNew.IsNew = true;
            RentLivingEditSectionNew.IsDirty = false;

            RentLivingEditSectionNew.RentLivingSectionRoomNumber = SectionsSelectedItem.RentLivingSectionRoomNumber + "の複製";
            RentLivingEditSectionNew.RentLivingSectionMadori = SectionsSelectedItem.RentLivingSectionMadori;
            RentLivingEditSectionNew.RentLivingSectionPrice = SectionsSelectedItem.RentLivingSectionPrice;
            // TODO: more to come

            // 追加
            RentLivingEdit.RentLivingSections.Add(RentLivingEditSectionNew);
*/
        }

        // 部屋の削除
        public ICommand SectionDeleteCommand { get; }
        public bool SectionDeleteCommand_CanExecute()
        {
            if (SectionsSelectedItem != null)
                return true;
            else
                return false;
        }
        public void SectionDeleteCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (SectionsSelectedItem == null) return;

            //RentLivingSectionToBeDeletedIDs
            if (SectionsSelectedItem.IsNew)
            {
                // 新規なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
            }
            else
            {
                // DBからも削除するために、削除リストに追加（後で削除）
                RentLivingEdit.RentLivingSectionToBeDeletedIDs.Add(SectionsSelectedItem.RentLivingSectionId);
            }

            // 部屋リストから削除
            RentLivingEdit.RentLivingSections.Remove(SectionsSelectedItem);
        }

        #endregion

        #region == 図面編集コマンド ==

        // RL編集　物件の図面PDF追加
        public ICommand RentLivingEditZumenPdfAddCommand { get; }
        public bool RentLivingEditZumenPdfAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditZumenPdfAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;

            string fileName = _openDialogService.GetOpenZumenPdfFileDialog("図面の追加");

            if (!string.IsNullOrEmpty(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Exists)
                {
                    // 図面ファイルのPDFデータの読み込み
                    byte[] PdfData;
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    long len = fs.Length;

                    BinaryReader br = new BinaryReader(fs);
                    PdfData = br.ReadBytes((int)fs.Length);
                    br.Close();

                    RentLivingZumenPDF rlZumen = new RentLivingZumenPDF(RentLivingEdit.RentId, RentLivingEdit.RentLivingId, Guid.NewGuid().ToString());
                    rlZumen.PDFData = PdfData;
                    rlZumen.FileSize = len;

                    // TODO:
                    //rlZumen.DateTimeAdded = DateTime.Now;
                    rlZumen.DateTimePublished = DateTime.Now;
                    rlZumen.DateTimeVerified = DateTime.Now;

                    rlZumen.IsDirty = false;
                    rlZumen.IsNew = true;

                    RentLivingEdit.RentLivingZumenPDFs.Add(rlZumen);

                    fs.Close();
                }
                else
                {
                    /*
                    // エラーイベント発火
                    MyError er = new MyError();
                    er.ErrType = "File";
                    er.ErrCode = 0;
                    er.ErrText = "「" + "File Does Not Exist" + "」";
                    er.ErrDescription = fileName + " ファイルが存在しません。";
                    er.ErrDatetime = DateTime.Now;
                    er.ErrPlace = "MainViewModel::RentLivingEditZumenPdfAddCommand_Execute()";
                    ErrorOccured?.Invoke(er);
                    */
                }
            }
        }

        // RL編集　物件の図面PDF削除
        public ICommand RentLivingEditZumenPdfDeleteCommand { get; }
        public bool RentLivingEditZumenPdfDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditZumenPdfDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;

            // 選択アイテム保持用
            List<RentLivingZumenPDF> selectedList = new List<RentLivingZumenPDF>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingZumenPDF>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingZumenPDF);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                if (item.IsNew)
                {
                    // 新規なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
                }
                else
                {
                    // DBからも削除するために、削除リストに追加（後で削除）
                    RentLivingEdit.RentLivingZumenPdfToBeDeletedIDs.Add(item.RentZumenPdfId);
                }

                // 一覧から削除
                RentLivingEdit.RentLivingZumenPDFs.Remove(item);
            }

        }

        // RL編集　物件の図面PDF表示
        public ICommand RentLivingEditZumenPdfShowCommand { get; }
        public bool RentLivingEditZumenPdfShowCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditZumenPdfShowCommand_Execute(object obj)
        {
            if (obj == null) return;
            if (RentLivingEdit == null) return;

            /*
            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingZumenPDF>();

            foreach (var item in collection)
            {

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT PdfData FROM RentLivingZumenPdf WHERE RentLivingZumenPdf_ID = '{0}'", (item as RentLivingZumenPDF).RentZumenPdfId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                byte[] pdfBytes = (byte[])reader["PdfData"];

                                File.WriteAllBytes(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf", pdfBytes);

                                Process.Start(new ProcessStartInfo(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf") { UseShellExecute = true });

                                break;
                            }
                        }
                    }
                }

                break;

            }
*/


        }

        // RL編集　物件の図面PDF表示（ダブルクリックやエンター押下で）
        public ICommand RentLivingEditZumenPdfEnterCommand { get; }
        public bool RentLivingEditZumenPdfEnterCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditZumenPdfEnterCommand_Execute(RentZumenPDF obj)
        {
            if (obj == null) return;
            if (RentLivingEdit == null) return;

            /*
                        using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = String.Format("SELECT PdfData FROM RentLivingZumenPdf WHERE RentLivingZumenPdf_ID = '{0}'", (obj as RentZumenPDF).RentZumenPdfId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] pdfBytes = (byte[])reader["PdfData"];

                            File.WriteAllBytes(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf", pdfBytes);

                            Process.Start(new ProcessStartInfo(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf") { UseShellExecute = true });

                            break;
                        }
                    }
                }
            }

*/



        }

        #endregion

        #region == エラー通知関連コマンド ==

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
