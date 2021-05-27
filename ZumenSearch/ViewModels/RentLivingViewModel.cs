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
using ZumenSearch.Models.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;

namespace ZumenSearch.ViewModels
{
    class RentLivingViewModel : ViewModelBase
    {
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        // 
        public RentLiving RentLivingEdit { get; set; }

        // 
        private RentLivingSection _rentLivingEditSectionEdit = new RentLivingSection(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLivingSection RentLivingEditSectionEdit
        {
            get
            {
                return _rentLivingEditSectionEdit;
            }
            set
            {
                if (_rentLivingEditSectionEdit == value) return;

                _rentLivingEditSectionEdit = value;
                this.NotifyPropertyChanged("RentLivingEditSectionEdit");
            }
        }

        // 
        private RentLivingSection _rentLivingEditSectionSelectedItem;
        public RentLivingSection RentLivingEditSectionSelectedItem
        {
            get
            {
                return _rentLivingEditSectionSelectedItem;
            }
            set
            {
                if (_rentLivingEditSectionSelectedItem == value) return;

                _rentLivingEditSectionSelectedItem = value;
                this.NotifyPropertyChanged("RentLivingEditSectionSelectedItem");
            }
        }

        #region == ダイアログ（サービス） ==

        // サービスのインジェクションは・・・とりあえずしない。
        //private IOpenDialogService openDialogService;
        private OpenDialogService _openDialogService = new OpenDialogService();

        #endregion

        #region == イベント） ==

        public event EventHandler<OpenRentLivingSectionWindowEventArgs> OpenRentLivingSectionWindow;

        #endregion

        public RentLivingViewModel(string id)
        {
            _id = id;

            #region == コマンド ==

            // RL 管理編集 画像追加と削除、差し替え
            RentLivingEditPictureAddCommand = new RelayCommand(RentLivingEditPictureAddCommand_Execute, RentLivingEditPictureAddCommand_CanExecute);
            RentLivingEditPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditPictureDeleteCommand_Execute(param),
                param => RentLivingEditPictureDeleteCommand_CanExecute());
            RentLivingEditPictureChangeCommand = new GenericRelayCommand<object>(
                param => RentLivingEditPictureChangeCommand_Execute(param),
                param => RentLivingEditPictureChangeCommand_CanExecute());

            // RL 管理編集　PDF追加と削除
            RentLivingEditZumenPdfAddCommand = new RelayCommand(RentLivingEditZumenPdfAddCommand_Execute, RentLivingEditZumenPdfAddCommand_CanExecute);
            RentLivingEditZumenPdfDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditZumenPdfDeleteCommand_Execute(param),
                param => RentLivingEditZumenPdfDeleteCommand_CanExecute());
            // 表示
            RentLivingEditZumenPdfShowCommand = new GenericRelayCommand<object>(
                param => RentLivingEditZumenPdfShowCommand_Execute(param),
                param => RentLivingEditZumenPdfShowCommand_CanExecute());
            // 
            RentLivingEditZumenPdfEnterCommand = new GenericRelayCommand<RentZumenPDF>(
                param => RentLivingEditZumenPdfEnterCommand_Execute(param),
                param => RentLivingEditZumenPdfEnterCommand_CanExecute());


            // RL 管理編集 新規部屋
            RentLivingEditSectionNewCommand = new RelayCommand(RentLivingEditSectionNewCommand_Execute, RentLivingEditSectionNewCommand_CanExecute);
            RentLivingEditSectionNewCancelCommand = new RelayCommand(RentLivingEditSectionNewCancelCommand_Execute, RentLivingEditSectionNewCancelCommand_CanExecute);
            RentLivingEditSectionAddCommand = new RelayCommand(RentLivingEditSectionAddCommand_Execute, RentLivingEditSectionAddCommand_CanExecute);
            // RL 管理編集 部屋編集
            RentLivingEditSectionEditCommand = new RelayCommand(RentLivingEditSectionEditCommand_Execute, RentLivingEditSectionEditCommand_CanExecute);
            RentLivingEditSectionEditCancelCommand = new RelayCommand(RentLivingEditSectionEditCancelCommand_Execute, RentLivingEditSectionEditCancelCommand_CanExecute);
            RentLivingEditSectionUpdateCommand = new RelayCommand(RentLivingEditSectionUpdateCommand_Execute, RentLivingEditSectionUpdateCommand_CanExecute);

            // RL 管理編集 部屋複製と削除
            RentLivingEditSectionDuplicateCommand = new RelayCommand(RentLivingEditSectionDuplicateCommand_Execute, RentLivingEditSectionDuplicateCommand_CanExecute);
            RentLivingEditSectionDeleteCommand = new RelayCommand(RentLivingEditSectionDeleteCommand_Execute, RentLivingEditSectionDeleteCommand_CanExecute);

