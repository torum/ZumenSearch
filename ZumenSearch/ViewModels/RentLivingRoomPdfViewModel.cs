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
    /// <summary>
    /// 賃貸住居用物件の図面編集用ViewModel
    /// </summary>
    public class RentLivingRoomPdfViewModel : ViewModelBase
    {
        // 賃貸住居用部屋の図面ID（Window識別用）（Winodow生成時に設定される）
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        // 元の賃貸住居用物件の図面オブジェクトを保持。（Winodow生成時に設定される）
        private RentLivingRoomPdf _rentLivingRoomPdfEdit;
        public RentLivingRoomPdf RentLivingRoomPdfEdit
        {
            get
            {
                return _rentLivingRoomPdfEdit;
            }
            set
            {
                if (_rentLivingRoomPdfEdit == value)
                    return;

                _rentLivingRoomPdfEdit = value;
                NotifyPropertyChanged(nameof(RentLivingRoomPdfEdit));

                // 値の設定時に、編集画面用のプロパティにそれぞれの値をポピュレイトする
                Picture = _rentLivingRoomPdfEdit.Picture;
                SelectedPdfType = _rentLivingRoomPdfEdit.PdfType;
                PdfDescription = _rentLivingRoomPdfEdit.PdfDescription;
                PdfIsMain = _rentLivingRoomPdfEdit.PdfIsMain;
                PdfTypes = _rentLivingRoomPdfEdit.PdfTypes;

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
        public ObservableCollection<RentLivingRoomPdf> RentLivingRoomPdfs { get; set; }

        #region == 編集用のプロパティ ==

        public Dictionary<string, string> PdfTypes { get; set; }

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
                this.NotifyPropertyChanged(nameof(Picture));

                // 変更フラグを立てる
                IsDirty = true;
            }
        }

        // 画像の種類
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
                NotifyPropertyChanged(nameof(SelectedPdfType));

                // 変更フラグを立てる
                IsDirty = true;
            }
        }

        // 画像のメイン画像フラグ
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
                NotifyPropertyChanged(nameof(PdfIsMain));

                // 変更フラグを立てる
                IsDirty = true;
            }
        }

        // 画像説明・コメント
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
                NotifyPropertyChanged(nameof(PdfDescription));

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

        #endregion

        #region == イベント ==

        // 親画面（賃貸住居用物件）に、（コードビハインド経由で）変更通知を送るイベント。
        public delegate void IsDirtyEventHandler();
        public event IsDirtyEventHandler RentLivingRoomIsDirty;

        #endregion

        public RentLivingRoomPdfViewModel(string id)
        {
            _id = id;

            #region == コマンド初期化 ==

            // 画像を保存
            PdfSaveCommand = new RelayCommand(PdfSaveCommand_Execute, PdfSaveCommand_CanExecute);

            #endregion

        }

        #region == メソッド ==

        // 画像の保存（追加または更新）メソッド（コードビハインドから保存確認ダイアログでも呼ばれる）
        public bool PdfSave()
        {
            if (RentLivingRoomPdfEdit == null)
                return false;

            if (RentLivingRoomPdfs == null)
                return false;

            if (IsDirty == false)
                return true;

            // TODO: 入力チェック

            //Debug.WriteLine("SelectedPictureType = " + SelectedPictureType);

            // 各値の更新
            RentLivingRoomPdfEdit.PdfDescription = PdfDescription;
            RentLivingRoomPdfEdit.PdfType = SelectedPdfType;

            // IsMainフラグだった場合、
            if (PdfIsMain)
            {
                // 一旦画像リスト内の全てのIsMainフラグをクリアする
                foreach (var hoge in RentLivingRoomPdfs)
                {
                    if (hoge.PdfIsMain)
                    {
                        hoge.PdfIsMain = false;

                        // 変更フラグを立ててDBに保存されるように
                        hoge.IsModified = true;
                    }
                }
            }
            RentLivingRoomPdfEdit.PdfIsMain = PdfIsMain;

            // 画像リストから該当オブジェクトを見つける
            var found = RentLivingRoomPdfs.FirstOrDefault(x => x.RentSectionPdfId == RentLivingRoomPdfEdit.RentSectionPdfId);
            if (found == null)
            {
                // 追加
                RentLivingRoomPdfs.Add(RentLivingRoomPdfEdit);
            }
            else
            {
                // 更新
                found = RentLivingRoomPdfEdit;
                //found.PictureIsMain = RentLivingPictureEdit.PictureIsMain;
            }

            // 新規フラグをクリア
            //RentLivingPictureEdit.IsNew = false;

            // 変更フラグをクリア
            IsDirty = false;

            // DB更新用のフラグを立てる
            RentLivingRoomPdfEdit.IsModified = true;
            // 触らない >RentLivingPictureEdit.IsNew

            // 親画面（賃貸住居用物件）に、変更通知を送る。
            RentLivingRoomIsDirty?.Invoke();

            return true;
        }

        #endregion

        #region == コマンドの実装 ==

        // 画像の保存（追加または更新）
        public ICommand PdfSaveCommand { get; }
        public bool PdfSaveCommand_CanExecute()
        {
            if (RentLivingRoomPdfEdit == null)
                return false;

            if (RentLivingRoomPdfs == null)
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

        #endregion

    }
}
