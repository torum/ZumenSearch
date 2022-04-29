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

    // 賃貸物件のタイプ（住居用・事業用・駐車場）
    public enum RentTypes
    {
        RentLiving, RentBussiness, RentParking
    }

    // 一括所有の建物か、区分所有か
    public enum Ownerships
    {
        Unknown, All, Unit
    }

    // 賃貸住居用物件種別（アパート・マンション・一戸建て・他）
    public enum RentLivingKinds
    {
        Apartment, Mansion, House, Other
    }

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

        public Dictionary<string, Ownerships> StringToRentOwnership { get; } = new Dictionary<string, Ownerships>()
        {
            {"Unknown", Ownerships.Unit},
            {"All", Ownerships.All},
            {"Unit", Ownerships.Unit},
        };

        // 所有権（一棟所有・区分所有）
        private Ownerships _ownership;
        public Ownerships Ownership
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

                // 変更フラグ
                IsDirty = true;
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

                // 変更フラグ
                IsDirty = true;
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

                // 変更フラグ
                IsDirty = true;
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

                // 変更フラグ
                IsDirty = true;
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

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 最寄り駅３
        private string _trainStation3;
        public string TrainStation3
        {
            get
            {
                return _trainStation3;
            }
            set
            {
                if (_trainStation3 == value) return;

                _trainStation3 = value;
                this.NotifyPropertyChanged("TrainStation3");

                // 変更フラグ
                IsDirty = true;
            }
        }

        public Rent()
        {

        }
    }

    // 物件（建物等）の検索結果表示用クラス
    public class RentLivingSummary : Rent
    {

        // Icon Path
        private static string _rsNew = "M12 5C15.87 5 19 8.13 19 12C19 15.87 15.87 19 12 19C8.13 19 5 15.87 5 12C5 8.13 8.13 5 12 5M12 2C17.5 2 22 6.5 22 12C22 17.5 17.5 22 12 22C6.5 22 2 17.5 2 12C2 6.5 6.5 2 12 2M12 4C7.58 4 4 7.58 4 12C4 16.42 7.58 20 12 20C16.42 20 20 16.42 20 12C20 7.58 16.42 4 12 4Z";
        private static string _rsNormal = "M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
        private static string _rsVisited = "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M12 20C7.59 20 4 16.41 4 12S7.59 4 12 4 20 7.59 20 12 16.41 20 12 20M16.59 7.58L10 14.17L7.41 11.59L6 13L10 17L18 9L16.59 7.58Z";



        // 建物ID
        protected string _rentLivingId;
        public string RentLivingId
        {
            get
            {
                return _rentLivingId;
            }
        }

        // For display.
        private ImageSource _pictureThumb;
        public ImageSource PictureThumb
        {
            get
            {
                return _pictureThumb;
            }
            set
            {
                if (_pictureThumb == value) return;

                _pictureThumb = value;
                this.NotifyPropertyChanged("PictureThumb");
            }
        }

        private string _pathIcon = "M12 5C15.87 5 19 8.13 19 12C19 15.87 15.87 19 12 19C8.13 19 5 15.87 5 12C5 8.13 8.13 5 12 5M12 2C17.5 2 22 6.5 22 12C22 17.5 17.5 22 12 22C6.5 22 2 17.5 2 12C2 6.5 6.5 2 12 2M12 4C7.58 4 4 7.58 4 12C4 16.42 7.58 20 12 20C16.42 20 20 16.42 20 12C20 7.58 16.42 4 12 4Z";
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


        // コンストラクタ
        public RentLivingSummary(string rentid, string rentlivingid)
        {
            _rentId = rentid;
            _rentLivingId = rentlivingid;

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

                // 変更フラグ
                IsDirty = true;
            }
        }


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

                // 変更フラグ
                IsDirty = true;
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

                // 変更フラグ
                IsDirty = true;
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

                // 変更フラグ
                IsDirty = true;
            }
        }


        // 写真一覧
        public ObservableCollection<RentLivingPicture> RentLivingPictures { get; set; } = new ObservableCollection<RentLivingPicture>();

        // 写真のDBへの更新時にDBから削除されるべき物件写真のIDリスト
        public List<string> RentLivingPicturesToBeDeletedIDs = new List<string>();

        // 図面一覧
        public ObservableCollection<RentLivingPdf> RentLivingPdfs { get; set; } = new ObservableCollection<RentLivingPdf>();

        // 図面のDBへの更新時にDBから削除されるべき図面のIDリスト
        public List<string> RentLivingPdfsToBeDeletedIDs = new List<string>();

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
