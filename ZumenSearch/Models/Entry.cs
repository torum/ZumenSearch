using CommunityToolkit.Mvvm.ComponentModel;
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
using System.Xml;
using System.Xml.Linq;
using ZumenSearch.ViewModels;

namespace ZumenSearch.Models
{
    public abstract class Entry : ObservableObject
    {
        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (SetProperty(ref _isDirty, value))
                {
                    //
                }
            }
        }

        protected string _id;
        public string Id
        {
            get
            {
                return _id;
            }
        }

        public string SetId
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Id cannot be null or empty.", nameof(value));
                }

                if (SetProperty(ref _id, value))
                {
                    // 
                }
            }
        }



        /*
        #region == 所在地 ==

        // 所在地郵便番号
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

        // TODO
        // LocationHiddenPart

        // 緯度経度（Lon）
        private string _geoLocationLongitude;
        public string GeoLocationLongitude
        {
            get
            {
                return _geoLocationLongitude;
            }
            set
            {
                if (_geoLocationLongitude == value) return;

                _geoLocationLongitude = value;
                this.NotifyPropertyChanged(nameof(GeoLocationLongitude));

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 緯度経度（Lat）
        private string _geoLocationLatitude;
        public string GeoLocationLatitude
        {
            get
            {
                return _geoLocationLatitude;
            }
            set
            {
                if (_geoLocationLatitude == value) return;

                _geoLocationLatitude = value;
                this.NotifyPropertyChanged(nameof(GeoLocationLatitude));

                // 変更フラグ
                IsDirty = true;
            }
        }

        #endregion
        */
        /*
        #region == 交通 ==

        // 沿線１
        private string _trainLine1;
        public string TrainLine1
        {
            get
            {
                return _trainLine1;
            }
            set
            {
                if (_trainLine1 == value) return;

                _trainLine1 = value;
                this.NotifyPropertyChanged("TrainLine1");

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 沿線２
        private string _trainLine2;
        public string TrainLine2
        {
            get
            {
                return _trainLine2;
            }
            set
            {
                if (_trainLine2 == value) return;

                _trainLine2 = value;
                this.NotifyPropertyChanged("TrainLine2");

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 沿線３
        private string _trainLine3;
        public string TrainLine3
        {
            get
            {
                return _trainLine3;
            }
            set
            {
                if (_trainLine3 == value) return;

                _trainLine3 = value;
                this.NotifyPropertyChanged("TrainLine3");

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

        // 最寄り駅１から徒歩分
        private string _trainStation1WalkMinutes;
        public string TrainStation1WalkMinutes
        {
            get
            {
                return _trainStation1WalkMinutes;
            }
            set
            {
                if (_trainStation1WalkMinutes == value) return;

                _trainStation1WalkMinutes = value;
                this.NotifyPropertyChanged("TrainStation1WalkMinutes");

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 最寄り駅２から徒歩分
        private string _trainStation2WalkMinutes;
        public string TrainStation2WalkMinutes
        {
            get
            {
                return _trainStation2WalkMinutes;
            }
            set
            {
                if (_trainStation2WalkMinutes == value) return;

                _trainStation2WalkMinutes = value;
                this.NotifyPropertyChanged("TrainStation2WalkMinutes");

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 最寄り駅３から徒歩分
        private string _trainStation3WalkMinutes;
        public string TrainStation3WalkMinutes
        {
            get
            {
                return _trainStation3WalkMinutes;
            }
            set
            {
                if (_trainStation3WalkMinutes == value) return;

                _trainStation3WalkMinutes = value;
                this.NotifyPropertyChanged("TrainStation3WalkMinutes");

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 最寄り駅１からのバス停名
        private string _trainStation1BusStop;
        public string TrainStation1BusStop
        {
            get
            {
                return _trainStation1BusStop;
            }
            set
            {
                if (_trainStation1BusStop == value) return;

                _trainStation1BusStop = value;
                this.NotifyPropertyChanged("TrainStation1BusStop");

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 最寄り駅１からバス分
        private string _trainStation1BusMinutes;
        public string TrainStation1BusMinutes
        {
            get
            {
                return _trainStation1BusMinutes;
            }
            set
            {
                if (_trainStation1BusMinutes == value) return;

                _trainStation1BusMinutes = value;
                this.NotifyPropertyChanged("TrainStation1BusMinutes");

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 最寄り駅２からのバス停名
        private string _trainStation2BusStop;
        public string TrainStation2BusStop
        {
            get
            {
                return _trainStation2BusStop;
            }
            set
            {
                if (_trainStation2BusStop == value) return;

                _trainStation2BusStop = value;
                this.NotifyPropertyChanged("TrainStation2BusStop");

                // 変更フラグ
                IsDirty = true;
            }
        }

        // 最寄り駅２からバス分
        private string _trainStation2BusMinutes;
        public string TrainStation2BusMinutes
        {
            get
            {
                return _trainStation2BusMinutes;
            }
            set
            {
                if (_trainStation2BusMinutes == value) return;

                _trainStation2BusMinutes = value;
                this.NotifyPropertyChanged("TrainStation2BusMinutes");

                // 変更フラグ
                IsDirty = true;
            }
        }

        #endregion
        */
        /*
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
                this.NotifyPropertyChanged(nameof(Ownership));

                // 変更フラグ
                IsDirty = true;
            }
        }
        public Dictionary<string, Ownerships> StringToRentOwnership { get; } = new Dictionary<string, Ownerships>()
        {
            {"Unknown", Ownerships.Unknown},
            {"All", Ownerships.All},
            {"Unit", Ownerships.Unit},
        };


        // 建物管理形態
        public Dictionary<BuildingManagement, string> BuildingManagementToLabel { get; } = new Dictionary<BuildingManagement, string>()
        {
            {BuildingManagement.Unspecified, ""},
            {BuildingManagement.Jisya, "自社管理"},
            {BuildingManagement.Tasha, "他社管理"},
            {BuildingManagement.Kashinushi, "貸主管理"},
            {BuildingManagement.Unknown, "不明"},
        };

        // 建物構造
        public Dictionary<BuildingStructure, string> BuildingStructureToLabel { get; } = new Dictionary<BuildingStructure, string>()
        {
            {BuildingStructure.Unspecified, ""},
            {BuildingStructure.Wood, "木造"},
            {BuildingStructure.Block, "ブロック造"},
            {BuildingStructure.LightSteel, "軽量鉄骨造"},
            {BuildingStructure.Steel, "鉄骨造"},
            {BuildingStructure.RC, "鉄筋コンクリート(RC)造"},
            {BuildingStructure.SRC, "鉄骨鉄筋コンクリート(SRC)造"},
            {BuildingStructure.ALC, "ALC造"},
            {BuildingStructure.PC, "プレキャストコンクリート(PC)造"},
            {BuildingStructure.HPC, "鉄骨プレキャストコンクリート(HPC)造"},
            {BuildingStructure.RB, "鉄筋ブロック造"},
            {BuildingStructure.CFT, "コンクリート充填鋼管(CFT)造"},
            {BuildingStructure.Other, "その他"},
        };
        */
        /*
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
            }
        }

        #endregion
        */

        protected Entry()
        {
            // Instead of using a GUID, we initialize _id to an empty string to indicate that the entry is NEW.
            _id = string.Empty;//Guid.CreateVersion7() //Guid.NewGuid().ToString();
        }

        protected Entry(string id)
        {
            _id = id;
        }
    }


    /*
    // 物件（建物等）の検索結果表示用クラス
    public class RentLivingSearchResult : Rent
    {
        // Icon Path
        //private static string _rsNew = "M12 5C15.87 5 19 8.13 19 12C19 15.87 15.87 19 12 19C8.13 19 5 15.87 5 12C5 8.13 8.13 5 12 5M12 2C17.5 2 22 6.5 22 12C22 17.5 17.5 22 12 22C6.5 22 2 17.5 2 12C2 6.5 6.5 2 12 2M12 4C7.58 4 4 7.58 4 12C4 16.42 7.58 20 12 20C16.42 20 20 16.42 20 12C20 7.58 16.42 4 12 4Z";
        //private static string _rsNormal = "M12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
        //private static string _rsVisited = "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M12 20C7.59 20 4 16.41 4 12S7.59 4 12 4 20 7.59 20 12 16.41 20 12 20M16.59 7.58L10 14.17L7.41 11.59L6 13L10 17L18 9L16.59 7.58Z";

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
        public RentLivingSearchResult(string rentid, string rentlivingid)
        {
            _rentId = rentid;
            _rentLivingId = rentlivingid;

        }
    }
    */
}
