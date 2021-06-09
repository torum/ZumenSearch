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
using System.Windows.Media;

namespace ZumenSearch.ViewModels
{
    ///
    /// TODO: 画像ファイルのサイズと拡張子の種類を表示し、画像ファイルのサイズが大きすぎる場合は警告を出す。
    /// 

    /// <summary>
    /// 賃貸住居用物件の画像編集用ViewModel
    /// </summary>
    public class RentLivingImageViewModel : ViewModelBase
    {
        // 賃貸住居用物件の画像ID（Window識別用）（Winodow生成時に設定される）
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        // 元の賃貸住居用物件の画像オブジェクトを保持。（Winodow生成時に設定される）
        private RentLivingPicture _rentLivingPictureEdit;
        public RentLivingPicture RentLivingPictureEdit
        {
            get
            {
                return _rentLivingPictureEdit;
            }
            set
            {
                if (_rentLivingPictureEdit == value)
                    return;

                _rentLivingPictureEdit = value;
                NotifyPropertyChanged("RentLivingPictureEdit");

                // 値の設定時に、編集画面用のプロパティにそれぞれの値をポピュレイトする
                Picture = _rentLivingPictureEdit.Picture;
                SelectedPictureType = _rentLivingPictureEdit.PictureType;
                PictureDescription = _rentLivingPictureEdit.PictureDescription;
                PictureIsMain = _rentLivingPictureEdit.PictureIsMain;

                // 変更フラグをクリアする（ユーザーの入力で変更・編集された訳ではないので）

                // TODO: リストに存在しなければ新規＞IsDirty, else IsDirty = false;
                /*
                if (_rentLivingPictureEdit.IsNew)
                {
                    IsDirty = true;
                }
                else
                {
                    IsDirty = false;
                }
                   */

            }
        }

        // 元の賃貸住居用物件の画像リストを保持。（Winodow生成時に設定される）
        public ObservableCollection<RentLivingPicture> RentLivingPictures { get; set; }

        #region == 編集用のプロパティ ==

        // 画像プレビュー
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
                IsDirty = true;
            }
        }

        // 画像の種類
        private string _selectedPictureType;
        public string SelectedPictureType
        {
            get
            {
                return _selectedPictureType;
            }
            set
            {
                if (_selectedPictureType == value)
                    return;

                _selectedPictureType = value;
                NotifyPropertyChanged("SelectedPictureType");

                // 変更フラグを立てる
                IsDirty = true;
            }
        }

        // 画像のメイン画像フラグ
        private bool _pictureIsMain;
        public bool PictureIsMain
        {
            get
            {
                return _pictureIsMain;
            }
            set
            {
                if (_pictureIsMain == value)
                    return;

                _pictureIsMain = value;
                NotifyPropertyChanged("PictureIsMain");

                // 変更フラグを立てる
                IsDirty = true;
            }
        }

        // 画像説明・コメント
        private string _pictureDescription;
        public string PictureDescription
        {
            get
            {
                return _pictureDescription;
            }
            set
            {
                if (_pictureDescription == value)
                    return;

                _pictureDescription = value;
                NotifyPropertyChanged("PictureDescription");

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

        public RentLivingImageViewModel(string id)
        {
            _id = id;

            #region == コマンド初期化 ==

            // 画像を保存
            PictureSaveCommand = new RelayCommand(PictureSaveCommand_Execute, PictureSaveCommand_CanExecute);

            #endregion

        }

        #region == イベントの実装 ==


        #endregion

        #region == コマンドの実装 ==

        // 画像の保存（追加または更新）
        public ICommand PictureSaveCommand { get; }
        public bool PictureSaveCommand_CanExecute()
        {
            if (RentLivingPictureEdit == null)
                return false;

            if (RentLivingPictures == null)
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
        public void PictureSaveCommand_Execute()
        {
            PictureSave();
        }

        // 画像の保存（追加または更新）メソッド（コードビハインドから保存確認ダイアログでも呼ばれる）
        public bool PictureSave()
        {
            if (RentLivingPictureEdit == null)
                return false;

            if (RentLivingPictures == null)
                return false;

            if (IsDirty == false)
                return true;

            // TODO: 入力チェック

            //Debug.WriteLine("SelectedPictureType = " + SelectedPictureType);

            // 各値の更新
            RentLivingPictureEdit.PictureDescription = PictureDescription;
            RentLivingPictureEdit.PictureType = SelectedPictureType;

            // IsMainフラグだった場合、
            if (PictureIsMain)
            {
                // 一旦画像リスト内の全てのIsMainフラグをクリアする
                foreach (var hoge in RentLivingPictures)
                {
                    if (hoge.PictureIsMain)
                    {
                        hoge.PictureIsMain = false;
                        
                        // 変更フラグを立ててDBに保存されるように
                        hoge.IsModified = true;
                    }
                }
            }
            RentLivingPictureEdit.PictureIsMain = PictureIsMain;


            // 画像リストから該当オブジェクトを見つける
            var found = RentLivingPictures.FirstOrDefault(x => x.RentPictureId == RentLivingPictureEdit.RentPictureId);
            if (found == null)
            {
                // 追加
                RentLivingPictures.Add(RentLivingPictureEdit);
            }
            else
            {
                // 更新
                found = RentLivingPictureEdit;
            }

            // 新規フラグをクリア
            //RentLivingPictureEdit.IsNew = false;

            // 変更フラグをクリア
            IsDirty = false;

            // DB更新用のフラグを立てる
            RentLivingPictureEdit.IsModified = true;
            // 触らない >RentLivingPictureEdit.IsNew

            // 親画面（賃貸住居用物件）に、変更通知を送る。
            RentLivingIsDirty?.Invoke();

            return true;
        }

        #endregion

    }
}
