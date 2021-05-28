using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Models.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;

namespace ZumenSearch.ViewModels.Classes
{
    public class OpenRentLivingWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 編集用賃貸住居用物件オブジェクト
        public RentLiving EditObject { get; set; }

        // データ編集用DBアクセスモジュール
        public DataAccess DataAccessModule { get; set; }
    }

    public class OpenRentLivingSectionWindowEventArgs : EventArgs
    {
        // Window識別用ID
        public string Id { get; set; }

        // 編集用賃貸住居用部屋オブジェクト
        public RentLivingSection EditObject { get; set; }

        // データ編集用DBアクセスモジュール
        public DataAccess DataAccessModule { get; set; }

    }

}
