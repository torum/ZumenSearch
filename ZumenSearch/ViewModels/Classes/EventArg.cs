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
    public class OpenRentLivingBuildingWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 編集用賃貸住居用物件オブジェクト
        public RentLiving RentLivingObject { get; set; }

        // データ編集用DBアクセスモジュール
        public DataAccess DataAccessModule { get; set; }
    }

    public class OpenRentLivingSectionWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 賃貸住居用物件に属する部屋リスト
        public ObservableCollection<RentLivingSection> RentLivingSections { get; set; }

        // 部屋オブジェクト
        public RentLivingSection RentLivingSectionObject { get; set; }

        // データ編集用DBアクセスモジュール
        //public DataAccess DataAccessModule { get; set; }

    }

    public class OpenRentLivingImagesWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 賃貸住居用物件に属する画像リスト
        public ObservableCollection<RentLivingPicture> RentLivingPictures { get; set; }

        // 画像オブジェクト
        public RentLivingPicture RentLivingPictureObject { get; set; }

        // データ編集用DBアクセスモジュール
        //public DataAccess DataAccessModule { get; set; }

    }

}
