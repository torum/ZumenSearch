using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Common;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Models
{

    /// <summary>
    /// 部屋・区画等の基底クラス
    /// </summary>
    public class Section : ViewModelBase
    {
        // 賃貸IDの保持
        protected string _rentId;
        public string RentId
        {
            get
            {
                return _rentId;
            }
        }

        // 空きフラグ
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

                IsModified = true;
            }
        }

        // 新規か編集（保存済み）かどうかのフラグ。
        private bool _isNew;
        public bool IsNew
        {
            get
            {
                return _isNew;
            }
            set
            {
                if (_isNew == value) return;

                _isNew = value;
                NotifyPropertyChanged("IsNew");
                NotifyPropertyChanged("StatusIsNew");
            }
        }

        // 表示用のステータス
        public string StatusIsNew
        {
            get
            {
                if (IsNew)
                    return "新規";
                else
                    return "編集";
            }
        }

        // 変更があったかどうかの（DB更新用）フラグ。
        private bool _isModified = false;
        public bool IsModified
        {
            get
            {
                return _isModified;
            }
            set
            {
                if (_isModified == value) return;

                _isModified = value;
                NotifyPropertyChanged("IsModified");
            }
        }
    }

    /// <summary>
    /// 賃貸住居用の部屋クラス
    /// </summary>
    public class RentLivingRoom : Section
    {
        // 建物IDの保持
        protected string _rentLivingId;
        public string RentLivingId
        {
            get
            {
                return _rentLivingId;
            }
        }

        // 部屋ID
        protected string _rentLivingSectionId;
        public string RentLivingRoomId
        {
            get
            {
                return _rentLivingSectionId;
            }
        }

        // 区分所有かどうか
        protected bool _isOwnershipTypeUnit;
        public bool IsOwnershipTypeUnit
        {
            get
            {
                return _isOwnershipTypeUnit;
            }
            set
            {
                if (_isOwnershipTypeUnit == value) 
                    return;

                _isOwnershipTypeUnit = value;
                this.NotifyPropertyChanged("IsOwnershipTypeUnit");
            }
        }

        // 部屋番号・号室
        private string _rentLivingSectionRoomNumber;
        public string RentLivingRoomRoomNumber
        {
            get
            {
                return _rentLivingSectionRoomNumber;
            }
            set
            {
                if (_rentLivingSectionRoomNumber == value) return;

                _rentLivingSectionRoomNumber = value;
                this.NotifyPropertyChanged("RentLivingRoomRoomNumber");

                // 変更フラグ
                IsModified = true;
            }
        }

        // 賃料
        private int _rentLivingSectionPrice;
        public int RentLivingRoomPrice
        {
            get
            {
                return _rentLivingSectionPrice;
            }
            set
            {
                if (_rentLivingSectionPrice == value) return;

                _rentLivingSectionPrice = value;
                this.NotifyPropertyChanged("RentLivingRoomPrice");

                // 変更フラグ
                IsModified = true;
            }
        }

        // 間取り
        private string _rentLivingSectionMadori; // TODO 1K, 2K...
        public string RentLivingRoomMadori
        {
            get
            {
                return _rentLivingSectionMadori;
            }
            set
            {
                if (_rentLivingSectionMadori == value) return;

                _rentLivingSectionMadori = value;
                this.NotifyPropertyChanged("RentLivingRoomMadori");

                // 変更フラグ
                IsModified = true;
            }
        }

        // 部屋写真コレクション
        public ObservableCollection<RentLivingRoomPicture> RentLivingRoomPictures { get; set; } = new ObservableCollection<RentLivingRoomPicture>();

        // DBへの更新時にDBから削除されるべき部屋写真のIDリスト
        public List<string> RentLivingRoomPicturesToBeDeletedIDs = new List<string>();

        public RentLivingRoom(string rentid, string rentlivingid, string sectionid)
        {
            _rentId = rentid;
            _rentLivingId = rentlivingid;
            _rentLivingSectionId = sectionid;
        }
    }

    /// <summary>
    /// 賃貸事業用の区画クラス
    /// </summary>
    public class RentBusinessSuite : Section
    {

    }
}