            // RL 管理編集　新規部屋の画像追加と削除
            RentLivingEditSectionNewPictureAddCommand = new RelayCommand(RentLivingEditSectionNewPictureAddCommand_Execute, RentLivingEditSectionNewPictureAddCommand_CanExecute);
            RentLivingEditSectionNewPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditSectionNewPictureDeleteCommand_Execute(param),
                param => RentLivingEditSectionNewPictureDeleteCommand_CanExecute());
            // RL 管理編集　編集部屋の画像追加と削除
            RentLivingEditSectionEditPictureAddCommand = new RelayCommand(RentLivingEditSectionEditPictureAddCommand_Execute, RentLivingEditSectionEditPictureAddCommand_CanExecute);
            RentLivingEditSectionEditPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditSectionEditPictureDeleteCommand_Execute(param),
                param => RentLivingEditSectionEditPictureDeleteCommand_CanExecute());


            // エラー通知画面を閉じる
            CloseErrorCommand = new RelayCommand(CloseErrorCommand_Execute, CloseErrorCommand_CanExecute);

            #endregion

            ErrorOccured += new MyErrorEvent(OnError);

        }

        #region == 編集コマンド ==

        // RL編集　物件管理　物件画像追加
        public ICommand RentLivingEditPictureAddCommand { get; }
        public bool RentLivingEditPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditPictureAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;

            /*
            
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


                            RentLivingPicture rlpic = new RentLivingPicture(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;
                            rlpic.IsModified = false;
                            rlpic.IsNew = true;

                            rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingEdit.RentLivingPictures.Add(rlpic);


                            fs.Close();
                        }
                        else
                        {
                            // エラーイベント発火
                            MyError er = new MyError
                            {
                                ErrType = "File",
                                ErrCode = 0,
                                ErrText = "「" + "File Does Not Exist" + "」",
                                ErrDescription = fileName + " ファイルが存在しません。",
                                ErrDatetime = DateTime.Now,
                                ErrPlace = "MainViewModel::RentLivingEditAddCommand_Execute()"
                            };
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
*/


        }

        // RL編集　物件管理　物件画像削除
        public ICommand RentLivingEditPictureDeleteCommand { get; }
        public bool RentLivingEditPictureDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditPictureDeleteCommand_Execute(object obj)
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
                    RentLivingEdit.RentLivingPicturesToBeDeletedIDs.Add(item.RentPicture_ID);
                }

                // 一覧から削除
                RentLivingEdit.RentLivingPictures.Remove(item);
            }

        }

        // RL編集　物件管理　物件画像差し替え更新
        public ICommand RentLivingEditPictureChangeCommand { get; }
        public bool RentLivingEditPictureChangeCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditPictureChangeCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;

            System.Collections.IList items = (System.Collections.IList)obj;

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
                                rlpic.PictureThumbW200xData = ImageThumbData;
                                rlpic.PictureFileExt = fi.Extension;
                                rlpic.IsModified = true;
                                rlpic.IsNew = false;

                                rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

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
                                er.ErrPlace = "MainViewModel::RentLivingEditPictureChangeCommand_Execute()";
                                ErrorOccured?.Invoke(er);
                            }
                        }

                        // multi = false なので、
                        break;
                    }
                }
            }



        }


        // RL編集　部屋追加（画面表示）
        public ICommand RentLivingEditSectionNewCommand { get; }
        public bool RentLivingEditSectionNewCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionNewCommand_Execute()
        {
            if (RentLivingEdit == null) return;

            // RentLivingNewオブジェクトを用意
            RentLivingSection RlS = new RentLivingSection(RentLivingEdit.Rent_ID, _id, Guid.NewGuid().ToString());
            RlS.IsNew = true;

            OpenRentLivingSectionWindowEventArgs ag = new OpenRentLivingSectionWindowEventArgs();
            ag.Id = RentLivingEdit.Rent_ID;
            ag.EditObject = RlS;

            OpenRentLivingSectionWindow?.Invoke(this, ag);
            
        }

        // RL編集　部屋追加キャンセル
        public ICommand RentLivingEditSectionNewCancelCommand { get; }
        public bool RentLivingEditSectionNewCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionNewCancelCommand_Execute()
        {
            //if (ShowRentLivingEditSectionNew) ShowRentLivingEditSectionNew = false;
        }

        // RL編集　部屋追加処理 (Add to the collection)
        public ICommand RentLivingEditSectionAddCommand { get; }
        public bool RentLivingEditSectionAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;

            /*
            if (RentLivingEditSectionNew == null) return;

            // TODO: 入力チェック

            // 物件オブジェクトの部屋コレクションに追加
            RentLivingEdit.RentLivingSections.Add(RentLivingEditSectionNew);

            // 追加画面を閉じる
            ShowRentLivingEditSectionNew = false;

*/
        }

        // RL編集　部屋編集（画面表示）
        public ICommand RentLivingEditSectionEditCommand { get; }
        public bool RentLivingEditSectionEditCommand_CanExecute()
        {
            if (RentLivingEditSectionSelectedItem != null)
                return true;
            else
                return false;
        }
        public void RentLivingEditSectionEditCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionSelectedItem == null) return;

            //
            RentLivingEditSectionEdit = RentLivingEditSectionSelectedItem;

            /*
            RentLivingEditSectionEdit = new RentLivingSection(RentLivingEditSectionSelectedItem.Rent_ID, RentLivingEditSectionSelectedItem.RentLiving_ID, RentLivingEditSectionSelectedItem.RentLivingSection_ID);

            RentLivingEditSectionEdit.IsNew = false;
            RentLivingEditSectionEdit.IsDirty = false;

            RentLivingEditSectionEdit.RentLivingSectionRoomNumber = RentLivingEditSectionSelectedItem.RentLivingSectionRoomNumber;
            RentLivingEditSectionEdit.RentLivingSectionMadori = RentLivingEditSectionSelectedItem.RentLivingSectionMadori;
            RentLivingEditSectionEdit.RentLivingSectionPrice = RentLivingEditSectionSelectedItem.RentLivingSectionPrice;
            // TODO: more to come

            // TODO: これは・・・
            foreach (var hoge in RentLivingEditSectionSelectedItem.RentLivingSectionPictures)
            {
                RentLivingEditSectionEdit.RentLivingSectionPictures.Add(hoge);
            }
            */

            //if (!ShowRentLivingEditSectionEdit) ShowRentLivingEditSectionEdit = true;
        }

        // RL編集　部屋編集キャンセル
        public ICommand RentLivingEditSectionEditCancelCommand { get; }
        public bool RentLivingEditSectionEditCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionEditCancelCommand_Execute()
        {
            //if (ShowRentLivingEditSectionEdit) ShowRentLivingEditSectionEdit = false;
        }

        // RL編集　部屋更新処理 (Update Collection)
        public ICommand RentLivingEditSectionUpdateCommand { get; }
        public bool RentLivingEditSectionUpdateCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionUpdateCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionSelectedItem == null) return;

            // TODO: 入力チェック

            /*
            var found = RentLivingEdit.RentLivingSections.FirstOrDefault(x => x.RentLivingSection_ID == RentLivingEditSectionSelectedItem.RentLivingSection_ID);
            if (found != null)
            {
                found.RentLivingSectionRoomNumber = RentLivingEditSectionEdit.RentLivingSectionRoomNumber;
                found.RentLivingSectionMadori = RentLivingEditSectionEdit.RentLivingSectionMadori;
                found.RentLivingSectionPrice = RentLivingEditSectionEdit.RentLivingSectionPrice;
                // TODO: more to come

                // 一旦クリアして追加しなおさないと、変更が通知（更新）されない
                found.RentLivingSectionPictures.Clear();
                foreach (var hoge in RentLivingEditSectionEdit.RentLivingSectionPictures)
                {
                    found.RentLivingSectionPictures.Add(hoge);
                }


            }
            else
            {
                System.Diagnostics.Debug.WriteLine("THIS SHOULD NOT BE HAPPENING @RentLivingEditSectionUpdateCommand_Execute");
            }
            */

            // 部屋編集画面を閉じる
            //ShowRentLivingEditSectionEdit = false;
        }

        // RL編集　部屋一覧の選択を複製
        public ICommand RentLivingEditSectionDuplicateCommand { get; }
        public bool RentLivingEditSectionDuplicateCommand_CanExecute()
        {
            if (RentLivingEditSectionSelectedItem != null)
                return true;
            else
                return false;
        }
        public void RentLivingEditSectionDuplicateCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionSelectedItem == null) return;
            /*
            
            //
            RentLivingEditSectionNew = new RentLivingSection(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Guid.NewGuid().ToString());
            RentLivingEditSectionNew.IsNew = true;
            RentLivingEditSectionNew.IsDirty = false;

            RentLivingEditSectionNew.RentLivingSectionRoomNumber = RentLivingEditSectionSelectedItem.RentLivingSectionRoomNumber + "の複製";
            RentLivingEditSectionNew.RentLivingSectionMadori = RentLivingEditSectionSelectedItem.RentLivingSectionMadori;
            RentLivingEditSectionNew.RentLivingSectionPrice = RentLivingEditSectionSelectedItem.RentLivingSectionPrice;
            // TODO: more to come

            // 追加
            RentLivingEdit.RentLivingSections.Add(RentLivingEditSectionNew);
*/
        }

        // RL編集　部屋一覧の選択を削除
        public ICommand RentLivingEditSectionDeleteCommand { get; }
        public bool RentLivingEditSectionDeleteCommand_CanExecute()
        {
            if (RentLivingEditSectionSelectedItem != null)
                return true;
            else
                return false;
        }
        public void RentLivingEditSectionDeleteCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionSelectedItem == null) return;

            //RentLivingSectionToBeDeletedIDs
            if (RentLivingEditSectionSelectedItem.IsNew)
            {
                // 新規なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
            }
            else
            {
                // DBからも削除するために、削除リストに追加（後で削除）
                RentLivingEdit.RentLivingSectionToBeDeletedIDs.Add(RentLivingEditSectionSelectedItem.RentLivingSection_ID);
            }

            // 削除
            RentLivingEdit.RentLivingSections.Remove(RentLivingEditSectionSelectedItem);
        }


        // RL編集　新規部屋の画像追加
        public ICommand RentLivingEditSectionNewPictureAddCommand { get; }
        public bool RentLivingEditSectionNewPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionNewPictureAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            /*

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


                            RentLivingSectionPicture rlpic = new RentLivingSectionPicture(RentLivingEditSectionNew.Rent_ID, RentLivingEditSectionNew.RentLiving_ID, RentLivingEditSectionNew.RentLivingSection_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsNew = true;
                            rlpic.IsModified = false;

                            rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingEditSectionNew.RentLivingSectionPictures.Add(rlpic);


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

*/

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

            /*
                        if (RentLivingEditSectionNew == null) return;

            // 選択アイテム保持用
            List<RentLivingSectionPicture> selectedList = new List<RentLivingSectionPicture>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingSectionPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingSectionPicture);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                RentLivingEditSectionNew.RentLivingSectionPictures.Remove(item);

                // 新規部屋なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
            }
*/


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


                            RentLivingSectionPicture rlpic = new RentLivingSectionPicture(RentLivingEditSectionEdit.Rent_ID, RentLivingEditSectionEdit.RentLiving_ID, RentLivingEditSectionEdit.RentLivingSection_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsModified = false;
                            rlpic.IsNew = true;

                            rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingEditSectionEdit.RentLivingSectionPictures.Add(rlpic);


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
                            er.ErrPlace = "MainViewModel::RentLivingEditSectionEditPictureAddCommand_Execute()";
                            ErrorOccured?.Invoke(er);
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
            List<RentLivingSectionPicture> selectedList = new List<RentLivingSectionPicture>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingSectionPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingSectionPicture);
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
                    RentLivingEditSectionEdit.RentLivingSectionPicturesToBeDeletedIDs.Add(item.RentSectionPicture_ID);
                }

                // 一覧から削除
                RentLivingEditSectionEdit.RentLivingSectionPictures.Remove(item);
            }


        }


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

                    RentLivingZumenPDF rlZumen = new RentLivingZumenPDF(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Guid.NewGuid().ToString());
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
                    // エラーイベント発火
                    MyError er = new MyError();
                    er.ErrType = "File";
                    er.ErrCode = 0;
                    er.ErrText = "「" + "File Does Not Exist" + "」";
                    er.ErrDescription = fileName + " ファイルが存在しません。";
                    er.ErrDatetime = DateTime.Now;
                    er.ErrPlace = "MainViewModel::RentLivingEditZumenPdfAddCommand_Execute()";
                    ErrorOccured?.Invoke(er);
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
                    RentLivingEdit.RentLivingZumenPdfToBeDeletedIDs.Add(item.RentZumenPDF_ID);
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
                        cmd.CommandText = String.Format("SELECT PdfData FROM RentLivingZumenPdf WHERE RentLivingZumenPdf_ID = '{0}'", (item as RentLivingZumenPDF).RentZumenPDF_ID);
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
                    cmd.CommandText = String.Format("SELECT PdfData FROM RentLivingZumenPdf WHERE RentLivingZumenPdf_ID = '{0}'", (obj as RentZumenPDF).RentZumenPDF_ID);
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

        #region == エラー通知画面周り ==

        #region == エラーイベント ==

        public delegate void MyErrorEvent(MyError err);
        public event MyErrorEvent ErrorOccured;

        // エラーイベントの実装
        private void OnError(MyError err)
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

    }
}
