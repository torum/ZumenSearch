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
using ZumenSearch.Models.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;

namespace ZumenSearch.ViewModels
{
    public class RentLivingSectionViewModel : ViewModelBase
    {
        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        public RentLivingSection RentLivingRoomEdit { get; set; }

        public RentLivingSectionViewModel(string id)
        {
            _id = id;

            #region == コマンド ==



            // エラー通知画面を閉じる
            CloseErrorCommand = new RelayCommand(CloseErrorCommand_Execute, CloseErrorCommand_CanExecute);

            #endregion

            ErrorOccured += new MyErrorEvent(OnError);

        }

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


    }
}
