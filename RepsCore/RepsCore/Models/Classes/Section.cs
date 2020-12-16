using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RepsCore.Common;
using RepsCore.ViewModels;

namespace RepsCore.Models.Classes
{

    /// <summary>
    /// 部屋・区画等の基底クラス
    /// </summary>
    public class Section : ViewModelBase
    {
        protected string _rent_ID;
        public string Rent_ID
        {
            get
            {
                return _rent_ID;
            }
        }

        private bool _isVacant;
        public bool IsVacant
        {
            get
            {
                return _isVacant;
            }
            set
            {
                if (_isVacant == value) return;

                _isVacant = value;
                this.NotifyPropertyChanged("IsVacant");
            }
        }


        // 新規に追加（Insert）
        public bool IsNew { get; set; }

        // 変更があった
        public bool IsDirty { get; set; }

    }

    /// <summary>
    /// 賃貸住居用の部屋クラス
    /// </summary>
    public class RentLivingSection : Section
    {
        protected string _rentLiving_ID;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_ID;
            }
        }

        protected string _rentLivingSection_ID;
        public string RentLivingSection_ID
        {
            get
            {
                return _rentLivingSection_ID;
            }
        }

        // 部屋番号
        private string _rentLivingSectionRoomNumber;
        public string RentLivingSectionRoomNumber
        {
            get
            {
                return _rentLivingSectionRoomNumber;
            }
            set
            {
                if (_rentLivingSectionRoomNumber == value) return;

                _rentLivingSectionRoomNumber = value;
                this.NotifyPropertyChanged("RentLivingSectionRoomNumber");
            }
        }

        // 賃料
        private int _rentLivingSectionPrice;
        public int RentLivingSectionPrice
        {
            get
            {
                return _rentLivingSectionPrice;
            }
            set
            {
                if (_rentLivingSectionPrice == value) return;

                _rentLivingSectionPrice = value;
                this.NotifyPropertyChanged("RentLivingSectionPrice");
            }
        }

        // 間取り
        private string _rentLivingSectionMadori; // TODO 1K, 2K...
        public string RentLivingSectionMadori
        {
            get
            {
                return _rentLivingSectionMadori;
            }
            set
            {
                if (_rentLivingSectionMadori == value) return;

                _rentLivingSectionMadori = value;
                this.NotifyPropertyChanged("RentLivingSectionMadori");
            }
        }

        // 部屋写真コレクション
        public ObservableCollection<RentLivingSectionPicture> RentLivingSectionPictures { get; set; } = new ObservableCollection<RentLivingSectionPicture>();

        // DBへの更新時にDBから削除されるべき部屋写真のIDリスト
        public List<string> RentLivingSectionPicturesToBeDeletedIDs = new List<string>();

        public RentLivingSection(string rentid, string rentlivingid, string sectionid)
        {
            this._rent_ID = rentid;
            this._rentLiving_ID = rentlivingid;
            this._rentLivingSection_ID = sectionid;
        }
    }

}
