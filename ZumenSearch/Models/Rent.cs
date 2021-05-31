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

namespace ZumenSearch.Models
{
    // 物件（建物等）の基底クラス
    public class Rent : ViewModelBase
    {
        // 賃貸ID（GUID & Primary Key）
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
                //NotifyPropertyChanged("Status");
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
                //NotifyPropertyChanged("Status");
                NotifyPropertyChanged("StatusIsNew");
                NotifyPropertyChanged("StatusIsDirty");
            }
        }

        /*
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
        */

        public string StatusIsNew
        {
            get
            {
                if (IsNew && IsDirty)
                    return "新規";
                else if (IsNew)
                    return "新規";
                else if (IsEdit && IsDirty)
                    return "更新";
                else if (IsEdit)
                    return "更新";
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

        // 物件名
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

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 賃貸の種類ラベル
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

        // 賃貸の種類
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

        // 所在地〒
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

        // 所在地
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

        // 最寄り駅１
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

        // 最寄り駅２
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

    // 賃貸住居用の物件クラス（建物）
    public class RentLiving : Rent
    {
        // 建物ID
        protected string _rentLivingId;
        public string RentLivingId
        {
            get
            {
                return _rentLivingId;
            }
        }

        // 賃貸住居用の物件種別ラベル
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

        // 賃貸住居用の物件種別　アパート・マンション・戸建て・他
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


        // 写真一覧
        public ObservableCollection<RentLivingPicture> RentLivingPictures { get; set; } = new ObservableCollection<RentLivingPicture>();

        // 写真のDBへの更新時にDBから削除されるべき物件写真のIDリスト
        public List<string> RentLivingPicturesToBeDeletedIDs = new List<string>();

        // 図面一覧
        public ObservableCollection<RentLivingPdf> RentLivingPdfs { get; set; } = new ObservableCollection<RentLivingPdf>();

        // 図面のDBへの更新時にDBから削除されるべき図面のIDリスト
        public List<string> RentLivingZumenPdfToBeDeletedIDs = new List<string>();

        // 部屋一覧
        public ObservableCollection<RentLivingRoom> RentLivingRooms { get; set; } = new ObservableCollection<RentLivingRoom>();

        // DBへの更新時にDBから削除されるべき部屋のIDリスト
        public List<string> RentLivingRoomToBeDeletedIDs = new List<string>();

        // コンストラクタ
        public RentLiving(string rentid, string rentlivingid)
        {
            _rentId = rentid;
            _rentLivingId = rentlivingid;

            _type = RentTypes.RentLiving;
        }

    }

}
