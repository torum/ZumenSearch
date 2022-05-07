using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;
using System.Collections.ObjectModel;

namespace ZumenSearch.ViewModels.Classes
{
    // 賃貸住居用　建物 Window
    public class OpenRentLivingBuildingWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 編集用賃貸住居用物件オブジェクト
        public RentLiving RentLivingObject { get; set; }

        // データ編集用DBアクセスモジュール
        public DataAccess DataAccessModule { get; set; }
    }

    // 賃貸住居用　部屋 Window
    public class OpenRentLivingRoomWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 賃貸住居用物件に属する部屋リスト
        public ObservableCollection<RentLivingRoom> RentLivingRooms { get; set; }

        // 部屋オブジェクト
        public RentLivingRoom RentLivingRoomObject { get; set; }

        // データ編集用DBアクセスモジュール
        //public DataAccess DataAccessModule { get; set; }

    }

    // 賃貸住居用　建物画像 Window
    public class OpenRentLivingImageWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 賃貸住居用物件に属する画像リスト
        public ObservableCollection<RentLivingPicture> RentLivingPictures { get; set; }

        // 画像オブジェクト
        public RentLivingPicture RentLivingPictureObject { get; set; }

        // 編集モードか新規モードかのフラグ（編集画面でIsDirtyのセットに使う）
        public bool IsEdit { get; set; }

        // データ編集用DBアクセスモジュール
        //public DataAccess DataAccessModule { get; set; }

    }

    // 賃貸住居用　建物PDF Window
    public class OpenRentLivingPdfWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 賃貸住居用物件に属する画像リスト
        public ObservableCollection<RentLivingPdf> RentLivingPdfs { get; set; }

        // 画像オブジェクト
        public RentLivingPdf RentLivingPdfObject { get; set; }

        // 編集モードか新規モードかのフラグ（編集画面でIsDirtyのセットに使う）
        public bool IsEdit { get; set; }

        // データ編集用DBアクセスモジュール
        //public DataAccess DataAccessModule { get; set; }

    }

    // 賃貸住居用　部屋画像 Window
    public class OpenRentLivingRoomImageWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 賃貸住居用物件に属する画像リスト
        public ObservableCollection<RentLivingRoomPicture> RentLivingRoomPictures { get; set; }

        // 画像オブジェクト
        public RentLivingRoomPicture RentLivingRoomPictureObject { get; set; }

        // 編集モードか新規モードかのフラグ（編集画面でIsDirtyのセットに使う）
        public bool IsEdit { get; set; }

    }

    // 賃貸住居用　部屋図面 Window
    public class OpenRentLivingRoomPdfWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 賃貸住居用物件に属する図面リスト
        public ObservableCollection<RentLivingRoomPdf> RentLivingRoomPdfs { get; set; }

        // 図面オブジェクト
        public RentLivingRoomPdf RentLivingRoomPdfObject { get; set; }

        // 編集モードか新規モードかのフラグ（編集画面でIsDirtyのセットに使う）
        public bool IsEdit { get; set; }

    }


}
