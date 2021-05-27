using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reps.Models
{
    // make this public
    public class SearchRealestateResult
    {
        #region フィールド

        #endregion

        #region プロパティ

        public string GUID { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 居住用・駐車場・事業用
        /// </summary>
        public string RealestateType { get; set; }

        //// 最寄駅
        ////    徒歩

        public string Address { get; set; }

        //// 貸主名

        protected string OwnerName { get; set; }

        protected string OwnerGUID { get; set; }

        #endregion

    }
}