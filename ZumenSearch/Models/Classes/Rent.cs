using System;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using ZumenSearch.Common;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Models.Classes
{
    /// <summary>
    /// 物件（建物等）の基底クラス
    /// </summary>
    public class Rent : ViewModelBase
    {
        // GUID and Primary Key
        protected string _rent_id;
        public string Rent_ID
        {
            get
            {
                return _rent_id;
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value) return;

                _name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        public Dictionary<RentTypes, string> RentTypeToLabel { get; } = new Dictionary<RentTypes, string>()
        {
            {RentTypes.RentLiving, "賃貸住居用"},
            {RentTypes.RentBussiness, "賃貸事業用"},
            {RentTypes.RentParking, "賃貸駐車場"},
        };

        public Dictionary<string, RentTypes> StringToRentType { get; } = new Dictionary<string, RentTypes>()
        {
            {"RentLiving", RentTypes.RentLiving},
            {"RentBussiness", RentTypes.RentBussiness},
            {"RentParking", RentTypes.RentParking},
        };

        protected RentTypes _type;
        public RentTypes Type
        {
            get
            {
                return _type;
            }
        }

        public string TypeLabel
        {
            get
            {
                return this.RentTypeToLabel[this.Type];
            }
        }

        private string _postalCode;
        public string PostalCode
        {
            get
            {
                return _postalCode;
            }
            set
            {
                if (_postalCode == value) return;

                _postalCode = value;
                this.NotifyPropertyChanged("PostalCode");
            }
        }

        private string _location;
        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                if (_location == value) return;

                _location = value;
                this.NotifyPropertyChanged("Location");
            }
        }

        private string _trainStation1;
        public string TrainStation1
        {
            get
            {
                return _trainStation1;
            }
            set
            {
                if (_trainStation1 == value) return;

                _trainStation1 = value;
                this.NotifyPropertyChanged("TrainStation1");
            }
        }

        private string _trainStation2;
        public string TrainStation2
        {
            get
            {
                return _trainStation2;
            }
            set
            {
                if (_trainStation2 == value) return;

                _trainStation2 = value;
                this.NotifyPropertyChanged("TrainStation2");
            }
        }

        public Rent()
        {

        }
    }

    /// <summary>
    /// 賃貸住居用の物件クラス（建物）
    /// </summary>
    public class RentLiving : Rent
    {
        protected string _rentLiving_id;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_id;
            }
        }

        // 新規か編集（保存済み）かどうかのフラグ。
        public bool IsNew { get; set; }
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

        // 変更があったかどうかのフラグ。
        public bool IsDirty { get; set; }

        public Dictionary<RentLivingKinds, string> RentLivingKindToLabel { get; } = new Dictionary<RentLivingKinds, string>()
        {
            {RentLivingKinds.Apartment, "アパート"},
            {RentLivingKinds.Mansion, "マンション"},
            {RentLivingKinds.House, "一戸建て"},
            {RentLivingKinds.Other, "その他"},
        };

        public Dictionary<string, RentLivingKinds> StringToRentLivingKind { get; } = new Dictionary<string, RentLivingKinds>()
        {
            {"Apartment", RentLivingKinds.Apartment},
            {"Mansion", RentLivingKinds.Mansion},
            {"House", RentLivingKinds.House},
            {"Other", RentLivingKinds.Other},
        };

        // 賃貸住居用物件種別　アパート・マンション・戸建て・他
        private RentLivingKinds _kind;
        public RentLivingKinds Kind
        {
            get
            {
                return _kind;
            }
            set
            {
                if (_kind == value) return;

                _kind = value;
                this.NotifyPropertyChanged("Kind");
            }
        }

        // 所有権（一棟所有・区分所有）
        private RentOwnerships _ownership;
        public RentOwnerships Ownership
        {
            get
            {
                return _ownership;
            }
            set
            {
                if (_ownership == value) return;

                _ownership = value;
                this.NotifyPropertyChanged("Ownership");
            }
        }

        public Dictionary<string, RentOwnerships> StringToRentOwnership { get; } = new Dictionary<string, RentOwnerships>()
        {
            {"Unit", RentOwnerships.Unit},
            {"All", RentOwnerships.All},
        };

        // 地上n階建て
        private int _floors;
        public int Floors
        {
            get
            {
                return _floors;
            }
            set
            {
                if (_floors == value) return;

                _floors = value;
                this.NotifyPropertyChanged("Floors");
            }
        }

        // 地下n階建て
        private int _floorsBasement;
        public int FloorsBasement
        {
            get
            {
                return _floorsBasement;
            }
            set
            {
                if (_floorsBasement == value) return;

                _floorsBasement = value;
                this.NotifyPropertyChanged("FloorsBasement");
            }
        }

        // 築年
        private int _builtYear;
        public int BuiltYear
        {
            get
            {
                return _builtYear;
            }
            set
            {
                if (_builtYear == value) return;

                _builtYear = value;
                this.NotifyPropertyChanged("BuiltYear");
            }
        }

        // 物件写真一覧
        public ObservableCollection<RentLivingPicture> RentLivingPictures { get; set; } = new ObservableCollection<RentLivingPicture>();

        // 物件写真のDBへの更新時にDBから削除されるべき物件写真のIDリスト
        public List<string> RentLivingPicturesToBeDeletedIDs = new List<string>();

        // 図面一覧
        public ObservableCollection<RentLivingZumenPDF> RentLivingZumenPDFs { get; set; } = new ObservableCollection<RentLivingZumenPDF>();

        // 図面のDBへの更新時にDBから削除されるべき図面のIDリスト
        public List<string> RentLivingZumenPdfToBeDeletedIDs = new List<string>();

        // 部屋一覧
        public ObservableCollection<RentLivingSection> RentLivingSections { get; set; } = new ObservableCollection<RentLivingSection>();

        // DBへの更新時にDBから削除されるべき部屋のIDリスト
        public List<string> RentLivingSectionToBeDeletedIDs = new List<string>();

        // コンストラクタ
        public RentLiving(string rentid, string rentlivingid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;

            this._type = RentTypes.RentLiving;
        }

    }


}
