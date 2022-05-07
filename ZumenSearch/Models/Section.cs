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

        private string _pathIcon = "M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
        public string PathIcon
        {
            get
            {
                return _pathIcon;
            }
            set
            {
                if (_pathIcon == value)
                    return;

                _pathIcon = value;
                NotifyPropertyChanged(nameof(PathIcon));
            }
        }

        #region == ステータスフラグ == 

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
                NotifyPropertyChanged("IsEdit");
                NotifyPropertyChanged("StatusIsNew");
                NotifyPropertyChanged("StatusIsDirty");
            }
        }

        // 新規か編集（保存済み）かどうかのフラグ。
        public bool IsEdit
        {
            get
            {
                if (IsNew)
                    return false;
                else
                    return true;
            }
        }

        // 変更（データ入力）があったかどうかのフラグ。
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
                NotifyPropertyChanged("StatusIsNew");
                NotifyPropertyChanged("StatusIsDirty");
            }
        }

        public string StatusIsNew
        {
            get
            {
                if (IsNew && IsDirty)
                    return "：新規";
                else if (IsNew)
                    return "：新規";
                else if (IsEdit && IsDirty)
                    return "：編集";
                else if (IsEdit)
                    return "：編集";
                else
                    return "";
            }
        }

        public string StatusIsDirty
        {
            get
            {
                if (IsNew && IsDirty)
                    return "変更";
                else if (IsNew)
                    return "";
                else if (IsEdit && IsDirty)
                    return "変更";
                else if (IsEdit)
                    return "";
                else
                    return "";
            }
        }

        #endregion
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
                this.NotifyPropertyChanged(nameof(IsOwnershipTypeUnit));
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
                this.NotifyPropertyChanged(nameof(RentLivingRoomRoomNumber));

                // 変更フラグ
                IsDirty = true;
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
                this.NotifyPropertyChanged(nameof(RentLivingRoomPrice));

                // 変更フラグ
                IsDirty = true;
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
                this.NotifyPropertyChanged(nameof(RentLivingRoomMadori));

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 部屋写真コレクション
        public ObservableCollection<RentLivingRoomPicture> RentLivingRoomPictures { get; set; } = new ObservableCollection<RentLivingRoomPicture>();

        // DBへの更新時にDBから削除されるべき部屋写真のIDリスト
        public List<string> RentLivingRoomPicturesToBeDeletedIDs = new List<string>();

        // 部屋図面コレクション
        public ObservableCollection<RentLivingRoomPdf> RentLivingRoomPdfs { get; set; } = new ObservableCollection<RentLivingRoomPdf>();

        // DBへの更新時にDBから削除されるべき部屋図面のIDリスト
        public List<string> RentLivingRoomPdfsToBeDeletedIDs = new List<string>();

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
