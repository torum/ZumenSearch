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

namespace ZumenSearch.ViewModels
{
    /// <summary>
    /// 賃貸住居用 部屋 のViewModel
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
                NotifyPropertyChanged("RentLivingRoomEdit");

                // 値の設定時に、編集画面用のプロパティにそれぞれの値をポピュレイトする
                RoomNumber = _rentLivingRoomEdit.RentLivingRoomRoomNumber;

                // 変更フラグをクリアする（ユーザーの入力で変更・編集された訳ではないので）
                IsDirty = false;
            }
        }

        // 元の賃貸住居用物件の部屋リストを保持。（Winodow生成時に設定される）
        public ObservableCollection<RentLivingRoom> RentLivingRooms { get; set; }

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
                NotifyPropertyChanged("RoomNumber");

                // 変更フラグを立てる
                IsDirty = true;
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

        #region == イベント ==

        public delegate void MyErrorEvent(ErrorObject err);
        public event MyErrorEvent ErrorOccured;
        
        // 親画面（賃貸住居用物件）に、（コードビハインド経由で）変更通知を送るイベント。
        public delegate void IsDirtyEventHandler();
        public event IsDirtyEventHandler RentLivingIsDirty;

        #endregion

        public RentLivingRoomViewModel(string id)
        {
            _id = id;

            #region == コマンド ==

            // 部屋情報の保存・更新
            SectionSaveCommand = new RelayCommand(SectionSaveCommand_Execute, SectionSaveCommand_CanExecute);

            // RL 管理編集　新規部屋の画像追加と削除
            //RentLivingEditSectionNewPictureAddCommand = new RelayCommand(RentLivingEditSectionNewPictureAddCommand_Execute, RentLivingEditSectionNewPictureAddCommand_CanExecute);
            //RentLivingEditSectionNewPictureDeleteCommand = new GenericRelayCommand<object>(
            //    param => RentLivingEditSectionNewPictureDeleteCommand_Execute(param),
            //    param => RentLivingEditSectionNewPictureDeleteCommand_CanExecute());
            // RL 管理編集　編集部屋の画像追加と削除
            //RentLivingEditSectionEditPictureAddCommand = new RelayCommand(RentLivingEditSectionEditPictureAddCommand_Execute, RentLivingEditSectionEditPictureAddCommand_CanExecute);
            //RentLivingEditSectionEditPictureDeleteCommand = new GenericRelayCommand<object>(
            //    param => RentLivingEditSectionEditPictureDeleteCommand_Execute(param),
            //    param => RentLivingEditSectionEditPictureDeleteCommand_CanExecute());

            // エラー通知画面を閉じる
            CloseErrorCommand = new RelayCommand(CloseErrorCommand_Execute, CloseErrorCommand_CanExecute);

            #endregion

            ErrorOccured += new MyErrorEvent(OnError);

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

        #region == コマンドの実装 == 

        #region == 編集コマンド ==

        // 部屋の保存（追加または更新）
        public ICommand SectionSaveCommand { get; }
        public bool SectionSaveCommand_CanExecute()
        {
            if (RentLivingRoomEdit == null)
                return false;
            //if (DataAccessModule == null)
            //    return false;

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

        // 部屋の保存（追加または更新）メソッド（コードビハインドから保存確認ダイアログでも呼ばれる）
        public bool SectionSave()
        {
            if (RentLivingRoomEdit == null)
                return false;

            //if (DataAccessModule == null)
            //    return false;

            if (RentLivingRooms == null)
                return false;

            if (IsDirty == false)
                return true;

            // TODO: 入力チェック

            //Debug.WriteLine(RoomNumber);

            // 各値の更新
            RentLivingRoomEdit.RentLivingRoomRoomNumber = RoomNumber;

            // 画像リストから該当オブジェクトを見つける
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
            /*
            if (RentLivingRoomEdit.IsNew)
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
            */

            // 変更フラグをクリア
            IsDirty = false;

            // DB更新用のフラグを立てる
            RentLivingRoomEdit.IsModified = true;

            // 親画面（賃貸住居用物件）に、変更通知を送る。
            RentLivingIsDirty?.Invoke();

            return true;

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
