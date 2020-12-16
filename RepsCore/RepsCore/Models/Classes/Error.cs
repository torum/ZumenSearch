using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepsCore.Models.Classes
{

    /// <summary>
    /// 情報保持・表示用のMyErrorクラス
    /// </summary>
    #region == エラー情報保持・表示用クラス ==

    public class MyError
    {
        public string ErrType { get; set; } // eg "API, DB, other"
        public int ErrCode { get; set; } // HTTP ERROR CODE?
        public string ErrText { get; set; } // API error code translated via dictionaly.
        public string ErrPlace { get; set; } // eg RESTのPATH。
        public string ErrPlaceParent { get; set; } // ?
        public DateTime ErrDatetime { get; set; }
        public string ErrDescription { get; set; } // 自前の補足説明。
    }

    #endregion

}
