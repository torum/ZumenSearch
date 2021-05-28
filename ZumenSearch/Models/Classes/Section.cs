using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZumenSearch.Common;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Models.Classes
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
                NotifyPropertyChanged("Status");
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
                NotifyPropertyChanged("Status");
            }
        }

        // 編集ステータスの情報表示。
        public string Status
        {
            get
            {
                if (IsNew && IsDirty)
                    return "[新規] [変更あり]";
                else if (IsNew)
                    return "[新規]";
                else if (IsEdit && IsDirty)
                    return "[更新] [変更あり]";
                else if (IsEdit)
                    return "[更新]";
                else
                    return "";
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

                IsDirty = true;
            }
        }
    }

    /// <summary>
    /// 賃貸住居用の部屋クラス
    /// </summary>
    public class RentLivingSection : Section
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
        public string RentLivingSectionId
        {
            get
            {
                return _rentLivingSectionId;
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

                // 変更フラグ
                IsDirty = true;
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

                // 変更フラグ
                IsDirty = true;
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

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 部屋写真コレクション
        public ObservableCollection<RentLivingSectionPicture> RentLivingSectionPictures { get; set; } = new ObservableCollection<RentLivingSectionPicture>();

        // DBへの更新時にDBから削除されるべき部屋写真のIDリスト
        public List<string> RentLivingSectionPicturesToBeDeletedIDs = new List<string>();

        public RentLivingSection(string rentid, string rentlivingid, string sectionid)
        {
            _rentId = rentid;
            _rentLivingId = rentlivingid;
            _rentLivingSectionId = sectionid;
        }
    }

}
