using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using RepsCore.Common;
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

/// ////////////////////////////////////////////////////////////
/// //////////////まずは小さく造って大きく育てる////////////////
/// ////////////////////////////////////////////////////////////

/// ■ TODO:
///
/// 建物や部屋の新規と編集の画面を共通化する。
/// 
/// 区分所有かどうかで、図面の追加画面を変える。（利便性のため、表示用の一覧はして、追加削除はしない読み取り専用にする）
/// 
/// 物件に、元付け・管理会社・オーナーの関連付け（元付けとオーナーは区分所有を考慮して、部屋で追加するが、建物として一覧はする）
/// 
/// 住所・〒・Geo関連のデータ
/// 
/// RL、RB、RP、各項目を整備（所在地を分けて、LastUpdateも）
/// 
/// 条件検索
/// 入力補助・入力チェック
/// 部屋等でIsDirtyをしっかりと
///  
/// エラー処理、及びログ保存
/// Modelsに基底クラス定義やデータ操作をリストラクチャーする。
/// 
/// 
/// XMLでインポート・エクスポート
/// RESTful API, Server, P2P and beyond...
/// （サーバー機能を追加して、同一ネットワーク内のローカルIPアドレスを指定してお互いに物件をフェッチし合えるようにする・・重複対策は・・・）
/// （不動産IDが出来たら、Webサーバーを作り、会社間共有）
/// （技術的に対応できるならば、P2Pで会社間共有）

/// ● 履歴：
/// 2020/10/30 金曜日: 元付け業者・管理会社・オーナーの追加、一覧、削除、編集。
/// 2020/10/29 木曜日: 図面PDFの追加、一覧、削除、表示。編集画面でのDBから削除洩れ修正。
/// 2020/10/28 水曜日: 部屋の写真の追加・削除。
/// 2020/10/27 火曜日: 部屋の追加と編集、削除と複製。
/// 2020/10/26 月曜日: 部屋の追加の画面遷移。
/// 2020/10/23 金曜日: エラーイベントとエラー表示機能。
/// 2020/10/20 火曜日: 建物の画像ファイルの追加処理。
/// 

/// ◆ 後で・検討中： 
/// WinUI 3.0が出たらツラを作り直す。
/// {あとで} PDFのデータは重いので、直近の一つを除いて？SELECTでBlobデータ自体は読み込まないようにする。（画像は？）
/// {あとで} 写真の「差し替え」機能
/// {レイアウト} 写真の追加をタブに切り分ける?
/// {レイアウト} 物件タブの中に部屋タブを入れる？
/// {あとで} 2020/10/27 火曜日: configやDBファイルのパスなどはconstでまとめて一か所に定義しておく。
/// {あとで} 画像データのビットマップ形式へ統一？


namespace RepsCore.ViewModels
{
    /// <summary>
    /// 物件の種別などの Enum （ViewのXAMLで参照するのでここで定義）
    /// </summary>
    #region == グローバル定数 ==

    // 賃貸物件のタイプ（住居用・事業用・駐車場）
    public enum RentTypes
    {
        RentLiving, RentBussiness, RentParking
    }

    // 賃貸住居用物件種別（アパート・マンション・一戸建て・他）
    public enum RentLivingKinds
    {
        Apartment, Mansion, House, Other
    }
    
    // 一括所有の建物か、区分所有か
    public enum RentOwnerships
    {
        All, Unit
    }

    #endregion

    /// <summary>
    /// 賃貸物件のRent・Section等、及び派生クラス
    /// </summary>
    #region == Rent・Section等、及びその派生クラス ==

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
        public RentLiving (string rentid, string rentlivingid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;

            this._type = RentTypes.RentLiving;
        }

    }

    /// <summary>
    /// 物件の写真の基底クラス
    /// </summary>
    public class RentPicture : ViewModelBase
    {
        protected string _rentPicture_id;
        public string RentPicture_ID
        {
            get
            {
                return _rentPicture_id;
            }
        }

        protected string _rent_id;
        public string Rent_ID
        {
            get
            {
                return _rent_id;
            }
        }

        // For display.
        private ImageSource _picture;
        public ImageSource Picture
        {
            get
            {
                return _picture;
            }
            set
            {
                if (_picture == value) return;

                _picture = value;
                this.NotifyPropertyChanged("Picture");
            }
        }

        private byte[] _pictureThumbW200xData;
        public byte[] PictureThumbW200xData
        {
            get
            {
                return _pictureThumbW200xData;
            }
            set
            {
                if (_pictureThumbW200xData == value) return;

                _pictureThumbW200xData = value;
                this.NotifyPropertyChanged("PictureThumbW200xData");
            }
        }

        private byte[] _pictureData;
        public byte[] PictureData
        {
            get
            {
                return _pictureData;
            }
            set
            {
                if (_pictureData == value) return;

                _pictureData = value;
                this.NotifyPropertyChanged("PictureData");
            }
        }

        private string _pictureFileExt;
        public string PictureFileExt
        {
            get
            {
                return _pictureFileExt;
            }
            set
            {
                if (_pictureFileExt == value) return;

                _pictureFileExt = value;
                this.NotifyPropertyChanged("PictureFileExt");
            }
        }

        // 新規追加された画像（要保存）
        public bool IsNew { get; set; }

        // 画像が差し替えなど、変更された（要保存）
        public bool IsModified { get; set; }

    }

    /// <summary>
    /// 賃貸住居用物件の写真クラス（外観等）
    /// </summary>
    public class RentLivingPicture : RentPicture
    {
        protected string _rentLiving_id;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_id;
            }
        }

        public RentLivingPicture(string rentid, string rentlivingid, string rentlivingpictureid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;

            this._rentPicture_id = rentlivingpictureid;
        }
    }


    /// <summary>
    /// 図面の基底クラス
    /// </summary>
    public class RentZumenPDF : ViewModelBase
    {
        protected string _rentZumenPDF_id;
        public string RentZumenPDF_ID
        {
            get
            {
                return _rentZumenPDF_id;
            }
        }

        protected string _rent_id;
        public string Rent_ID
        {
            get
            {
                return _rent_id;
            }
        }

        private byte[] _pdfData;
        public byte[] PDFData
        {
            get
            {
                return _pdfData;
            }
            set
            {
                if (_pdfData == value) return;

                _pdfData = value;
                this.NotifyPropertyChanged("PDFData");
            }
        }

        // 登録日
        protected DateTime _dateTimeAdded;
        public DateTime DateTimeAdded
        {
            get
            {
                return _dateTimeAdded;
            }
            set
            {
                if (_dateTimeAdded == value) return;

                _dateTimeAdded = value;
                this.NotifyPropertyChanged("DateTimeAdded");
            }
        }

        // 情報公開日
        private DateTime _dateTimePublished;
        public DateTime DateTimePublished
        {
            get
            {
                return _dateTimePublished;
            }
            set
            {
                if (_dateTimePublished == value) return;

                _dateTimePublished = value;
                this.NotifyPropertyChanged("DateTimePublished");

                this.IsDirty = true;
            }
        }

        // 最終確認日
        private DateTime _dateTimeVerified;
        public DateTime DateTimeVerified
        {
            get
            {
                return _dateTimeVerified;
            }
            set
            {
                if (_dateTimeVerified == value) return;

                _dateTimeVerified = value;
                this.NotifyPropertyChanged("DateTimeVerified");

                this.IsDirty = true;
            }
        }

        // ファイルサイズ
        public long _fileSize;
        public long FileSize
        {
            get
            {
                return _fileSize;
            }
            set
            {
                if (_fileSize == value) return;

                _fileSize = value;
                this.NotifyPropertyChanged("FileSize");
                this.NotifyPropertyChanged("FileSizeLabel");
            }
        }

        public string FileSizeLabel
        {
            get
            {
                if (FileSize > 0)
                {
                    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                    double len = FileSize;
                    int order = 0;
                    while (len >= 1024 && order < sizes.Length - 1)
                    {
                        order++;
                        len = len / 1024;
                    }

                    // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
                    // show a single decimal place, and no space.
                    return String.Format("{0:0.##} {1}", len, sizes[order]);
                }
                else
                {
                    return "";
                }


            }
        }

        // 新規追加なので、DBにINSERTが必要
        public bool IsNew { get; set; }

        // 日付などが変更された（DBのUPDATEが必要）
        public bool IsDirty { get; set; }
    }

    /// <summary>
    /// 賃貸住居用物件の図面クラス
    /// </summary>
    public class RentLivingZumenPDF : RentZumenPDF
    {
        protected string _rentLiving_id;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_id;
            }
        }

        public RentLivingZumenPDF(string rentid, string rentlivingid, string rentlivingzumenid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;

            this._rentZumenPDF_id = rentlivingzumenid;

            // 一応
            this._dateTimeAdded = DateTime.Now;
        }
    }


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

    /// <summary>
    /// 部屋・区画の写真基底クラス
    /// </summary>
    public class RentSectionPicture : ViewModelBase
    {
        protected string _rentSectionPicture_id;
        public string RentSectionPicture_ID
        {
            get
            {
                return _rentSectionPicture_id;
            }
        }

        protected string _rent_id;
        public string Rent_ID
        {
            get
            {
                return _rent_id;
            }
        }

        // For display.
        private ImageSource _picture;
        public ImageSource Picture
        {
            get
            {
                return _picture;
            }
            set
            {
                if (_picture == value) return;

                _picture = value;
                this.NotifyPropertyChanged("Picture");
            }
        }

        private byte[] _pictureThumbW200xData;
        public byte[] PictureThumbW200xData
        {
            get
            {
                return _pictureThumbW200xData;
            }
            set
            {
                if (_pictureThumbW200xData == value) return;

                _pictureThumbW200xData = value;
                this.NotifyPropertyChanged("PictureThumbW200xData");
            }
        }

        private byte[] _pictureData;
        public byte[] PictureData
        {
            get
            {
                return _pictureData;
            }
            set
            {
                if (_pictureData == value) return;

                _pictureData = value;
                this.NotifyPropertyChanged("PictureData");
            }
        }

        private string _pictureFileExt;
        public string PictureFileExt
        {
            get
            {
                return _pictureFileExt;
            }
            set
            {
                if (_pictureFileExt == value) return;

                _pictureFileExt = value;
                this.NotifyPropertyChanged("PictureFileExt");
            }
        }

        // 新規に追加されたので、まだDBに保存されていない。
        public bool IsNew { get; set; }

        // 保存されていてIDは固定だが、内容が変更されているのでUPDATEが必要。
        public bool IsModified { get; set; }

    }

    /// <summary>
    /// 賃貸住居用物件の写真クラス（室内・設備写真等）
    /// </summary>
    public class RentLivingSectionPicture : RentSectionPicture
    {
        protected string _rentLivingSection_id;
        public string RentLivingSection_ID
        {
            get
            {
                return _rentLivingSection_id;
            }
        }

        protected string _rentLiving_id;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_id;
            }
        }

        public RentLivingSectionPicture(string rentid, string rentlivingid, string rentlivingsectionid, string rentlivingsectionpictureid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;
            this._rentLivingSection_id = rentlivingsectionid;

            this._rentSectionPicture_id = rentlivingsectionpictureid;
        }
    }


    /// <summary>
    /// 元付け業者クラス
    /// </summary>
    public class Agency : ViewModelBase
    {
        // GUID and Primary Key
        protected string _agency_id;
        public string Agency_ID
        {
            get
            {
                return _agency_id;
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

        private string _branch;
        public string Branch
        {
            get
            {
                return _branch;
            }
            set
            {
                if (_branch == value) return;

                _branch = value;
                this.NotifyPropertyChanged("Branch");
            }
        }

        private string _telNumber;
        public string TelNumber
        {
            get
            {
                return _telNumber;
            }
            set
            {
                if (_telNumber == value) return;

                _telNumber = value;
                this.NotifyPropertyChanged("TelNumber");
            }
        }

        private string _faxNumber;
        public string FaxNumber
        {
            get
            {
                return _faxNumber;
            }
            set
            {
                if (_faxNumber == value) return;

                _faxNumber = value;
                this.NotifyPropertyChanged("FaxNumber");
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

        private string _address;
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                if (_address == value) return;

                _address = value;
                this.NotifyPropertyChanged("Address");
            }
        }

        private string _memo;
        public string Memo
        {
            get
            {
                return _memo;
            }
            set
            {
                if (_memo == value) return;

                _memo = value;
                this.NotifyPropertyChanged("Memo");
            }
        }

        public bool IsNew { get; set; }

        public bool IsDirty { get; set; }

        public Agency(string agencyid)
        {
            this._agency_id = agencyid;
        }
    }

    /// <summary>
    /// 管理会社クラス
    /// </summary>
    public class MaintenanceCompany : ViewModelBase
    {
        // GUID and Primary Key
        protected string _maintenanceCompany_id;
        public string MaintenanceCompany_ID
        {
            get
            {
                return _maintenanceCompany_id;
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

        private string _branch;
        public string Branch
        {
            get
            {
                return _branch;
            }
            set
            {
                if (_branch == value) return;

                _branch = value;
                this.NotifyPropertyChanged("Branch");
            }
        }

        private string _telNumber;
        public string TelNumber
        {
            get
            {
                return _telNumber;
            }
            set
            {
                if (_telNumber == value) return;

                _telNumber = value;
                this.NotifyPropertyChanged("TelNumber");
            }
        }

        private string _faxNumber;
        public string FaxNumber
        {
            get
            {
                return _faxNumber;
            }
            set
            {
                if (_faxNumber == value) return;

                _faxNumber = value;
                this.NotifyPropertyChanged("FaxNumber");
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

        private string _address;
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                if (_address == value) return;

                _address = value;
                this.NotifyPropertyChanged("Address");
            }
        }

        private string _memo;
        public string Memo
        {
            get
            {
                return _memo;
            }
            set
            {
                if (_memo == value) return;

                _memo = value;
                this.NotifyPropertyChanged("Memo");
            }
        }

        public bool IsNew { get; set; }

        public bool IsDirty { get; set; }

        public MaintenanceCompany(string maintenanceCompanyid)
        {
            this._maintenanceCompany_id = maintenanceCompanyid;
        }
    }

    /// <summary>
    /// オーナークラス
    /// </summary>
    public class Owner : ViewModelBase
    {
        // GUID and Primary Key
        protected string _owner_id;
        public string Owner_ID
        {
            get
            {
                return _owner_id;
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

        private string _telNumber;
        public string TelNumber
        {
            get
            {
                return _telNumber;
            }
            set
            {
                if (_telNumber == value) return;

                _telNumber = value;
                this.NotifyPropertyChanged("TelNumber");
            }
        }

        private string _faxNumber;
        public string FaxNumber
        {
            get
            {
                return _faxNumber;
            }
            set
            {
                if (_faxNumber == value) return;

                _faxNumber = value;
                this.NotifyPropertyChanged("FaxNumber");
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

        private string _address;
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                if (_address == value) return;

                _address = value;
                this.NotifyPropertyChanged("Address");
            }
        }

        private string _memo;
        public string Memo
        {
            get
            {
                return _memo;
            }
            set
            {
                if (_memo == value) return;

                _memo = value;
                this.NotifyPropertyChanged("Memo");
            }
        }
        public bool IsNew { get; set; }

        public bool IsDirty { get; set; }

        public Owner(string ownerid)
        {

            this._owner_id = ownerid;
        }
    }

    #endregion

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

    /// <summary>
    /// IO Dialog Service
    /// </summary>
    #region == IO Dialog Serviceダイアログ表示用クラス ==
    
    /// TODO: サービスのインジェクションは・・・とりあえずしない。
    /// https://stackoverflow.com/questions/28707039/trying-to-understand-using-a-service-to-open-a-dialog?noredirect=1&lq=1
    /*
    public interface IOpenDialogService
    {
        string[] GetOpenPictureFileDialog(string title, bool multi = true);
    }
    */

    public class OpenDialogService// : IOpenDialogService
    {

        public string[] GetOpenPictureFileDialog(string title, bool multi = true)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = multi;
            openFileDialog.Filter = "イメージファイル (*.jpg;*.png;*.gif;*.jpeg)|*.png;*.jpg;*.gif;*.jpeg|写真ファイル (*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg|画像ファイル(*.gif;*.png)|*.gif;*.png"; // 外観ならJPGかPNGのみ。間取りならGIFかPNG。
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures); // or MyDocuments
            openFileDialog.Title = title;

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileNames;
            }
            return null;
        }

        public string GetOpenZumenPdfFileDialog(string title)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "PDFファイル (*.pdf)|*.pdf"; 
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Title = title;

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }
    }

    #endregion

    /// <summary>
    /// メインのビューモデル
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        #region == 基本 ==

        // Application version.
        private const string _appVer = "0.0.0.1";

        // Application name.
        private const string _appName = "RepsCore";

        // Application config file folder name aka "publisher".
        private const string _appDeveloper = "torum";

        // Application Window Title.
        public string AppTitle
        {
            get
            {
                return _appName + " " + _appVer;
            }
        }

        #endregion

        #region == SQLite データベース ==

        // Sqlite DB file path.
        private readonly string _dataBaseFilePath;
        public string DataBaseFilePath
        {
            get { return _dataBaseFilePath; }
        }

        // SqliteConnectionStringBuilder.
        public SqliteConnectionStringBuilder connectionStringBuilder;

        #endregion

        #region == 物件関連オブジェクト ==

        // 賃貸住居用物件　管理　一覧
        public ObservableCollection<RentLiving> EditRents { get; } = new ObservableCollection<RentLiving>();

        // 賃貸住居用 管理　検索結果・一覧リストビューの選択されたオブジェクトを保持
        private RentLiving _rentLivingEditSelectedItem;
        public RentLiving RentLivingEditSelectedItem
        {
            get
            {
                return _rentLivingEditSelectedItem;
            }
            set
            {
                if (_rentLivingEditSelectedItem == value) return;

                _rentLivingEditSelectedItem = value;
                this.NotifyPropertyChanged("RentLivingEditSelectedItem");
            }
        }


        // 賃貸住居用 管理　新規物件追加の部屋一覧の選択されたSectionオブジェクトを保持
        private RentLivingSection _rentLivingNewSectionSelectedItem;
        public RentLivingSection RentLivingNewSectionSelectedItem
        {
            get
            {
                return _rentLivingNewSectionSelectedItem;
            }
            set
            {
                if (_rentLivingNewSectionSelectedItem == value) return;

                _rentLivingNewSectionSelectedItem = value;
                this.NotifyPropertyChanged("RentLivingNewSectionSelectedItem");
            }
        }

        // 賃貸住居用 管理　物件編集の部屋一覧の選択されたSectionオブジェクトを保持
        private RentLivingSection _rentLivingEditSectionSelectedItem;
        public RentLivingSection RentLivingEditSectionSelectedItem
        {
            get
            {
                return _rentLivingEditSectionSelectedItem;
            }
            set
            {
                if (_rentLivingEditSectionSelectedItem == value) return;

                _rentLivingEditSectionSelectedItem = value;
                this.NotifyPropertyChanged("RentLivingEditSectionSelectedItem");
            }
        }


        // 賃貸物件住居用　新規追加用のクラスオブジェクト
        private RentLiving _rentLivingNew = new RentLiving(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLiving RentLivingNew
        {
            get
            {
                return _rentLivingNew;
            }
            set
            {
                if (_rentLivingNew == value) return;

                _rentLivingNew = value;
                this.NotifyPropertyChanged("RentLivingNew");
            }
        }
        
        // 賃貸物件住居用　編集更新用のクラスオブジェクト
        private RentLiving _rentLivingEdit = new RentLiving(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLiving RentLivingEdit
        {
            get
            {
                return _rentLivingEdit;
            }
            set
            {
                if (_rentLivingEdit == value) return;

                _rentLivingEdit = value;
                this.NotifyPropertyChanged("RentLivingEdit");
            }
        }

        // 新規RLの新規部屋クラスオブジェクト
        private RentLivingSection _rentLivingNewSectionNew = new RentLivingSection(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLivingSection RentLivingNewSectionNew
        {
            get
            {
                return _rentLivingNewSectionNew;
            }
            set
            {
                if (_rentLivingNewSectionNew == value) return;

                _rentLivingNewSectionNew = value;
                this.NotifyPropertyChanged("RentLivingNewSectionNew");
            }
        }

        // 新規RLの編集部屋クラスオブジェクト
        private RentLivingSection _rentLivingNewSectionEdit = new RentLivingSection(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLivingSection RentLivingNewSectionEdit
        {
            get
            {
                return _rentLivingNewSectionEdit;
            }
            set
            {
                if (_rentLivingNewSectionEdit == value) return;

                _rentLivingNewSectionEdit = value;
                this.NotifyPropertyChanged("RentLivingNewSectionEdit");
            }
        }

        // 編集RLの新規部屋クラスオブジェクト
        private RentLivingSection _rentLivingEditSectionNew = new RentLivingSection(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLivingSection RentLivingEditSectionNew
        {
            get
            {
                return _rentLivingEditSectionNew;
            }
            set
            {
                if (_rentLivingEditSectionNew == value) return;

                _rentLivingEditSectionNew = value;
                this.NotifyPropertyChanged("RentLivingEditSectionNew");
            }
        }

        // 編集RLの編集部屋クラスオブジェクト
        private RentLivingSection _rentLivingEditSectionEdit = new RentLivingSection(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        public RentLivingSection RentLivingEditSectionEdit
        {
            get
            {
                return _rentLivingEditSectionEdit;
            }
            set
            {
                if (_rentLivingEditSectionEdit == value) return;

                _rentLivingEditSectionEdit = value;
                this.NotifyPropertyChanged("RentLivingEditSectionEdit");
            }
        }


        // 元付け業者　一覧
        public ObservableCollection<Agency> Agencies { get; } = new ObservableCollection<Agency>();

        // 元付け業者　一覧の選択されたオブジェクトを保持
        private Agency _agenciesSelectedItem;
        public Agency AgenciesSelectedItem
        {
            get
            {
                return _agenciesSelectedItem;
            }
            set
            {
                if (_agenciesSelectedItem == value) return;

                _agenciesSelectedItem = value;
                this.NotifyPropertyChanged("AgenciesSelectedItem");
            }
        }

        // 元付け業者　編集用のクラスオブジェクト
        private Agency _agencyEdit = new Agency(Guid.NewGuid().ToString());
        public Agency AgencyEdit
        {
            get
            {
                return _agencyEdit;
            }
            set
            {
                if (_agencyEdit == value) return;

                _agencyEdit = value;
                this.NotifyPropertyChanged("AgencyEdit");
            }
        }


        // 管理会社　一覧
        public ObservableCollection<MaintenanceCompany> MaintenanceCompanies { get; } = new ObservableCollection<MaintenanceCompany>();

        // 管理会社 一覧の選択されたオブジェクトを保持
        private MaintenanceCompany _maintenanceCompaniesSelectedItem;
        public MaintenanceCompany MaintenanceCompaniesSelectedItem
        {
            get
            {
                return _maintenanceCompaniesSelectedItem;
            }
            set
            {
                if (_maintenanceCompaniesSelectedItem == value) return;

                _maintenanceCompaniesSelectedItem = value;
                this.NotifyPropertyChanged("MaintenanceCompaniesSelectedItem");
            }
        }

        // 管理会社　編集用のクラスオブジェクト
        private MaintenanceCompany _maintenanceCompanyEdit = new MaintenanceCompany(Guid.NewGuid().ToString());
        public MaintenanceCompany MaintenanceCompanyEdit
        {
            get
            {
                return _maintenanceCompanyEdit;
            }
            set
            {
                if (_maintenanceCompanyEdit == value) return;

                _maintenanceCompanyEdit = value;
                this.NotifyPropertyChanged("MaintenanceCompanyEdit");
            }
        }


        // オーナー　管理　一覧
        public ObservableCollection<Owner> Owners { get; } = new ObservableCollection<Owner>();

        // オーナー 管理　一覧の選択されたオブジェクトを保持
        private Owner _ownersSelectedItem;
        public Owner OwnersSelectedItem
        {
            get
            {
                return _ownersSelectedItem;
            }
            set
            {
                if (_ownersSelectedItem == value) return;

                _ownersSelectedItem = value;
                this.NotifyPropertyChanged("OwnersSelectedItem");
            }
        }

        // オーナー　編集用のクラスオブジェクト
        private Owner _ownderEdit = new Owner(Guid.NewGuid().ToString());
        public Owner OwnerEdit
        {
            get
            {
                return _ownderEdit;
            }
            set
            {
                if (_ownderEdit == value) return;

                _ownderEdit = value;
                this.NotifyPropertyChanged("OwnerEdit");
            }
        }

        #endregion

        #region == 表示切替のフラグ ==

        // RL検索一覧・追加タブインデックス
        private int _showRentLivingTabActiveIndex = 0;
        public int ShowRentLivingTabActiveIndex
        {
            get
            {
                return _showRentLivingTabActiveIndex;
            }
            set
            {
                if (_showRentLivingTabActiveIndex == value) return;

                _showRentLivingTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowRentLivingTabActiveIndex");

            }
        }

        // RL検索一覧画面の表示フラグ
        private bool _showRentLivingSearchList = true;
        public bool ShowRentLivingSearchList
        {
            get
            {
                return _showRentLivingSearchList;
            }
            set
            {
                if (_showRentLivingSearchList == value) return;

                _showRentLivingSearchList = value;
                this.NotifyPropertyChanged("ShowRentLivingSearchList");

                if (_showRentLivingSearchList)
                {
                    ShowRentLivingTabActiveIndex = 0;

                    ShowRentLivingNew = false;
                    ShowRentLivingEdit = false;
                }

            }
        }
        
        // RL新規追加画面の表示フラグ
        private bool _showRentLivingNew = false;
        public bool ShowRentLivingNew
        {
            get
            {
                return _showRentLivingNew;
            }
            set
            {
                if (_showRentLivingNew == value) return;

                _showRentLivingNew = value;
                this.NotifyPropertyChanged("ShowRentLivingNew");

                ShowRentLivingSearchList = !_showRentLivingNew;

                if (_showRentLivingNew)
                {
                    ShowRentLivingTabActiveIndex = 1;

                    ShowRentLivingSearchList = false;
                    ShowRentLivingEdit = false;
                }

            }
        }

        // RL編集画面の表示フラグ
        private bool _showRentLivingEdit = false;
        public bool ShowRentLivingEdit
        {
            get
            {
                return _showRentLivingEdit;
            }
            set
            {
                if (_showRentLivingEdit == value) return;

                _showRentLivingEdit = value;
                this.NotifyPropertyChanged("ShowRentLivingEdit");

                ShowRentLivingSearchList = !_showRentLivingEdit;

                if (_showRentLivingEdit)
                {
                    ShowRentLivingTabActiveIndex = 2;

                    ShowRentLivingNew = false;
                }

            }
        }


        // RL新規物件の新規部屋タブインデックス
        private int _showRentLivingNewSectionTabActiveIndex = 0;
        public int ShowRentLivingNewSectionTabActiveIndex
        {
            get
            {
                return _showRentLivingNewSectionTabActiveIndex;
            }
            set
            {
                if (_showRentLivingNewSectionTabActiveIndex == value) return;

                _showRentLivingNewSectionTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowRentLivingNewSectionTabActiveIndex");

            }
        }

        // RL新規物件の部屋一覧画面の表示フラグ
        private bool _showRentLivingNewSectionList = true;
        public bool ShowRentLivingNewSectionList
        {
            get
            {
                return _showRentLivingNewSectionList;
            }
            set
            {
                if (_showRentLivingNewSectionList == value) return;

                _showRentLivingNewSectionList = value;
                this.NotifyPropertyChanged("ShowRentLivingNewSectionList");

                if (_showRentLivingNewSectionList)
                {
                    ShowRentLivingNewSectionTabActiveIndex = 0;
                    ShowRentLivingNewSectionNew = false;
                    ShowRentLivingNewSectionEdit = false;
                }

            }
        }

        // RL新規物件の部屋新規追加画面の表示フラグ
        private bool _showRentLivingNewSectionNew = false;
        public bool ShowRentLivingNewSectionNew
        {
            get
            {
                return _showRentLivingNewSectionNew;
            }
            set
            {
                if (_showRentLivingNewSectionNew == value) return;

                _showRentLivingNewSectionNew = value;
                this.NotifyPropertyChanged("ShowRentLivingNewSectionNew");

                if (_showRentLivingNewSectionNew)
                {
                    ShowRentLivingNewSectionTabActiveIndex = 1;
                    ShowRentLivingNewSectionList = false;
                    ShowRentLivingNewSectionEdit = false;
                }
                else
                {
                    ShowRentLivingNewSectionTabActiveIndex = 0;
                    ShowRentLivingNewSectionList = true;
                    ShowRentLivingNewSectionEdit = false;
                }

            }
        }

        // RL新規物件の部屋編集画面の表示フラグ
        private bool _showRentLivingNewSectionEdit = false;
        public bool ShowRentLivingNewSectionEdit
        {
            get
            {
                return _showRentLivingNewSectionEdit;
            }
            set
            {
                if (_showRentLivingNewSectionEdit == value) return;

                _showRentLivingNewSectionEdit = value;
                this.NotifyPropertyChanged("ShowRentLivingNewSectionEdit");

                if (_showRentLivingNewSectionEdit)
                {
                    ShowRentLivingNewSectionTabActiveIndex = 2;
                    ShowRentLivingNewSectionList = false;
                    ShowRentLivingNewSectionNew = false;
                }
                else
                {
                    ShowRentLivingNewSectionTabActiveIndex = 0;
                    ShowRentLivingNewSectionList = true;
                    ShowRentLivingNewSectionNew = false;
                }

            }
        }


        // RL編集物件の新規部屋タブインデックス
        private int _showRentLivingEditSectionTabActiveIndex = 0;
        public int ShowRentLivingEditSectionTabActiveIndex
        {
            get
            {
                return _showRentLivingEditSectionTabActiveIndex;
            }
            set
            {
                if (_showRentLivingEditSectionTabActiveIndex == value) return;

                _showRentLivingEditSectionTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowRentLivingEditSectionTabActiveIndex");

            }
        }

        // RL編集物件の部屋一覧画面の表示フラグ
        private bool _showRentLivingEditSectionList = true;
        public bool ShowRentLivingEditSectionList
        {
            get
            {
                return _showRentLivingEditSectionList;
            }
            set
            {
                if (_showRentLivingEditSectionList == value) return;

                _showRentLivingEditSectionList = value;
                this.NotifyPropertyChanged("ShowRentLivingEditSectionList");

                if (_showRentLivingEditSectionList)
                {
                    ShowRentLivingEditSectionTabActiveIndex = 0;
                    ShowRentLivingEditSectionNew = false;
                    ShowRentLivingEditSectionEdit = false;
                }

            }
        }

        // RL編集物件の部屋新規追加画面の表示フラグ
        private bool _showRentLivingEditSectionNew = false;
        public bool ShowRentLivingEditSectionNew
        {
            get
            {
                return _showRentLivingEditSectionNew;
            }
            set
            {
                if (_showRentLivingEditSectionNew == value) return;

                _showRentLivingEditSectionNew = value;
                this.NotifyPropertyChanged("ShowRentLivingEditSectionNew");

                if (_showRentLivingEditSectionNew)
                {
                    ShowRentLivingEditSectionTabActiveIndex = 1;
                    ShowRentLivingEditSectionList = false;
                    ShowRentLivingEditSectionEdit = false;
                }
                else
                {
                    ShowRentLivingEditSectionTabActiveIndex = 0;
                    ShowRentLivingEditSectionList = true;
                    ShowRentLivingEditSectionEdit = false;
                }
            }
        }

        // RL編集物件の部屋編集画面の表示フラグ
        private bool _showRentLivingEditSectionEdit = false;
        public bool ShowRentLivingEditSectionEdit
        {
            get
            {
                return _showRentLivingEditSectionEdit;
            }
            set
            {
                if (_showRentLivingEditSectionEdit == value) return;

                _showRentLivingEditSectionEdit = value;
                this.NotifyPropertyChanged("ShowRentLivingEditSectionEdit");

                if (_showRentLivingEditSectionEdit)
                {
                    ShowRentLivingEditSectionTabActiveIndex = 2;
                    ShowRentLivingEditSectionList = false;
                    ShowRentLivingEditSectionNew = false;
                }
                else
                {
                    ShowRentLivingEditSectionTabActiveIndex = 0;
                    ShowRentLivingEditSectionList = true;
                    ShowRentLivingEditSectionNew = false;
                }

            }
        }


        // 元付け業者Agency 検索一覧・追加・編集タブインデックス
        private int _showAgencyTabActiveIndex = 0;
        public int ShowAgencyTabActiveIndex
        {
            get
            {
                return _showAgencyTabActiveIndex;
            }
            set
            {
                if (_showAgencyTabActiveIndex == value) return;

                _showAgencyTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowAgencyTabActiveIndex");
            }
        }

        // 元付け業者Agency検索一覧画面の表示フラグ
        private bool _showAgencySearchList = true;
        public bool ShowAgencySearchList
        {
            get
            {
                return _showAgencySearchList;
            }
            set
            {
                if (_showAgencySearchList == value) return;

                _showAgencySearchList = value;
                this.NotifyPropertyChanged("ShowAgencySearchList");

                if (_showAgencySearchList)
                {
                    ShowAgencyTabActiveIndex = 0;

                    ShowAgencyEdit = false;
                }

            }

        }
        
        // 元付け業者Agency新規追加画面の表示フラグ
        private bool _showAgencyEdit = false;
        public bool ShowAgencyEdit
        {
            get
            {
                return _showAgencyEdit;
            }
            set
            {
                if (_showAgencyEdit == value) return;

                _showAgencyEdit = value;
                this.NotifyPropertyChanged("ShowAgencyEdit");

                ShowAgencySearchList = !_showAgencyEdit;

                if (_showAgencyEdit)
                {
                    ShowAgencyTabActiveIndex = 1;
                }

            }
        }


        // 管理会社 検索一覧・追加・編集タブインデックス
        private int _showMaintenanceCompanyTabActiveIndex = 0;
        public int ShowMaintenanceCompanyTabActiveIndex
        {
            get
            {
                return _showMaintenanceCompanyTabActiveIndex;
            }
            set
            {
                if (_showMaintenanceCompanyTabActiveIndex == value) return;

                _showMaintenanceCompanyTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowMaintenanceCompanyTabActiveIndex");
            }
        }

        // 管理会社 検索一覧画面の表示フラグ
        private bool _showMaintenanceCompanySearchList = true;
        public bool ShowMaintenanceCompanySearchList
        {
            get
            {
                return _showMaintenanceCompanySearchList;
            }
            set
            {
                if (_showMaintenanceCompanySearchList == value) return;

                _showMaintenanceCompanySearchList = value;
                this.NotifyPropertyChanged("ShowMaintenanceCompanySearchList");

                if (_showMaintenanceCompanySearchList)
                {
                    ShowMaintenanceCompanyTabActiveIndex = 0;

                    ShowMaintenanceCompanyEdit = false;
                }

            }

        }

        // 管理会社 新規追加画面の表示フラグ
        private bool _showMaintenanceCompanyEdit = false;
        public bool ShowMaintenanceCompanyEdit
        {
            get
            {
                return _showMaintenanceCompanyEdit;
            }
            set
            {
                if (_showMaintenanceCompanyEdit == value) return;

                _showMaintenanceCompanyEdit = value;
                this.NotifyPropertyChanged("ShowMaintenanceCompanyEdit");

                ShowMaintenanceCompanySearchList = !_showMaintenanceCompanyEdit;

                if (_showMaintenanceCompanyEdit)
                {
                    ShowMaintenanceCompanyTabActiveIndex = 1;
                }

            }
        }


        // オーナー 検索一覧・追加・編集タブインデックス
        private int _showOwnerTabActiveIndex = 0;
        public int ShowOwnerTabActiveIndex
        {
            get
            {
                return _showOwnerTabActiveIndex;
            }
            set
            {
                if (_showOwnerTabActiveIndex == value) return;

                _showOwnerTabActiveIndex = value;
                this.NotifyPropertyChanged("ShowOwnerTabActiveIndex");
            }
        }

        // オーナー 検索一覧画面の表示フラグ
        private bool _showOwnerSearchList = true;
        public bool ShowOwnerSearchList
        {
            get
            {
                return _showOwnerSearchList;
            }
            set
            {
                if (_showOwnerSearchList == value) return;

                _showOwnerSearchList = value;
                this.NotifyPropertyChanged("ShowOwnerSearchList");

                if (_showOwnerSearchList)
                {
                    ShowOwnerTabActiveIndex = 0;

                    ShowOwnerEdit = false;
                }

            }

        }

        // オーナー 新規追加画面の表示フラグ
        private bool _showOwnerEdit = false;
        public bool ShowOwnerEdit
        {
            get
            {
                return _showOwnerEdit;
            }
            set
            {
                if (_showOwnerEdit == value) return;

                _showOwnerEdit = value;
                this.NotifyPropertyChanged("ShowOwnerEdit");

                ShowOwnerSearchList = !_showOwnerEdit;

                if (_showOwnerEdit)
                {
                    ShowOwnerTabActiveIndex = 1;
                }

            }
        }


        // エラー通知表示フラグ
        private bool _showErrorDialog = false;
        public bool ShowErrorDialog
        {
            get
            {
                return _showErrorDialog;
            }
            set
            {
                if (_showErrorDialog == value) return;

                _showErrorDialog = value;
                this.NotifyPropertyChanged("ShowErrorDialog");
            }
        }

        #endregion

        #region == その他のプロパティ ==

        // 賃貸住居用編集画面の検索文字列
        private string _rentLivingEditSearchText;
        public string RentLivingEditSearchText
        {
            get
            {
                return _rentLivingEditSearchText;
            }
            set
            {
                if (_rentLivingEditSearchText == value) return;

                _rentLivingEditSearchText = value;
                this.NotifyPropertyChanged("RentLivingEditSearchText");
            }
        }

        // 業者検索 検索文字列
        private string _agencySearchText;
        public string AgencySearchText
        {
            get
            {
                return _agencySearchText;
            }
            set
            {
                if (_agencySearchText == value) return;

                _agencySearchText = value;
                this.NotifyPropertyChanged("AgencySearchText");
            }
        }

        // 管理会社 検索文字列
        private string _maintenanceCompanySearchText;
        public string MaintenanceCompanySearchText
        {
            get
            {
                return _maintenanceCompanySearchText;
            }
            set
            {
                if (_maintenanceCompanySearchText == value) return;

                _maintenanceCompanySearchText = value;
                this.NotifyPropertyChanged("MaintenanceCompanySearchText");
            }
        }

        // オーナー 検索文字列
        private string _ownerSearchText;
        public string OwnerSearchText
        {
            get
            {
                return _ownerSearchText;
            }
            set
            {
                if (_ownerSearchText == value) return;

                _ownerSearchText = value;
                this.NotifyPropertyChanged("OwnerSearchText");
            }
        }

        #endregion

        #region == エラー通知やログ関連 ==

        private StringBuilder _errorText = new StringBuilder();
        public string ErrorText
        {
            get
            {
                return _errorText.ToString();
            }
            set
            {
                _errorText.Insert(0, value + Environment.NewLine);
                this.NotifyPropertyChanged("ErrorText");
            }
        }

        // エラーのリスト TODO: ログ保存
        private ObservableCollection<MyError> _errors = new ObservableCollection<MyError>();
        public ObservableCollection<MyError> Errors
        {
            get { return this._errors; }
        }

        #endregion

        #region == ダイアログ（サービス） ==

        // サービスのインジェクションは・・・とりあえずしない。
        //private IOpenDialogService openDialogService;
        private OpenDialogService _openDialogService = new OpenDialogService();
        
        #endregion

        /// <summary>
        /// メインのビューモデルのコンストラクタ
        /// </summary>
        public MainViewModel()// (IOpenDialogService openDialogService)
        {
            //this._openDialogService = openDialogService;

            // エラーイベントにサブスクライブ
            ErrorOccured += new MyErrorEvent(OnError);

            #region == コマンドのイニシャライズ ==

            // RL 管理検索
            RentLivingEditSearchCommand = new RelayCommand(RentLivingEditSearchCommand_Execute, RentLivingEditSearchCommand_CanExecute);
            // RL 管理一覧
            RentLivingEditListCommand = new RelayCommand(RentLivingEditListCommand_Execute, RentLivingEditListCommand_CanExecute);
            // RL 管理一覧選択表示
            RentLivingEditSelectedViewCommand = new RelayCommand(RentLivingEditSelectedViewCommand_Execute, RentLivingEditSelectedViewCommand_CanExecute);
            // RL 管理一覧選択削除
            RentLivingEditSelectedDeleteCommand = new RelayCommand(RentLivingEditSelectedDeleteCommand_Execute, RentLivingEditSelectedDeleteCommand_CanExecute);


            // RL 管理新規物件
            RentLivingNewCommand = new RelayCommand(RentLivingNewCommand_Execute, RentLivingNewCommand_CanExecute);
            RentLivingNewAddCommand = new RelayCommand(RentLivingNewAddCommand_Execute, RentLivingNewAddCommand_CanExecute);
            RentLivingNewCancelCommand = new RelayCommand(RentLivingNewCancelCommand_Execute, RentLivingNewCancelCommand_CanExecute);

            // RL 管理新規画像追加と削除
            RentLivingNewPictureAddCommand = new RelayCommand(RentLivingNewPictureAddCommand_Execute, RentLivingNewPictureAddCommand_CanExecute);
            RentLivingNewPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingNewPictureDeleteCommand_Execute(param),
                param => RentLivingNewPictureDeleteCommand_CanExecute());

            // RL 管理新規　PDF追加と削除
            RentLivingNewZumenPdfAddCommand = new RelayCommand(RentLivingNewZumenPdfAddCommand_Execute, RentLivingNewZumenPdfAddCommand_CanExecute);
            RentLivingNewZumenPdfDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingNewZumenPdfDeleteCommand_Execute(param),
                param => RentLivingNewZumenPdfDeleteCommand_CanExecute());
            // 表示
            RentLivingNewZumenPdfShowCommand = new GenericRelayCommand<object>(
                param => RentLivingNewZumenPdfShowCommand_Execute(param),
                param => RentLivingNewZumenPdfShowCommand_CanExecute());
            // 
            RentLivingNewZumenPdfEnterCommand = new GenericRelayCommand<RentZumenPDF>(
                param => RentLivingNewZumenPdfEnterCommand_Execute(param),
                param => RentLivingNewZumenPdfEnterCommand_CanExecute());

            // RL 管理新規 部屋新規
            RentLivingNewSectionNewCommand = new RelayCommand(RentLivingNewSectionNewCommand_Execute, RentLivingNewSectionNewCommand_CanExecute);
            RentLivingNewSectionNewCancelCommand = new RelayCommand(RentLivingNewSectionNewCancelCommand_Execute, RentLivingNewSectionNewCancelCommand_CanExecute);
            RentLivingNewSectionAddCommand = new RelayCommand(RentLivingNewSectionAddCommand_Execute, RentLivingNewSectionAddCommand_CanExecute);
            // RL 管理新規 部屋編集
            RentLivingNewSectionEditCommand = new RelayCommand(RentLivingNewSectionEditCommand_Execute, RentLivingNewSectionEditCommand_CanExecute);
            RentLivingNewSectionEditCancelCommand = new RelayCommand(RentLivingNewSectionEditCancelCommand_Execute, RentLivingNewSectionEditCancelCommand_CanExecute);
            RentLivingNewSectionUpdateCommand = new RelayCommand(RentLivingNewSectionUpdateCommand_Execute, RentLivingNewSectionUpdateCommand_CanExecute);

            // RL 管理新規 部屋複製と削除
            RentLivingNewSectionDuplicateCommand = new RelayCommand(RentLivingNewSectionDuplicateCommand_Execute, RentLivingNewSectionDuplicateCommand_CanExecute);
            RentLivingNewSectionDeleteCommand = new RelayCommand(RentLivingNewSectionDeleteCommand_Execute, RentLivingNewSectionDeleteCommand_CanExecute);

            // RL 管理新規　新規部屋の画像追加と削除
            RentLivingNewSectionNewPictureAddCommand = new RelayCommand(RentLivingNewSectionNewPictureAddCommand_Execute, RentLivingNewSectionNewPictureAddCommand_CanExecute);
            RentLivingNewSectionNewPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingNewSectionNewPictureDeleteCommand_Execute(param),
                param => RentLivingNewSectionNewPictureDeleteCommand_CanExecute());
            // RL 管理新規　編集部屋の画像追加と削除
            RentLivingNewSectionEditPictureAddCommand = new RelayCommand(RentLivingNewSectionEditPictureAddCommand_Execute, RentLivingNewSectionEditPictureAddCommand_CanExecute);
            RentLivingNewSectionEditPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingNewSectionEditPictureDeleteCommand_Execute(param),
                param => RentLivingNewSectionEditPictureDeleteCommand_CanExecute());



            // RL 管理一覧選択編集
            RentLivingEditSelectedEditCommand = new RelayCommand(RentLivingEditSelectedEditCommand_Execute, RentLivingEditSelectedEditCommand_CanExecute);
            RentLivingEditSelectedEditUpdateCommand = new RelayCommand(RentLivingEditSelectedEditUpdateCommand_Execute, RentLivingEditSelectedEditUpdateCommand_CanExecute);
            RentLivingEditSelectedEditCancelCommand = new RelayCommand(RentLivingEditSelectedEditCancelCommand_Execute, RentLivingEditSelectedEditCancelCommand_CanExecute);

            // RL 管理編集 画像追加と削除、差し替え
            RentLivingEditPictureAddCommand = new RelayCommand(RentLivingEditPictureAddCommand_Execute, RentLivingEditPictureAddCommand_CanExecute);
            RentLivingEditPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditPictureDeleteCommand_Execute(param),
                param => RentLivingEditPictureDeleteCommand_CanExecute());
            RentLivingEditPictureChangeCommand = new GenericRelayCommand<object>(
                param => RentLivingEditPictureChangeCommand_Execute(param),
                param => RentLivingEditPictureChangeCommand_CanExecute());

            // RL 管理編集　PDF追加と削除
            RentLivingEditZumenPdfAddCommand = new RelayCommand(RentLivingEditZumenPdfAddCommand_Execute, RentLivingEditZumenPdfAddCommand_CanExecute);
            RentLivingEditZumenPdfDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditZumenPdfDeleteCommand_Execute(param),
                param => RentLivingEditZumenPdfDeleteCommand_CanExecute());
            // 表示
            RentLivingEditZumenPdfShowCommand = new GenericRelayCommand<object>(
                param => RentLivingEditZumenPdfShowCommand_Execute(param),
                param => RentLivingEditZumenPdfShowCommand_CanExecute());
            // 
            RentLivingEditZumenPdfEnterCommand = new GenericRelayCommand<RentZumenPDF>(
                param => RentLivingEditZumenPdfEnterCommand_Execute(param),
                param => RentLivingEditZumenPdfEnterCommand_CanExecute());
            

            // RL 管理編集 新規部屋
            RentLivingEditSectionNewCommand = new RelayCommand(RentLivingEditSectionNewCommand_Execute, RentLivingEditSectionNewCommand_CanExecute);
            RentLivingEditSectionNewCancelCommand = new RelayCommand(RentLivingEditSectionNewCancelCommand_Execute, RentLivingEditSectionNewCancelCommand_CanExecute);
            RentLivingEditSectionAddCommand = new RelayCommand(RentLivingEditSectionAddCommand_Execute, RentLivingEditSectionAddCommand_CanExecute);
            // RL 管理編集 部屋編集
            RentLivingEditSectionEditCommand = new RelayCommand(RentLivingEditSectionEditCommand_Execute, RentLivingEditSectionEditCommand_CanExecute);
            RentLivingEditSectionEditCancelCommand = new RelayCommand(RentLivingEditSectionEditCancelCommand_Execute, RentLivingEditSectionEditCancelCommand_CanExecute);
            RentLivingEditSectionUpdateCommand = new RelayCommand(RentLivingEditSectionUpdateCommand_Execute, RentLivingEditSectionUpdateCommand_CanExecute);

            // RL 管理編集 部屋複製と削除
            RentLivingEditSectionDuplicateCommand = new RelayCommand(RentLivingEditSectionDuplicateCommand_Execute, RentLivingEditSectionDuplicateCommand_CanExecute);
            RentLivingEditSectionDeleteCommand = new RelayCommand(RentLivingEditSectionDeleteCommand_Execute, RentLivingEditSectionDeleteCommand_CanExecute);

            // RL 管理編集　新規部屋の画像追加と削除
            RentLivingEditSectionNewPictureAddCommand = new RelayCommand(RentLivingEditSectionNewPictureAddCommand_Execute, RentLivingEditSectionNewPictureAddCommand_CanExecute);
            RentLivingEditSectionNewPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditSectionNewPictureDeleteCommand_Execute(param),
                param => RentLivingEditSectionNewPictureDeleteCommand_CanExecute());
            // RL 管理編集　編集部屋の画像追加と削除
            RentLivingEditSectionEditPictureAddCommand = new RelayCommand(RentLivingEditSectionEditPictureAddCommand_Execute, RentLivingEditSectionEditPictureAddCommand_CanExecute);
            RentLivingEditSectionEditPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditSectionEditPictureDeleteCommand_Execute(param),
                param => RentLivingEditSectionEditPictureDeleteCommand_CanExecute());


            // 元付け業者
            AgencySearchCommand = new RelayCommand(AgencySearchCommand_Execute, AgencySearchCommand_CanExecute);
            AgencyListCommand = new RelayCommand(AgencyListCommand_Execute, AgencyListCommand_CanExecute);
            AgencyNewCommand = new RelayCommand(AgencyNewCommand_Execute, AgencyNewCommand_CanExecute);
            AgencySelectedEditCommand = new RelayCommand(AgencySelectedEditCommand_Execute, AgencySelectedEditCommand_CanExecute);
            AgencySelectedDeleteCommand = new RelayCommand(AgencySelectedDeleteCommand_Execute, AgencySelectedDeleteCommand_CanExecute);
            AgencyNewOrEditCancelCommand = new RelayCommand(AgencyNewOrEditCancelCommand_Execute, AgencyNewOrEditCancelCommand_CanExecute);
            AgencyInsertOrUpdateCommand = new RelayCommand(AgencyInsertOrUpdateCommand_Execute, AgencyInsertOrUpdateCommand_CanExecute);

            // 管理会社
            MaintenanceCompanySearchCommand = new RelayCommand(MaintenanceCompanySearchCommand_Execute, MaintenanceCompanySearchCommand_CanExecute);
            MaintenanceCompanyListCommand = new RelayCommand(MaintenanceCompanyListCommand_Execute, MaintenanceCompanyListCommand_CanExecute);
            MaintenanceCompanyNewCommand = new RelayCommand(MaintenanceCompanyNewCommand_Execute, MaintenanceCompanyNewCommand_CanExecute);
            MaintenanceCompanySelectedEditCommand = new RelayCommand(MaintenanceCompanySelectedEditCommand_Execute, MaintenanceCompanySelectedEditCommand_CanExecute);
            MaintenanceCompanySelectedDeleteCommand = new RelayCommand(MaintenanceCompanySelectedDeleteCommand_Execute, MaintenanceCompanySelectedDeleteCommand_CanExecute);
            MaintenanceCompanyNewOrEditCancelCommand = new RelayCommand(MaintenanceCompanyNewOrEditCancelCommand_Execute, MaintenanceCompanyNewOrEditCancelCommand_CanExecute);
            MaintenanceCompanyInsertOrUpdateCommand = new RelayCommand(MaintenanceCompanyInsertOrUpdateCommand_Execute, MaintenanceCompanyInsertOrUpdateCommand_CanExecute);

            // オーナー
            OwnerSearchCommand = new RelayCommand(OwnerSearchCommand_Execute, OwnerSearchCommand_CanExecute);
            OwnerListCommand = new RelayCommand(OwnerListCommand_Execute, OwnerListCommand_CanExecute);
            OwnerNewCommand = new RelayCommand(OwnerNewCommand_Execute, OwnerNewCommand_CanExecute);
            OwnerSelectedEditCommand = new RelayCommand(OwnerSelectedEditCommand_Execute, OwnerSelectedEditCommand_CanExecute);
            OwnerSelectedDeleteCommand = new RelayCommand(OwnerSelectedDeleteCommand_Execute, OwnerSelectedDeleteCommand_CanExecute);
            OwnerNewOrEditCancelCommand = new RelayCommand(OwnerNewOrEditCancelCommand_Execute, OwnerNewOrEditCancelCommand_CanExecute);
            OwnerInsertOrUpdateCommand = new RelayCommand(OwnerInsertOrUpdateCommand_Execute, OwnerInsertOrUpdateCommand_CanExecute);

            // エラー通知画面を閉じる
            CloseErrorCommand = new RelayCommand(CloseErrorCommand_Execute, CloseErrorCommand_CanExecute);
            
            #endregion

            #region == SQLite DB のイニシャライズ ==

            // DB file path のセット
            _dataBaseFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + _appName + ".db";

            // Create a table if not exists.
            connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = DataBaseFilePath
            };

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();

                    using (var tableCmd = connection.CreateCommand())
                    {
                        // トランザクション開始
                        tableCmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            // メインの賃貸物件「インデックス」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Rent (" +
                                "Rent_ID TEXT NOT NULL PRIMARY KEY," +
                                "Name TEXT NOT NULL," +
                                "Type TEXT NOT NULL," +
                                "PostalCode TEXT," +
                                "Location TEXT," +
                                "LocationHiddenPart TEXT," +
                                "GeoLocationLatitude TEXT," +
                                "GeoLocationLongitude TEXT," +
                                "TrainStation1 TEXT," +
                                "TrainStation2 TEXT)";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件のテーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLiving (" +
                                "RentLiving_ID TEXT NOT NULL PRIMARY KEY," +
                                "Rent_ID TEXT NOT NULL," +
                                "Kind TEXT NOT NULL," +
                                "Floors INTEGER NOT NULL," +
                                "FloorsBasement INTEGER," +
                                "BuiltYear INTEGER NOT NULL," +
                                "UnitOwnership TEXT NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件の「写真」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingPicture (" +
                                "RentLivingPicture_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "PictureData BLOB NOT NULL," +
                                "PictureThumbW200xData BLOB NOT NULL," +
                                "PictureFileExt TEXT NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件の「図面」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingZumenPdf (" +
                                "RentLivingZumenPdf_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "PdfData BLOB NOT NULL," +
                                "DateTimeAdded TEXT NOT NULL," +
                                "DateTimePublished TEXT NOT NULL," +
                                "DateTimeVerified TEXT NOT NULL," +
                                "FileSize REAL NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件の「部屋」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingSection(" +
                                "RentLivingSection_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "RoomNumber TEXT," +
                                "Price INTEGER NOT NULL," +
                                "Madori TEXT NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件の「部屋の写真」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingSectionPicture (" +
                                "RentLivingSectionPicture_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLivingSection_ID TEXT NOT NULL," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "PictureData BLOB NOT NULL," +
                                "PictureThumbW200xData BLOB NOT NULL," +
                                "PictureFileExt TEXT NOT NULL," +
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)," +
                                "FOREIGN KEY (RentLivingSection_ID) REFERENCES RentLivingSection(RentLivingSection_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 元付け業者テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Agency (" +
                                "Agency_ID TEXT NOT NULL PRIMARY KEY," +
                                "Name TEXT NOT NULL," +
                                "Branch TEXT NOT NULL," +
                                "TelNumber TEXT NOT NULL," +
                                "FaxNumber TEXT NOT NULL," +
                                "PostalCode TEXT NOT NULL," +
                                "Address TEXT NOT NULL," +
                                "Memo TEXT NOT NULL" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // 管理会社テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS MaintenanceCompany (" +
                                "MaintenanceCompany_ID TEXT NOT NULL PRIMARY KEY," +
                                "Name TEXT NOT NULL," +
                                "Branch TEXT NOT NULL," +
                                "TelNumber TEXT NOT NULL," +
                                "FaxNumber TEXT NOT NULL," +
                                "PostalCode TEXT NOT NULL," +
                                "Address TEXT NOT NULL," +
                                "Memo TEXT NOT NULL" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // オーナーテーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Owner (" +
                                "Owner_ID TEXT NOT NULL PRIMARY KEY," +
                                "Name TEXT NOT NULL," +
                                "TelNumber TEXT NOT NULL," +
                                "FaxNumber TEXT NOT NULL," +
                                "PostalCode TEXT NOT NULL," +
                                "Address TEXT NOT NULL," +
                                "Memo TEXT NOT NULL" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // トランザクションコミット
                            tableCmd.Transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            // トランザクションのロールバック
                            tableCmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel()");

                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "データベースのイニシャライズ処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::MainViewModel()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        string err = e.InnerException.Message;
                        System.Diagnostics.Debug.WriteLine(err);
                    }
                }

            }

            #endregion

        }

        #region == イベント ==

        // 起動時の処理
        public void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // データ保存フォルダの取得
            var AppDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDataFolder = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            // 存在していなかったら作成
            System.IO.Directory.CreateDirectory(AppDataFolder);

            #region == アプリ設定のロード  ==

            // 設定ファイルのパス
            var AppConfigFilePath = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";

            // アプリ設定情報の読み込み
            if (File.Exists(AppConfigFilePath))
            {
                XDocument xdoc = XDocument.Load(AppConfigFilePath);

                #region == ウィンドウ関連 ==
                /*
                if (sender is Window)
                {
                    // Main Window element
                    var mainWindow = xdoc.Root.Element("MainWindow");
                    if (mainWindow != null)
                    {
                        var hoge = mainWindow.Attribute("top");
                        if (hoge != null)
                        {
                            (sender as Window).Top = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("left");
                        if (hoge != null)
                        {
                            (sender as Window).Left = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("height");
                        if (hoge != null)
                        {
                            (sender as Window).Height = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("width");
                        if (hoge != null)
                        {
                            (sender as Window).Width = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("state");
                        if (hoge != null)
                        {
                            if (hoge.Value == "Maximized")
                            {
                                (sender as Window).WindowState = WindowState.Maximized;
                            }
                            else if (hoge.Value == "Normal")
                            {
                                (sender as Window).WindowState = WindowState.Normal;
                            }
                            else if (hoge.Value == "Minimized")
                            {
                                (sender as Window).WindowState = WindowState.Normal;
                            }
                        }
                    }
                }
                */
                #endregion

            }
            else
            {
                // デフォ
            }

            #endregion
        }

        // 終了時の処理
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // データ保存フォルダの取得
            var AppDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDataFolder = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            // 存在していなかったら作成
            System.IO.Directory.CreateDirectory(AppDataFolder);

            #region == アプリ設定の保存 ==

            // 設定ファイルのパス
            var AppConfigFilePath = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";

            // 設定ファイル用のXMLオブジェクト
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            // Root Document Element
            XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
            doc.AppendChild(root);

            XmlAttribute attrs = doc.CreateAttribute("Version");
            attrs.Value = _appVer;
            root.SetAttributeNode(attrs);

            #region == ウィンドウ関連 ==
            /*
            if (sender is Window)
            {
                // Main Window element
                XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

                // Main Window attributes
                attrs = doc.CreateAttribute("height");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Height.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Height.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("width");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Width.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Width.ToString();

                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("top");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Top.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Top.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("left");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Left.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Left.ToString();
                }
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("state");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = "Maximized";
                }
                else if ((sender as Window).WindowState == WindowState.Normal)
                {
                    attrs.Value = "Normal";

                }
                else if ((sender as Window).WindowState == WindowState.Minimized)
                {
                    attrs.Value = "Minimized";
                }
                mainWindow.SetAttributeNode(attrs);



                // set Main Window element to root.
                root.AppendChild(mainWindow);

            }
            */
            #endregion

            try
            {
                // 設定ファイルの保存
                doc.Save(AppConfigFilePath);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ Error  設定ファイルの保存中: " + ex + " while opening : " + AppConfigFilePath);
            }
            #endregion

        }

        #region == エラーイベント ==

        // デリゲート
        public delegate void MyErrorEvent(MyError err);

        // エラーイベント
        public event MyErrorEvent ErrorOccured;

        // エラーイベントの実装
        private void OnError(MyError err)
        {
            if (err == null) { return; }

            if (Application.Current == null) { return; }
            Application.Current.Dispatcher.Invoke(() =>
            {
                // リストに追加。TODO：あとあとログ保存等
                _errors.Insert(0, err);

                ErrorText = String.Format("エラー：{3}、エラー内容 {4}、 タイプ {1}、発生箇所 {2}、発生時刻 {0}", err.ErrDatetime.ToString(), err.ErrType, err.ErrPlace, err.ErrText, err.ErrDescription);

            });

            // エラーの表示
            ShowErrorDialog = true;
        }

        #endregion

        #endregion

        #region == メソッド ==

        // バイト配列をImageオブジェクトに変換
        public System.Drawing.Image ByteArrayToImage(byte[] b)
        {
            ImageConverter imgconv = new ImageConverter();
            System.Drawing.Image img = (System.Drawing.Image)imgconv.ConvertFrom(b);
            return img;
        }

        // Imageオブジェクトをバイト配列に変換
        public byte[] ImageToByteArray(System.Drawing.Image img)
        {
            ImageConverter imgconv = new ImageConverter();
            byte[] b = (byte[])imgconv.ConvertTo(img, typeof(byte[]));
            return b;
        }

        // バイト配列をBitmapImageオブジェクトに変換（Imageに表示するSource）
        public BitmapImage BitmapImageFromBytes(Byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                BitmapImage bmimage = new BitmapImage();
                bmimage.BeginInit();
                bmimage.CacheOption = BitmapCacheOption.OnLoad;
                bmimage.StreamSource = stream;
                bmimage.EndInit();
                return bmimage;
            }
        }

        // System.Drawing.Image をBitmapImageオブジェクトに変換
        public BitmapImage BitmapImageFromImage(System.Drawing.Image img, ImageFormat imf)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, imf);
                ms.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        // 画像ファイルの拡張子からImageFormat形式を返す。
        public ImageFormat FileExtToImageFormat(string fileext)
        {
            switch (fileext.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".gif":
                    return ImageFormat.Gif;
                default:
                    throw new Exception(String.Format("取り扱わない画像ファイルフォーマット: {0}", fileext));
            }

        }

        // サムネイル画像を縦横比を保ったまま、かつパディング付きで作成する。
        public System.Drawing.Image FixedSize(System.Drawing.Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            // 背景を白でベタ塗。
            grPhoto.Clear(System.Drawing.Color.White);
            grPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        #endregion

        #region == コマンドの宣言と実装 ==

        #region == RL 物件管理 ==

        // RL　物件管理、検索
        public ICommand RentLivingEditSearchCommand { get; }
        public bool RentLivingEditSearchCommand_CanExecute()
        {
            if (String.IsNullOrEmpty(RentLivingEditSearchText))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void RentLivingEditSearchCommand_Execute()
        {
            // Firest, clear it.
            EditRents.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Rent INNER JOIN RentLiving ON Rent.Rent_ID = RentLiving.Rent_ID WHERE Name Like '%" + RentLivingEditSearchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            RentLiving rl = new RentLiving(Convert.ToString(reader["Rent_ID"]), Convert.ToString(reader["RentLiving_ID"]));
                            //rl.Rent_ID = Convert.ToString(reader["Rent_ID"]);
                            rl.Name = Convert.ToString(reader["Name"]);
                            //rl.Type = rl.StringToRentType[Convert.ToString(reader["Type"])];
                            rl.PostalCode = Convert.ToString(reader["PostalCode"]);
                            rl.Location = Convert.ToString(reader["Location"]);
                            rl.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                            rl.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                            EditRents.Add(rl);

                        }
                    }
                }
            }
        }

        // RL　物件管理、一覧
        public ICommand RentLivingEditListCommand { get; }
        public bool RentLivingEditListCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditListCommand_Execute()
        {
            // Firest, clear it.
            EditRents.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM Rent INNER JOIN RentLiving ON Rent.Rent_ID = RentLiving.Rent_ID";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                RentLiving rl = new RentLiving(Convert.ToString(reader["Rent_ID"]), Convert.ToString(reader["RentLiving_ID"]));
                                //rl.Rent_ID = Convert.ToString(reader["Rent_ID"]);
                                rl.Name = Convert.ToString(reader["Name"]);
                                //rl.Type = rl.StringToRentType[Convert.ToString(reader["Type"])];
                                rl.PostalCode = Convert.ToString(reader["PostalCode"]);
                                rl.Location = Convert.ToString(reader["Location"]);
                                rl.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                                rl.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                                EditRents.Add(rl);

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errmessage;
                if (e.InnerException != null)
                {
                    errmessage = e.InnerException.Message;
                    System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::RentLivingEditListCommand_Execute()");
                }
                else
                {
                    errmessage = e.Message;
                    System.Diagnostics.Debug.WriteLine("Exception:'" + e.Message + "' @MainViewModel::RentLivingEditListCommand_Execute()");
                }

                // エラーイベント発火
                MyError er = new MyError();
                er.ErrType = "DB";
                er.ErrCode = 0;
                er.ErrText = "「" + errmessage + "」";
                er.ErrDescription = "賃貸住居用　物件管理、一覧（SELECT）する処理でエラーが発生しました。";
                er.ErrDatetime = DateTime.Now;
                er.ErrPlace = "In " + e.Source + " from MainViewModel::RentLivingEditListCommand_Execute()";
                ErrorOccured?.Invoke(er);
            }


        }

        // RL　物件管理、一覧選択アイテム表示(PDFとか)
        public ICommand RentLivingEditSelectedViewCommand { get; }
        public bool RentLivingEditSelectedViewCommand_CanExecute()
        {
            if (RentLivingEditSelectedItem == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void RentLivingEditSelectedViewCommand_Execute()
        {
            //RentLivingEditSelectedItem
            System.Diagnostics.Debug.WriteLine("RentLivingEditSelectedViewCommand_Execute");

            // TODO view
        }

        // RL　物件管理、一覧選択アイテム削除（DELETE）
        public ICommand RentLivingEditSelectedDeleteCommand { get; }
        public bool RentLivingEditSelectedDeleteCommand_CanExecute()
        {
            if (RentLivingEditSelectedItem == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void RentLivingEditSelectedDeleteCommand_Execute()
        {
            if (RentLivingEditSelectedItem != null)
            {
                // 選択アイテムのデータを削除

                string sqlRentTable = String.Format("DELETE FROM Rent WHERE Rent_ID = '{0}'", RentLivingEditSelectedItem.Rent_ID);

                string sqlRentLivingTable = String.Format("DELETE FROM RentLiving WHERE Rent_ID = '{0}'", RentLivingEditSelectedItem.Rent_ID);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sqlRentLivingTable;
                            var result = cmd.ExecuteNonQuery();


                            cmd.CommandText = sqlRentTable;
                            result = cmd.ExecuteNonQuery();

                            // TODO その他の外部キー依存しているテーブルからも削除


                            cmd.Transaction.Commit();

                            // 一覧から削除
                            if (EditRents.Remove(RentLivingEditSelectedItem))
                            {
                                RentLivingEditSelectedItem = null;
                            }

                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @RentLivingEditSelectedDeleteCommand_Execute()");

                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "賃貸住居用物件の選択アイテム削除（DELETE）で、データベースを更新する処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingEditSelectedDeleteCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        #endregion

        #region == RL 新規物件 ==

        // RL新規　物件追加（画面表示）
        public ICommand RentLivingNewCommand { get; }
        public bool RentLivingNewCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewCommand_Execute()
        {
            // RentLivingNewオブジェクトを用意
            RentLivingNew = new RentLiving(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            if (!ShowRentLivingNew) ShowRentLivingNew = true;
        }

        // RL新規　物件追加キャンセル
        public ICommand RentLivingNewCancelCommand { get; }
        public bool RentLivingNewCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewCancelCommand_Execute()
        {
            // 編集を非表示に（閉じる）
            if (ShowRentLivingNew) ShowRentLivingNew = false;
        }

        // RL新規　物件追加処理(INSERT)
        public ICommand RentLivingNewAddCommand { get; }
        public bool RentLivingNewAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewAddCommand_Execute()
        {
            if (RentLivingNew == null) return;

            // TODO: 入力チェック

            try
            {
                string sqlInsertIntoRent = String.Format("INSERT INTO Rent (Rent_ID, Name, Type, PostalCode, Location, TrainStation1, TrainStation2) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                    RentLivingNew.Rent_ID, RentLivingNew.Name, RentLivingNew.Type.ToString(), RentLivingNew.PostalCode, RentLivingNew.Location, RentLivingNew.TrainStation1, RentLivingNew.TrainStation2);

                string sqlInsertIntoRentLiving = String.Format("INSERT INTO RentLiving (RentLiving_ID, Rent_ID, Kind, Floors, FloorsBasement, BuiltYear) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                    RentLivingNew.RentLiving_ID, RentLivingNew.Rent_ID, RentLivingNew.Kind.ToString(), RentLivingNew.Floors, RentLivingNew.FloorsBasement, RentLivingNew.BuiltYear);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            // Rentテーブルへ追加
                            cmd.CommandText = sqlInsertIntoRent;
                            var InsertIntoRentResult = cmd.ExecuteNonQuery();
                            if (InsertIntoRentResult != 1)
                            {
                                // これいる?
                            }

                            // RentLivingテーブルへ追加
                            cmd.CommandText = sqlInsertIntoRentLiving;
                            var InsertIntoRentLivingResult = cmd.ExecuteNonQuery();
                            if (InsertIntoRentLivingResult != 1)
                            {
                                // これいる?
                            }

                            // 写真追加
                            if (RentLivingNew.RentLivingPictures.Count > 0)
                            {
                                foreach (var pic in RentLivingNew.RentLivingPictures)
                                {
                                    // 新規なので全てIsNewのはずだけど・・・
                                    if (pic.IsNew)
                                    {
                                        string sqlInsertIntoRentLivingPicture = String.Format("INSERT INTO RentLivingPicture (RentLivingPicture_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbW200xData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', @0, @1, '{5}')",
                                            pic.RentPicture_ID, RentLivingNew.RentLiving_ID, RentLivingNew.Rent_ID, pic.PictureData, pic.PictureThumbW200xData, pic.PictureFileExt);
                                        
                                        // 物件画像の追加
                                        cmd.CommandText = sqlInsertIntoRentLivingPicture;
                                        // ループなので、前のパラメーターをクリアする。
                                        cmd.Parameters.Clear();

                                        SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                        parameter1.Value = pic.PictureData;
                                        cmd.Parameters.Add(parameter1);

                                        SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                        parameter2.Value = pic.PictureThumbW200xData;
                                        cmd.Parameters.Add(parameter2);

                                        var result = cmd.ExecuteNonQuery();
                                        if (result > 0)
                                        {
                                            pic.IsNew = false;
                                            pic.IsModified = false;
                                        }
                                    }
                                }
                            }

                            if (RentLivingNew.RentLivingZumenPDFs.Count > 0)
                            {
                                foreach (var pdf in RentLivingNew.RentLivingZumenPDFs)
                                {
                                    string sqlInsertIntoRentLivingZumenPdf = String.Format("INSERT INTO RentLivingZumenPdf (RentLivingZumenPdf_ID, RentLiving_ID, Rent_ID, PdfData, DateTimeAdded, DateTimePublished, DateTimeVerified, FileSize) VALUES ('{0}', '{1}', '{2}', @0, '{4}', '{5}', '{6}', '{7}')",
                                        pdf.RentZumenPDF_ID, RentLivingNew.RentLiving_ID, RentLivingNew.Rent_ID, pdf.PDFData, pdf.DateTimeAdded.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

                                    // 図面の追加
                                    cmd.CommandText = sqlInsertIntoRentLivingZumenPdf;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pdf.PDFData;
                                    cmd.Parameters.Add(parameter1);

                                    var result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pdf.IsNew = false;
                                        //pic.IsModified = false;
                                    }
                                }
                            }

                            // 部屋追加
                            if (RentLivingNew.RentLivingSections.Count > 0)
                            {
                                foreach (var room in RentLivingNew.RentLivingSections)
                                {
                                    string sqlInsertIntoRentLivingSection = String.Format("INSERT INTO RentLivingSection (RentLivingSection_ID, RentLiving_ID, Rent_ID, RoomNumber, Price, Madori) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                                            room.RentLivingSection_ID, RentLivingNew.RentLiving_ID, RentLivingNew.Rent_ID, room.RentLivingSectionRoomNumber, room.RentLivingSectionPrice, room.RentLivingSectionMadori);

                                    cmd.CommandText = sqlInsertIntoRentLivingSection;
                                    var InsertIntoRentLivingSectionResult = cmd.ExecuteNonQuery();
                                    if (InsertIntoRentLivingSectionResult > 0)
                                    {
                                        room.IsNew = false;
                                        room.IsDirty = false;
                                    }

                                    // 部屋の写真
                                    if (room.RentLivingSectionPictures.Count > 0)
                                    {
                                        foreach (var roompic in room.RentLivingSectionPictures)
                                        {
                                            string sqlInsertIntoRentLivingSectionPic = String.Format("INSERT INTO RentLivingSectionPicture (RentLivingSectionPicture_ID, RentLivingSection_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbW200xData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', '{3}', @0, @1, '{6}')",
                                                roompic.RentSectionPicture_ID, roompic.RentLivingSection_ID, RentLivingNew.RentLiving_ID, RentLivingNew.Rent_ID, roompic.PictureData, roompic.PictureThumbW200xData, roompic.PictureFileExt);

                                            cmd.CommandText = sqlInsertIntoRentLivingSectionPic;
                                            // ループなので、前のパラメーターをクリアする。
                                            cmd.Parameters.Clear();

                                            SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                            parameter1.Value = roompic.PictureData;
                                            cmd.Parameters.Add(parameter1);

                                            SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                            parameter2.Value = roompic.PictureThumbW200xData;
                                            cmd.Parameters.Add(parameter2);

                                            var InsertIntoRentLivingSectionPicResult = cmd.ExecuteNonQuery();
                                            if (InsertIntoRentLivingSectionPicResult > 0)
                                            {
                                                roompic.IsNew = false;
                                                roompic.IsModified = false;
                                            }
                                        }
                                    }

                                }
                            }

                            //　コミット
                            cmd.Transaction.Commit();

                            // 追加画面を非表示に（閉じる）
                            if (ShowRentLivingNew) ShowRentLivingNew = false;
                        }
                        catch (Exception e)
                        {
                            // ロールバック
                            cmd.Transaction.Rollback();
                          
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "賃貸住居用物件の新規追加 (INSERT)で、データベースに追加する処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingNewAddCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                System.Diagnostics.Debug.WriteLine("Opps. TargetInvocationException@MainViewModel::RentLivingNewAddCommand_Execute()");
                throw ex.InnerException;
            }
            catch (System.InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine("Opps. InvalidOperationException@MainViewModel::RentLivingNewAddCommand_Execute()");
                throw ex.InnerException;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::RentLivingNewAddCommand_Execute()");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::RentLivingNewAddCommand_Execute()");
                }
            }
        }


        // RL新規　物件の画像追加
        public ICommand RentLivingNewPictureAddCommand { get; }
        public bool RentLivingNewPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewPictureAddCommand_Execute()
        {
            if (RentLivingNew == null) return;

            var files = _openDialogService.GetOpenPictureFileDialog("物件写真の追加");

            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 画像データの読み込み
                            byte[] ImageData;
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            //BinaryReader br = new BinaryReader(fs);
                            //ImageData = br.ReadBytes((int)fs.Length);
                            //br.Close();
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                            ImageData = ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = FixedSize(img, 200, 150);
                            byte[] ImageThumbData = ImageToByteArray(thumbImg);


                            RentLivingPicture rlpic = new RentLivingPicture(RentLivingNew.Rent_ID, RentLivingNew.RentLiving_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;
                            rlpic.IsModified = false;
                            rlpic.IsNew = true;

                            rlpic.Picture = BitmapImageFromImage(thumbImg, FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingNew.RentLivingPictures.Add(rlpic);


                            fs.Close();
                        }
                        else
                        {
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "File";
                            er.ErrCode = 0;
                            er.ErrText = "「" + "File Does Not Exist" + "」";
                            er.ErrDescription = fileName + " ファイルが存在しません。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingNewAddCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // RL新規　物件の画像削除
        public ICommand RentLivingNewPictureDeleteCommand { get; }
        public bool RentLivingNewPictureDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewPictureDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingNew == null) return;

            // 選択アイテム保持用
            List<RentLivingPicture> selectedList = new List<RentLivingPicture>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingPicture);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                RentLivingNew.RentLivingPictures.Remove(item);
            }

        }


        // RL新規　部屋追加（画面表示）
        public ICommand RentLivingNewSectionNewCommand { get; }
        public bool RentLivingNewSectionNewCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionNewCommand_Execute()
        {
            if (RentLivingNew == null) return;

            // RentLivingNewSectionNew オブジェクトを用意
            RentLivingNewSectionNew = new RentLivingSection(RentLivingNew.Rent_ID, RentLivingNew.RentLiving_ID, Guid.NewGuid().ToString());
            RentLivingNewSectionNew.IsNew = true;
            RentLivingNewSectionNew.IsDirty = false;

            if (!ShowRentLivingNewSectionNew) ShowRentLivingNewSectionNew = true;
        }
        
        // RL新規　部屋追加キャンセル
        public ICommand RentLivingNewSectionNewCancelCommand { get; }
        public bool RentLivingNewSectionNewCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionNewCancelCommand_Execute()
        {
            if (ShowRentLivingNewSectionNew) ShowRentLivingNewSectionNew = false;
        }

        // RL新規　部屋追加処理 (ADD to Collection)
        public ICommand RentLivingNewSectionAddCommand { get; }
        public bool RentLivingNewSectionAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionAddCommand_Execute()
        {
            if (RentLivingNew == null) return;
            if (RentLivingNewSectionNew == null) return;

            // TODO: 入力チェック

            // 物件オブジェクトの部屋コレクションに追加
            RentLivingNew.RentLivingSections.Add(RentLivingNewSectionNew);

            // 追加画面を閉じる
            ShowRentLivingNewSectionNew = false;
        }
        
        // RL新規　部屋編集（画面表示）
        public ICommand RentLivingNewSectionEditCommand { get; }
        public bool RentLivingNewSectionEditCommand_CanExecute()
        {
            if (RentLivingNewSectionSelectedItem != null)
                return true;
            else 
                return false;
        }
        public void RentLivingNewSectionEditCommand_Execute()
        {
            if (RentLivingNew == null) return;
            if (RentLivingNewSectionSelectedItem == null) return;

            // 
            RentLivingNewSectionEdit = RentLivingNewSectionSelectedItem;
            /*
            RentLivingNewSectionEdit = new RentLivingSection(RentLivingNewSectionSelectedItem.Rent_ID, RentLivingNewSectionSelectedItem.RentLiving_ID, RentLivingNewSectionSelectedItem.RentLivingSection_ID);

            RentLivingNewSectionEdit.IsNew = false;
            RentLivingNewSectionEdit.IsDirty = false;

            RentLivingNewSectionEdit.RentLivingSectionRoomNumber = RentLivingNewSectionSelectedItem.RentLivingSectionRoomNumber;
            RentLivingNewSectionEdit.RentLivingSectionMadori = RentLivingNewSectionSelectedItem.RentLivingSectionMadori;
            RentLivingNewSectionEdit.RentLivingSectionPrice = RentLivingNewSectionSelectedItem.RentLivingSectionPrice;
            // TODO: more to come

            // TODO: これは・・・
            foreach (var hoge in RentLivingNewSectionSelectedItem.RentLivingSectionPictures)
            {
                RentLivingNewSectionEdit.RentLivingSectionPictures.Add(hoge);
            }
            */

            if (!ShowRentLivingNewSectionEdit) ShowRentLivingNewSectionEdit = true;
        }

        // RL新規　部屋編集キャンセル
        public ICommand RentLivingNewSectionEditCancelCommand { get; }
        public bool RentLivingNewSectionEditCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionEditCancelCommand_Execute()
        {
            if (ShowRentLivingNewSectionEdit) ShowRentLivingNewSectionEdit = false;
        }

        // RL新規　部屋更新 (Update collection)
        public ICommand RentLivingNewSectionUpdateCommand { get; }
        public bool RentLivingNewSectionUpdateCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionUpdateCommand_Execute()
        {
            if (RentLivingNew == null) return;
            if (RentLivingNewSectionSelectedItem == null) return;

            // TODO: 入力チェック

            /*
            var found = RentLivingNew.RentLivingSections.FirstOrDefault(x => x.RentLivingSection_ID == RentLivingNewSectionEdit.RentLivingSection_ID);
            if (found != null)
            {
                found.RentLivingSectionRoomNumber = RentLivingNewSectionEdit.RentLivingSectionRoomNumber;
                found.RentLivingSectionMadori = RentLivingNewSectionEdit.RentLivingSectionMadori;
                found.RentLivingSectionPrice = RentLivingNewSectionEdit.RentLivingSectionPrice;
                // TODO: more to come

                // 一旦クリアして追加しなおさないと、変更が通知（更新）されない
                found.RentLivingSectionPictures.Clear();
                foreach (var hoge in RentLivingNewSectionEdit.RentLivingSectionPictures)
                {
                    found.RentLivingSectionPictures.Add(hoge);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("THIS SHOULD NOT BE HAPPENING @RentLivingNewSectionUpdateCommand_Execute");
            }
            */

            // 部屋編集画面を閉じる
            ShowRentLivingNewSectionEdit = false;
        }

        // RL新規　部屋一覧の選択を複製
        public ICommand RentLivingNewSectionDuplicateCommand { get; }
        public bool RentLivingNewSectionDuplicateCommand_CanExecute()
        {
            if (RentLivingNewSectionSelectedItem != null)
                return true;
            else
                return false;
        }
        public void RentLivingNewSectionDuplicateCommand_Execute()
        {
            if (RentLivingNew == null) return;
            if (RentLivingNewSectionSelectedItem == null) return;

            //
            RentLivingNewSectionNew = new RentLivingSection(RentLivingNew.Rent_ID, RentLivingNew.RentLiving_ID, Guid.NewGuid().ToString());
            RentLivingNewSectionNew.IsNew = true;
            RentLivingNewSectionNew.IsDirty = false;

            RentLivingNewSectionNew.RentLivingSectionRoomNumber = RentLivingNewSectionSelectedItem.RentLivingSectionRoomNumber + "の複製";
            RentLivingNewSectionNew.RentLivingSectionMadori = RentLivingNewSectionSelectedItem.RentLivingSectionMadori;
            RentLivingNewSectionNew.RentLivingSectionPrice = RentLivingNewSectionSelectedItem.RentLivingSectionPrice;
            // TODO: more to come

            // 追加
            RentLivingNew.RentLivingSections.Add(RentLivingNewSectionNew);
        }

        // RL新規　部屋一覧の選択を削除
        public ICommand RentLivingNewSectionDeleteCommand { get; }
        public bool RentLivingNewSectionDeleteCommand_CanExecute()
        {
            if (RentLivingNewSectionSelectedItem != null)
                return true;
            else
                return false;
        }
        public void RentLivingNewSectionDeleteCommand_Execute()
        {
            if (RentLivingNew == null) return;
            if (RentLivingNewSectionSelectedItem == null) return;

            // 削除
            RentLivingNew.RentLivingSections.Remove(RentLivingNewSectionSelectedItem);
        }


        // RL新規　新規部屋の画像追加
        public ICommand RentLivingNewSectionNewPictureAddCommand { get; }
        public bool RentLivingNewSectionNewPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionNewPictureAddCommand_Execute()
        {
            if (RentLivingNew == null) return;
            if (RentLivingNewSectionNew == null) return;

            var files = _openDialogService.GetOpenPictureFileDialog("部屋の写真追加");

            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 画像データの読み込み
                            byte[] ImageData;
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            //BinaryReader br = new BinaryReader(fs);
                            //ImageData = br.ReadBytes((int)fs.Length);
                            //br.Close();
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                            ImageData = ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = FixedSize(img, 200, 150);
                            byte[] ImageThumbData = ImageToByteArray(thumbImg);


                            RentLivingSectionPicture rlpic = new RentLivingSectionPicture(RentLivingNewSectionNew.Rent_ID, RentLivingNewSectionNew.RentLiving_ID, RentLivingNewSectionNew.RentLivingSection_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsNew = true;
                            rlpic.IsModified = false;

                            rlpic.Picture = BitmapImageFromImage(thumbImg, FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingNewSectionNew.RentLivingSectionPictures.Add(rlpic);


                            fs.Close();
                        }
                        else
                        {
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "File";
                            er.ErrCode = 0;
                            er.ErrText = "「" + "File Does Not Exist" + "」";
                            er.ErrDescription = fileName + " ファイルが存在しません。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingNewSectionNewPictureAddCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // RL新規　新規部屋の画像削除
        public ICommand RentLivingNewSectionNewPictureDeleteCommand { get; }
        public bool RentLivingNewSectionNewPictureDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionNewPictureDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingNew == null) return;
            if (RentLivingNewSectionNew == null) return;

            // 選択アイテム保持用
            List<RentLivingSectionPicture> selectedList = new List<RentLivingSectionPicture>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingSectionPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingSectionPicture);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                RentLivingNewSectionNew.RentLivingSectionPictures.Remove(item);

                // 新規部屋なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
            }

        }

        // RL新規　編集部屋の画像追加
        public ICommand RentLivingNewSectionEditPictureAddCommand { get; }
        public bool RentLivingNewSectionEditPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionEditPictureAddCommand_Execute()
        {
            if (RentLivingNew == null) return;
            if (RentLivingNewSectionEdit == null) return;

            var files = _openDialogService.GetOpenPictureFileDialog("部屋の写真追加");

            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 画像データの読み込み
                            byte[] ImageData;
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            //BinaryReader br = new BinaryReader(fs);
                            //ImageData = br.ReadBytes((int)fs.Length);
                            //br.Close();
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                            ImageData = ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = FixedSize(img, 200, 150);
                            byte[] ImageThumbData = ImageToByteArray(thumbImg);


                            RentLivingSectionPicture rlpic = new RentLivingSectionPicture(RentLivingNewSectionEdit.Rent_ID, RentLivingNewSectionEdit.RentLiving_ID, RentLivingNewSectionEdit.RentLivingSection_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsNew = true;
                            rlpic.IsModified = false;

                            rlpic.Picture = BitmapImageFromImage(thumbImg, FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingNewSectionEdit.RentLivingSectionPictures.Add(rlpic);


                            fs.Close();
                        }
                        else
                        {
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "File";
                            er.ErrCode = 0;
                            er.ErrText = "「" + "File Does Not Exist" + "」";
                            er.ErrDescription = fileName + " ファイルが存在しません。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingNewSectionNewPictureAddCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // RL新規　編集部屋の画像削除
        public ICommand RentLivingNewSectionEditPictureDeleteCommand { get; }
        public bool RentLivingNewSectionEditPictureDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionEditPictureDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingNew == null) return;
            if (RentLivingNewSectionEdit == null) return;

            // 選択アイテム保持用
            List<RentLivingSectionPicture> selectedList = new List<RentLivingSectionPicture>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingSectionPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingSectionPicture);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                RentLivingNewSectionEdit.RentLivingSectionPictures.Remove(item);

                // 新規部屋なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
            }
        }


        // RL新規　物件の図面PDF追加
        public ICommand RentLivingNewZumenPdfAddCommand { get; }
        public bool RentLivingNewZumenPdfAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewZumenPdfAddCommand_Execute()
        {
            if (RentLivingNew == null) return;

            string fileName = _openDialogService.GetOpenZumenPdfFileDialog("図面の追加");

            if (!string.IsNullOrEmpty(fileName))
            {
                FileInfo fi = new FileInfo(fileName.Trim());
                if (fi.Exists)
                {
                    // 図面ファイルのPDFデータの読み込み
                    byte[] PdfData;
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    long len = fs.Length;

                    BinaryReader br = new BinaryReader(fs);
                    PdfData = br.ReadBytes((int)fs.Length);
                    br.Close();

                    RentLivingZumenPDF rlZumen = new RentLivingZumenPDF(RentLivingNew.Rent_ID, RentLivingNew.RentLiving_ID, Guid.NewGuid().ToString());
                    rlZumen.PDFData = PdfData;
                    rlZumen.FileSize = len;

                    // TODO:
                    //rlZumen.DateTimeAdded = DateTime.Now;
                    rlZumen.DateTimePublished = DateTime.Now;
                    rlZumen.DateTimeVerified = DateTime.Now;

                    //rlZumen.IsModified = false;
                    rlZumen.IsNew = true;

                    RentLivingNew.RentLivingZumenPDFs.Add(rlZumen);

                    fs.Close();
                }
                else
                {
                    // エラーイベント発火
                    MyError er = new MyError();
                    er.ErrType = "File";
                    er.ErrCode = 0;
                    er.ErrText = "「" + "File Does Not Exist" + "」";
                    er.ErrDescription = fileName + " ファイルが存在しません。";
                    er.ErrDatetime = DateTime.Now;
                    er.ErrPlace = "MainViewModel::RentLivingNewZumenPdfAddCommand_Execute()";
                    ErrorOccured?.Invoke(er);
                }
            }
        }

        // RL新規　物件の図面PDF削除
        public ICommand RentLivingNewZumenPdfDeleteCommand { get; }
        public bool RentLivingNewZumenPdfDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewZumenPdfDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingNew == null) return;

            // 選択アイテム保持用
            List<RentLivingZumenPDF> selectedList = new List<RentLivingZumenPDF>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingZumenPDF>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingZumenPDF);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                RentLivingNew.RentLivingZumenPDFs.Remove(item);
            }

        }

        // RL編集　物件の図面PDF表示
        public ICommand RentLivingNewZumenPdfShowCommand { get; }
        public bool RentLivingNewZumenPdfShowCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewZumenPdfShowCommand_Execute(object obj)
        {
            if (obj == null) return;
            if (RentLivingNew == null) return;
            
            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingZumenPDF>();

            foreach (var item in collection)
            {
                byte[] pdfBytes = (item as RentLivingZumenPDF).PDFData;

                File.WriteAllBytes(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf", pdfBytes);

                Process.Start(new ProcessStartInfo(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf") { UseShellExecute = true });

                break;
            }
            
        }

        // RL編集　物件の図面PDF表示（ダブルクリックやエンター押下で）
        public ICommand RentLivingNewZumenPdfEnterCommand { get; }
        public bool RentLivingNewZumenPdfEnterCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewZumenPdfEnterCommand_Execute(RentZumenPDF obj)
        {
            if (obj == null) return;
            if (RentLivingNew == null) return;

            byte[] pdfBytes = (obj as RentZumenPDF).PDFData;

            File.WriteAllBytes(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf", pdfBytes);

            Process.Start(new ProcessStartInfo(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf") { UseShellExecute = true });
        }

        #endregion

        #region == RL 編集物件 ==

        // RL編集　物件管理、一覧選択アイテム編集（画面表示）
        public ICommand RentLivingEditSelectedEditCommand { get; }
        public bool RentLivingEditSelectedEditCommand_CanExecute()
        {
            if (RentLivingEditSelectedItem == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void RentLivingEditSelectedEditCommand_Execute()
        {
            if (RentLivingEditSelectedItem != null)
            {
                RentLivingEdit = new RentLiving(RentLivingEditSelectedItem.Rent_ID, RentLivingEditSelectedItem.RentLiving_ID);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();

                        // 物件＋住居用ジョイン
                        cmd.CommandText = String.Format("SELECT * FROM Rent INNER JOIN RentLiving ON Rent.Rent_ID = RentLiving.Rent_ID WHERE Rent.Rent_ID = '{0}'", RentLivingEditSelectedItem.Rent_ID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //RentLivingEdit.Rent_ID = rentid;
                                RentLivingEdit.Name = Convert.ToString(reader["Name"]);
                                //RentLivingEdit.Type = RentLivingEdit.StringToRentType[Convert.ToString(reader["Type"])];
                                RentLivingEdit.PostalCode = Convert.ToString(reader["PostalCode"]);
                                RentLivingEdit.Location = Convert.ToString(reader["Location"]);
                                RentLivingEdit.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                                RentLivingEdit.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                                //RentLivingEdit.RentLiving_ID = Convert.ToString(reader["RentLiving_ID"]);
                                RentLivingEdit.Kind = RentLivingEdit.StringToRentLivingKind[Convert.ToString(reader["Kind"])];
                                RentLivingEdit.Floors = Convert.ToInt32(reader["Floors"]);
                                RentLivingEdit.FloorsBasement = Convert.ToInt32(reader["FloorsBasement"]);
                                RentLivingEdit.BuiltYear = Convert.ToInt32(reader["BuiltYear"]);

                            }
                        }

                        // 物件写真
                        cmd.CommandText = String.Format("SELECT * FROM RentLivingPicture WHERE Rent_ID = '{0}'", RentLivingEditSelectedItem.Rent_ID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                RentLivingPicture rlpic = new RentLivingPicture(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Convert.ToString(reader["RentLivingPicture_ID"]));

                                byte[] imageBytes = (byte[])reader["PictureData"];
                                rlpic.PictureData = imageBytes;

                                byte[] imageThumbBytes = (byte[])reader["PictureThumbW200xData"];
                                rlpic.PictureThumbW200xData =  imageThumbBytes;
                                /*
                                byte[] imageBytes = new byte[0];
                                if (reader["PictureData"] != null && !Convert.IsDBNull(reader["PictureData"]))
                                {
                                    imageBytes = (byte[])(reader["PictureData"]);
                                }
                                */

                                rlpic.PictureFileExt = Convert.ToString(reader["PictureFileExt"]);

                                rlpic.Picture = BitmapImageFromBytes(imageThumbBytes);

                                RentLivingEdit.RentLivingPictures.Add(rlpic);
                            }
                        }

                        // 図面
                        cmd.CommandText = String.Format("SELECT * FROM RentLivingZumenPdf WHERE Rent_ID = '{0}'", RentLivingEditSelectedItem.Rent_ID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RentLivingZumenPDF rlpdf = new RentLivingZumenPDF(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Convert.ToString(reader["RentLivingZumenPdf_ID"]));

                                byte[] pdfBytes = (byte[])reader["PdfData"];
                                rlpdf.PDFData = pdfBytes;

                                /*
                                byte[] imageBytes = new byte[0];
                                if (reader["PictureData"] != null && !Convert.IsDBNull(reader["PictureData"]))
                                {
                                    imageBytes = (byte[])(reader["PictureData"]);
                                }
                                */

                                DateTime dt = new DateTime();

                                dt = DateTime.Parse(Convert.ToString(reader["DateTimeAdded"]));
                                rlpdf.DateTimeAdded = dt.ToLocalTime();
                                dt = DateTime.Parse(Convert.ToString(reader["DateTimePublished"]));
                                rlpdf.DateTimePublished = dt.ToLocalTime();
                                dt = DateTime.Parse(Convert.ToString(reader["DateTimeVerified"]));
                                rlpdf.DateTimeVerified = dt.ToLocalTime();

                                rlpdf.FileSize = Convert.ToInt64(reader["FileSize"]);

                                rlpdf.IsNew = false;

                                RentLivingEdit.RentLivingZumenPDFs.Add(rlpdf);
                            }
                        }

                        // 部屋
                        cmd.CommandText = String.Format("SELECT * FROM RentLivingSection WHERE Rent_ID = '{0}'", RentLivingEditSelectedItem.Rent_ID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RentLivingSection room = new RentLivingSection(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Convert.ToString(reader["RentLivingSection_ID"]));
                                room.RentLivingSectionRoomNumber = Convert.ToString(reader["RoomNumber"]);
                                room.RentLivingSectionMadori = Convert.ToString(reader["Madori"]);
                                room.RentLivingSectionPrice = Convert.ToInt32(reader["Price"]);

                                room.IsNew = false;
                                room.IsDirty = false;

                                RentLivingEdit.RentLivingSections.Add(room);
                            }
                        }

                        // 部屋写真
                        foreach (var hoge in RentLivingEdit.RentLivingSections)
                        {
                            cmd.CommandText = String.Format("SELECT * FROM RentLivingSectionPicture WHERE RentLivingSection_ID = '{0}'", hoge.RentLivingSection_ID);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    RentLivingSectionPicture rlsecpic = new RentLivingSectionPicture(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, hoge.RentLivingSection_ID, Convert.ToString(reader["RentLivingSectionPicture_ID"]));

                                    byte[] imageBytes = (byte[])reader["PictureData"];
                                    rlsecpic.PictureData = imageBytes;

                                    byte[] imageThumbBytes = (byte[])reader["PictureThumbW200xData"];
                                    rlsecpic.PictureThumbW200xData = imageThumbBytes;
                                    /*
                                    byte[] imageBytes = new byte[0];
                                    if (reader["PictureData"] != null && !Convert.IsDBNull(reader["PictureData"]))
                                    {
                                        imageBytes = (byte[])(reader["PictureData"]);
                                    }
                                    */

                                    rlsecpic.PictureFileExt = Convert.ToString(reader["PictureFileExt"]);

                                    rlsecpic.Picture = BitmapImageFromBytes(imageThumbBytes);

                                    rlsecpic.IsNew = false;
                                    rlsecpic.IsModified = false;

                                    hoge.RentLivingSectionPictures.Add(rlsecpic);

                                }
                            }

                        }

                        cmd.Transaction.Commit();
                    }
                }


                // 編集画面の表示
                if (!ShowRentLivingEdit) ShowRentLivingEdit = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("RentLivingEditSelectedItem == null at RentLivingEditSelectedEditCommand_Execute");
            }
        }

        // RL編集　物件管理、一覧選択アイテム編集、更新キャンセル
        public ICommand RentLivingEditSelectedEditCancelCommand { get; }
        public bool RentLivingEditSelectedEditCancelCommand_CanExecute()
        {
            if (ShowRentLivingEdit)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void RentLivingEditSelectedEditCancelCommand_Execute()
        {
            if (ShowRentLivingEdit) ShowRentLivingEdit = false;
        }

        // RL編集　物件管理、選択編集、更新（UPDATE）
        public ICommand RentLivingEditSelectedEditUpdateCommand { get; }
        public bool RentLivingEditSelectedEditUpdateCommand_CanExecute()
        {
            if (RentLivingEditSelectedItem == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void RentLivingEditSelectedEditUpdateCommand_Execute()
        {

            if (RentLivingEdit == null) return;
            if (RentLivingEditSelectedItem == null) return;

            // TODO: 入力チェック

            // 編集オブジェクトに格納されている更新された情報をDBへ更新

            string sqlUpdateRent = String.Format("UPDATE Rent SET Name = '{1}', Type = '{2}', PostalCode = '{3}', Location = '{4}', TrainStation1 = '{5}', TrainStation2 = '{6}' WHERE Rent_ID = '{0}'",
                RentLivingEdit.Rent_ID, RentLivingEdit.Name, RentLivingEdit.Type.ToString(), RentLivingEdit.PostalCode, RentLivingEdit.Location, RentLivingEdit.TrainStation1, RentLivingEdit.TrainStation2);

            string sqlUpdateRentLiving = String.Format("UPDATE RentLiving SET Kind = '{1}', Floors = '{2}', FloorsBasement = '{3}', BuiltYear = '{4}' WHERE RentLiving_ID = '{0}'",
                RentLivingEdit.RentLiving_ID, RentLivingEdit.Kind.ToString(), RentLivingEdit.Floors, RentLivingEdit.FloorsBasement, RentLivingEdit.BuiltYear);

            // TODO more to come

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.Transaction = connection.BeginTransaction();
                    try
                    {
                        cmd.CommandText = sqlUpdateRent;
                        var result = cmd.ExecuteNonQuery();
                        if (result != 1)
                        {
                            // 
                        }

                        cmd.CommandText = sqlUpdateRentLiving;
                        result = cmd.ExecuteNonQuery();
                        if (result != 1)
                        {
                            // 
                        }

                        // 物件写真の追加または更新
                        if (RentLivingEdit.RentLivingPictures.Count > 0)
                        {
                            foreach (var pic in RentLivingEdit.RentLivingPictures)
                            {
                                if (pic.IsNew)
                                {
                                    string sqlInsertIntoRentLivingPicture = String.Format("INSERT INTO RentLivingPicture (RentLivingPicture_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbW200xData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', @0, @1, '{5}')",
                                        pic.RentPicture_ID, RentLivingEdit.RentLiving_ID, RentLivingEdit.Rent_ID, pic.PictureData, pic.PictureThumbW200xData, pic.PictureFileExt);

                                    // 物件画像の追加
                                    cmd.CommandText = sqlInsertIntoRentLivingPicture;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pic.PictureData;
                                    cmd.Parameters.Add(parameter1);

                                    SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                    parameter2.Value = pic.PictureThumbW200xData;
                                    cmd.Parameters.Add(parameter2);

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pic.IsNew = false;
                                        pic.IsModified = false;
                                    }
                                }
                                else if (pic.IsModified)
                                {
                                    string sqlUpdateRentLivingPicture = String.Format("UPDATE RentLivingPicture SET PictureData = @0, PictureThumbW200xData = @1, PictureFileExt = '{5}' WHERE RentLivingPicture_ID = '{0}'",
                                        pic.RentPicture_ID, RentLivingEdit.RentLiving_ID, RentLivingEdit.Rent_ID, pic.PictureData, pic.PictureThumbW200xData, pic.PictureFileExt);

                                    // 物件画像の更新
                                    cmd.CommandText = sqlUpdateRentLivingPicture;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pic.PictureData;
                                    cmd.Parameters.Add(parameter1);

                                    SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                    parameter2.Value = pic.PictureThumbW200xData;
                                    cmd.Parameters.Add(parameter2);

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pic.IsNew = false;
                                        pic.IsModified = false;
                                    }
                                }

                            }
                        }

                        // 物件写真の削除リストを処理
                        if (RentLivingEdit.RentLivingPicturesToBeDeletedIDs.Count > 0)
                        {
                            foreach (var id in RentLivingEdit.RentLivingPicturesToBeDeletedIDs)
                            {
                                // 削除
                                string sqlDeleteRentLivingPicture = String.Format("DELETE FROM RentLivingPicture WHERE RentLivingPicture_ID = '{0}'",
                                        id);

                                cmd.CommandText = sqlDeleteRentLivingPicture;
                                var DelRentLivingPicResult = cmd.ExecuteNonQuery();
                                if (DelRentLivingPicResult > 0)
                                {
                                    //
                                }
                            }
                            RentLivingEdit.RentLivingPicturesToBeDeletedIDs.Clear();
                        }

                        // 図面の更新
                        if (RentLivingEdit.RentLivingZumenPDFs.Count > 0)
                        {
                            foreach (var pdf in RentLivingEdit.RentLivingZumenPDFs)
                            {
                                if (pdf.IsNew)
                                {
                                    string sqlInsertIntoRentLivingZumen = String.Format("INSERT INTO RentLivingZumenPdf (RentLivingZumenPdf_ID, RentLiving_ID, Rent_ID, PdfData, DateTimeAdded, DateTimePublished, DateTimeVerified, FileSize) VALUES ('{0}', '{1}', '{2}', @0, '{4}', '{5}', '{6}', '{7}')",
                                        pdf.RentZumenPDF_ID, RentLivingEdit.RentLiving_ID, RentLivingEdit.Rent_ID, pdf.PDFData, pdf.DateTimeAdded.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

                                    // 図面の追加
                                    cmd.CommandText = sqlInsertIntoRentLivingZumen;
                                    // ループなので、前のパラメーターをクリアする。
                                    cmd.Parameters.Clear();

                                    SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                    parameter1.Value = pdf.PDFData;
                                    cmd.Parameters.Add(parameter1);

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pdf.IsNew = false;
                                        pdf.IsDirty = false;
                                    }
                                }
                                else if (pdf.IsDirty)
                                {
                                    string sqlUpdateRentLivingZumen = String.Format("UPDATE RentLivingZumenPdf SET DateTimePublished = '{1}', DateTimeVerified = '{2}' WHERE RentLivingZumenPdf_ID = '{0}'",
                                        pdf.RentZumenPDF_ID, pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

                                    // 図面アトリビュート情報の更新
                                    cmd.CommandText = sqlUpdateRentLivingZumen;

                                    result = cmd.ExecuteNonQuery();
                                    if (result > 0)
                                    {
                                        pdf.IsNew = false;
                                        pdf.IsDirty = false;
                                    }
                                }
                            }
                        }

                        // 図面の削除リストを処理
                        if (RentLivingEdit.RentLivingZumenPdfToBeDeletedIDs.Count > 0)
                        {
                            foreach (var id in RentLivingEdit.RentLivingZumenPdfToBeDeletedIDs)
                            {
                                // 削除
                                string sqlDeleteRentLivingPDF = String.Format("DELETE FROM RentLivingZumenPdf WHERE RentLivingZumenPdf_ID = '{0}'",
                                        id);

                                cmd.CommandText = sqlDeleteRentLivingPDF;
                                var DelRentLivingPdfResult = cmd.ExecuteNonQuery();
                                if (DelRentLivingPdfResult > 0)
                                {
                                    //
                                }
                            }
                            RentLivingEdit.RentLivingZumenPdfToBeDeletedIDs.Clear();
                        }

                        // 部屋更新
                        if (RentLivingEdit.RentLivingSections.Count > 0)
                        {
                            foreach (var room in RentLivingEdit.RentLivingSections)
                            {
                                if (room.IsNew)
                                {
                                    // 新規追加
                                    string sqlInsertIntoRentLivingSection = String.Format("INSERT INTO RentLivingSection (RentLivingSection_ID, RentLiving_ID, Rent_ID, RoomNumber, Price, Madori) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                                            room.RentLivingSection_ID, RentLivingEdit.RentLiving_ID, RentLivingEdit.Rent_ID, room.RentLivingSectionRoomNumber, room.RentLivingSectionPrice, room.RentLivingSectionMadori);

                                    cmd.CommandText = sqlInsertIntoRentLivingSection;
                                    var InsertIntoRentLivingSectionResult = cmd.ExecuteNonQuery();
                                    if (InsertIntoRentLivingSectionResult > 0)
                                    {
                                        room.IsNew = false;
                                        room.IsDirty = false;
                                    }
                                }
                                // TODO:
                                //else if (room.IsDirty)
                                else
                                {
                                    // 更新
                                    string sqlUpdateRentLivingSection = String.Format("UPDATE RentLivingSection SET RoomNumber = '{1}', Price = '{2}', Madori = '{3}' WHERE RentLivingSection_ID = '{0}'",
                                            room.RentLivingSection_ID, room.RentLivingSectionRoomNumber, room.RentLivingSectionPrice, room.RentLivingSectionMadori);
                                    
                                    cmd.CommandText = sqlUpdateRentLivingSection;
                                    var UpdateoRentLivingSectionResult = cmd.ExecuteNonQuery();
                                    if (UpdateoRentLivingSectionResult > 0)
                                    {
                                        //room.IsNew = false;
                                        room.IsDirty = false;
                                    }
                                }

                                // 部屋の写真
                                if (room.RentLivingSectionPictures.Count > 0)
                                {
                                    foreach (var roompic in room.RentLivingSectionPictures)
                                    {
                                        if (roompic.IsNew)
                                        {
                                            string sqlInsertIntoRentLivingSectionPic = String.Format("INSERT INTO RentLivingSectionPicture (RentLivingSectionPicture_ID, RentLivingSection_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbW200xData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', '{3}', @0, @1, '{6}')",
                                                roompic.RentSectionPicture_ID, roompic.RentLivingSection_ID, roompic.RentLiving_ID, roompic.Rent_ID, roompic.PictureData, roompic.PictureThumbW200xData, roompic.PictureFileExt);

                                            cmd.CommandText = sqlInsertIntoRentLivingSectionPic;
                                            // ループなので、前のパラメーターをクリアする。
                                            cmd.Parameters.Clear();

                                            SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                            parameter1.Value = roompic.PictureData;
                                            cmd.Parameters.Add(parameter1);

                                            SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                            parameter2.Value = roompic.PictureThumbW200xData;
                                            cmd.Parameters.Add(parameter2);

                                            var InsertIntoRentLivingSectionPicResult = cmd.ExecuteNonQuery();
                                            if (InsertIntoRentLivingSectionPicResult > 0)
                                            {
                                                roompic.IsNew = false;
                                                roompic.IsModified = false;
                                            }
                                        }
                                        else if (roompic.IsModified)
                                        {
                                            string sqlUpdateRentLivingSectionPic = String.Format("UPDATE RentLivingSectionPicture SET PictureData = @0, PictureThumbW200xData = @1, PictureFileExt = '{6}' WHERE RentLivingSectionPicture_ID = '{0}'",
                                                roompic.RentSectionPicture_ID, roompic.RentLivingSection_ID, roompic.RentLiving_ID, roompic.Rent_ID, roompic.PictureData, roompic.PictureThumbW200xData, roompic.PictureFileExt);

                                            cmd.CommandText = sqlUpdateRentLivingSectionPic;
                                            // ループなので、前のパラメーターをクリアする。
                                            cmd.Parameters.Clear();

                                            SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                            parameter1.Value = roompic.PictureData;
                                            cmd.Parameters.Add(parameter1);

                                            SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                            parameter2.Value = roompic.PictureThumbW200xData;
                                            cmd.Parameters.Add(parameter2);

                                            var UpdateRentLivingSectionPicResult = cmd.ExecuteNonQuery();
                                            if (UpdateRentLivingSectionPicResult > 0)
                                            {
                                                roompic.IsNew = false;
                                                roompic.IsModified = false;
                                            }
                                        }
                                    }
                                }

                                // 部屋画像の削除リストを処理
                                if (room.RentLivingSectionPicturesToBeDeletedIDs.Count > 0)
                                {
                                    foreach (var id in room.RentLivingSectionPicturesToBeDeletedIDs)
                                    {
                                        // 削除
                                        string sqlDeleteRentLivingSectionPicture = String.Format("DELETE FROM RentLivingSectionPicture WHERE RentLivingSectionPicture_ID = '{0}'",
                                                id);

                                        cmd.CommandText = sqlDeleteRentLivingSectionPicture;
                                        var UpdateoRentLivingSectionResult = cmd.ExecuteNonQuery();
                                        if (UpdateoRentLivingSectionResult > 0)
                                        {
                                            //
                                        }
                                    }
                                }

                            }
                        }

                        // 部屋の削除リストを処理
                        if (RentLivingEdit.RentLivingSectionToBeDeletedIDs.Count > 0)
                        {
                            foreach (var id in RentLivingEdit.RentLivingSectionToBeDeletedIDs)
                            {
                                // 削除
                                string sqlDeleteRentLivingSection = String.Format("DELETE FROM RentLivingSection WHERE RentLivingSection_ID = '{0}'",
                                        id);

                                cmd.CommandText = sqlDeleteRentLivingSection;
                                var DelRentLivingPdfResult = cmd.ExecuteNonQuery();
                                if (DelRentLivingPdfResult > 0)
                                {
                                    //
                                }
                            }
                            RentLivingEdit.RentLivingSectionToBeDeletedIDs.Clear();
                        }

                        cmd.Transaction.Commit();

                        // 編集オブジェクトに格納された情報を、選択アイテムに更新（Listviewの情報が更新されるー＞DBからSelectして一覧を読み込みし直さなくて良くなる）
                        RentLivingEditSelectedItem.Name = RentLivingEdit.Name;
                        RentLivingEditSelectedItem.PostalCode = RentLivingEdit.PostalCode;
                        RentLivingEditSelectedItem.Location = RentLivingEdit.Location;
                        RentLivingEditSelectedItem.TrainStation1 = RentLivingEdit.TrainStation1;
                        RentLivingEditSelectedItem.TrainStation2 = RentLivingEdit.TrainStation2;
                        // TODO

                        // 編集画面を非表示に
                        if (ShowRentLivingEdit) ShowRentLivingEdit = false;
                    }
                    catch (Exception e)
                    {
                        cmd.Transaction.Rollback();

                        System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::RentLivingEditSelectedEditUpdateCommand_Execute()");

                        // エラーイベント発火
                        MyError er = new MyError();
                        er.ErrType = "DB";
                        er.ErrCode = 0;
                        er.ErrText = "「" + e.Message + "」";
                        er.ErrDescription = "賃貸住居用物件の選択アイテム編集更新 (UPDATE)で、データベースを更新する処理でエラーが発生し、ロールバックしました。";
                        er.ErrDatetime = DateTime.Now;
                        er.ErrPlace = "MainViewModel::RentLivingEditSelectedEditUpdateCommand_Execute()";
                        ErrorOccured?.Invoke(er);
                    }
                }
            }
        }


        // RL編集　物件管理　物件画像追加
        public ICommand RentLivingEditPictureAddCommand { get; }
        public bool RentLivingEditPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditPictureAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;

            var files = _openDialogService.GetOpenPictureFileDialog("物件写真の追加");

            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 画像データの読み込み
                            byte[] ImageData;
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            //BinaryReader br = new BinaryReader(fs);
                            //ImageData = br.ReadBytes((int)fs.Length);
                            //br.Close();
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                            ImageData = ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = FixedSize(img, 200, 150);
                            byte[] ImageThumbData = ImageToByteArray(thumbImg);


                            RentLivingPicture rlpic = new RentLivingPicture(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;
                            rlpic.IsModified = false;
                            rlpic.IsNew = true;

                            rlpic.Picture = BitmapImageFromImage(thumbImg, FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingEdit.RentLivingPictures.Add(rlpic);


                            fs.Close();
                        }
                        else
                        {
                            // エラーイベント発火
                            MyError er = new MyError
                            {
                                ErrType = "File",
                                ErrCode = 0,
                                ErrText = "「" + "File Does Not Exist" + "」",
                                ErrDescription = fileName + " ファイルが存在しません。",
                                ErrDatetime = DateTime.Now,
                                ErrPlace = "MainViewModel::RentLivingEditAddCommand_Execute()"
                            };
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // RL編集　物件管理　物件画像削除
        public ICommand RentLivingEditPictureDeleteCommand { get; }
        public bool RentLivingEditPictureDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditPictureDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;

            // 選択アイテム保持用
            List<RentLivingPicture> selectedList = new List<RentLivingPicture>();
            // キャンセルする注文IDを保持
            //List<int> cancelIdList = new List<int>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingPicture);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                if (item.IsNew)
                {
                    // 新規画像なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
                }
                else
                {
                    // DBからも削除するために、削除リストに追加（後で削除）
                    RentLivingEdit.RentLivingPicturesToBeDeletedIDs.Add(item.RentPicture_ID);
                }

                // 一覧から削除
                RentLivingEdit.RentLivingPictures.Remove(item);
            }

        }

        // RL編集　物件管理　物件画像差し替え更新
        public ICommand RentLivingEditPictureChangeCommand { get; }
        public bool RentLivingEditPictureChangeCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditPictureChangeCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;

            System.Collections.IList items = (System.Collections.IList)obj;
            
            if (items.Count > 0)
            {
                RentLivingPicture rlpic = items.Cast<RentLivingPicture>().First();

                var files = _openDialogService.GetOpenPictureFileDialog("物件写真の差し替え", false);

                if (files != null)
                {
                    foreach (String filePath in files)
                    {
                        string fileName = filePath.Trim();

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            FileInfo fi = new FileInfo(fileName);
                            if (fi.Exists)
                            {
                                // 画像データの読み込み
                                byte[] ImageData;
                                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                                //BinaryReader br = new BinaryReader(fs);
                                //ImageData = br.ReadBytes((int)fs.Length);
                                //br.Close();
                                System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                                ImageData = ImageToByteArray(img);

                                // サムネイル画像の作成
                                System.Drawing.Image thumbImg = FixedSize(img, 200, 150);
                                byte[] ImageThumbData = ImageToByteArray(thumbImg);

                                // データ更新
                                rlpic.PictureData = ImageData;
                                rlpic.PictureThumbW200xData = ImageThumbData;
                                rlpic.PictureFileExt = fi.Extension;
                                rlpic.IsModified = true;
                                rlpic.IsNew = false;

                                rlpic.Picture = BitmapImageFromImage(thumbImg, FileExtToImageFormat(rlpic.PictureFileExt));

                                fs.Close();
                            }
                            else
                            {
                                // エラーイベント発火
                                MyError er = new MyError();
                                er.ErrType = "File";
                                er.ErrCode = 0;
                                er.ErrText = "「" + "File Does Not Exist" + "」";
                                er.ErrDescription = fileName + " ファイルが存在しません。";
                                er.ErrDatetime = DateTime.Now;
                                er.ErrPlace = "MainViewModel::RentLivingEditPictureChangeCommand_Execute()";
                                ErrorOccured?.Invoke(er);
                            }
                        }

                        // multi = false なので、
                        break;
                    }
                }
            }



        }


        // RL編集　部屋追加（画面表示）
        public ICommand RentLivingEditSectionNewCommand { get; }
        public bool RentLivingEditSectionNewCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionNewCommand_Execute()
        {
            if (RentLivingEdit == null) return;

            // RentLivingEditSectionNew オブジェクトを用意
            RentLivingEditSectionNew = new RentLivingSection(RentLivingNew.Rent_ID, RentLivingNew.RentLiving_ID, Guid.NewGuid().ToString());
            
            RentLivingEditSectionNew.IsNew = true;
            RentLivingEditSectionNew.IsDirty = false;

            if (!ShowRentLivingEditSectionNew) ShowRentLivingEditSectionNew = true;
        }
        
        // RL編集　部屋追加キャンセル
        public ICommand RentLivingEditSectionNewCancelCommand { get; }
        public bool RentLivingEditSectionNewCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionNewCancelCommand_Execute()
        {
            if (ShowRentLivingEditSectionNew) ShowRentLivingEditSectionNew = false;
        }

        // RL編集　部屋追加処理 (Add to the collection)
        public ICommand RentLivingEditSectionAddCommand { get; }
        public bool RentLivingEditSectionAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionNew == null) return;

            // TODO: 入力チェック

            // 物件オブジェクトの部屋コレクションに追加
            RentLivingEdit.RentLivingSections.Add(RentLivingEditSectionNew);

            // 追加画面を閉じる
            ShowRentLivingEditSectionNew = false;
        }

        // RL編集　部屋編集（画面表示）
        public ICommand RentLivingEditSectionEditCommand { get; }
        public bool RentLivingEditSectionEditCommand_CanExecute()
        {
            if (RentLivingEditSectionSelectedItem != null)
                return true;
            else
                return false;
        }
        public void RentLivingEditSectionEditCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionSelectedItem == null) return;

            //
            RentLivingEditSectionEdit = RentLivingEditSectionSelectedItem;

            /*
            RentLivingEditSectionEdit = new RentLivingSection(RentLivingEditSectionSelectedItem.Rent_ID, RentLivingEditSectionSelectedItem.RentLiving_ID, RentLivingEditSectionSelectedItem.RentLivingSection_ID);

            RentLivingEditSectionEdit.IsNew = false;
            RentLivingEditSectionEdit.IsDirty = false;

            RentLivingEditSectionEdit.RentLivingSectionRoomNumber = RentLivingEditSectionSelectedItem.RentLivingSectionRoomNumber;
            RentLivingEditSectionEdit.RentLivingSectionMadori = RentLivingEditSectionSelectedItem.RentLivingSectionMadori;
            RentLivingEditSectionEdit.RentLivingSectionPrice = RentLivingEditSectionSelectedItem.RentLivingSectionPrice;
            // TODO: more to come

            // TODO: これは・・・
            foreach (var hoge in RentLivingEditSectionSelectedItem.RentLivingSectionPictures)
            {
                RentLivingEditSectionEdit.RentLivingSectionPictures.Add(hoge);
            }
            */

            if (!ShowRentLivingEditSectionEdit) ShowRentLivingEditSectionEdit = true;
        }

        // RL編集　部屋編集キャンセル
        public ICommand RentLivingEditSectionEditCancelCommand { get; }
        public bool RentLivingEditSectionEditCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionEditCancelCommand_Execute()
        {
            if (ShowRentLivingEditSectionEdit) ShowRentLivingEditSectionEdit = false;
        }

        // RL編集　部屋更新処理 (Update Collection)
        public ICommand RentLivingEditSectionUpdateCommand { get; }
        public bool RentLivingEditSectionUpdateCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionUpdateCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionSelectedItem == null) return;

            // TODO: 入力チェック

            /*
            var found = RentLivingEdit.RentLivingSections.FirstOrDefault(x => x.RentLivingSection_ID == RentLivingEditSectionSelectedItem.RentLivingSection_ID);
            if (found != null)
            {
                found.RentLivingSectionRoomNumber = RentLivingEditSectionEdit.RentLivingSectionRoomNumber;
                found.RentLivingSectionMadori = RentLivingEditSectionEdit.RentLivingSectionMadori;
                found.RentLivingSectionPrice = RentLivingEditSectionEdit.RentLivingSectionPrice;
                // TODO: more to come

                // 一旦クリアして追加しなおさないと、変更が通知（更新）されない
                found.RentLivingSectionPictures.Clear();
                foreach (var hoge in RentLivingEditSectionEdit.RentLivingSectionPictures)
                {
                    found.RentLivingSectionPictures.Add(hoge);
                }


            }
            else
            {
                System.Diagnostics.Debug.WriteLine("THIS SHOULD NOT BE HAPPENING @RentLivingEditSectionUpdateCommand_Execute");
            }
            */

            // 部屋編集画面を閉じる
            ShowRentLivingEditSectionEdit = false;
        }

        // RL編集　部屋一覧の選択を複製
        public ICommand RentLivingEditSectionDuplicateCommand { get; }
        public bool RentLivingEditSectionDuplicateCommand_CanExecute()
        {
            if (RentLivingEditSectionSelectedItem != null)
                return true;
            else
                return false;
        }
        public void RentLivingEditSectionDuplicateCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionSelectedItem == null) return;

            //
            RentLivingEditSectionNew = new RentLivingSection(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Guid.NewGuid().ToString());
            RentLivingEditSectionNew.IsNew = true;
            RentLivingEditSectionNew.IsDirty = false;

            RentLivingEditSectionNew.RentLivingSectionRoomNumber = RentLivingEditSectionSelectedItem.RentLivingSectionRoomNumber + "の複製";
            RentLivingEditSectionNew.RentLivingSectionMadori = RentLivingEditSectionSelectedItem.RentLivingSectionMadori;
            RentLivingEditSectionNew.RentLivingSectionPrice = RentLivingEditSectionSelectedItem.RentLivingSectionPrice;
            // TODO: more to come

            // 追加
            RentLivingEdit.RentLivingSections.Add(RentLivingEditSectionNew);
        }

        // RL編集　部屋一覧の選択を削除
        public ICommand RentLivingEditSectionDeleteCommand { get; }
        public bool RentLivingEditSectionDeleteCommand_CanExecute()
        {
            if (RentLivingEditSectionSelectedItem != null)
                return true;
            else
                return false;
        }
        public void RentLivingEditSectionDeleteCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionSelectedItem == null) return;

            //RentLivingSectionToBeDeletedIDs
            if (RentLivingEditSectionSelectedItem.IsNew)
            {
                // 新規なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
            }
            else
            {
                // DBからも削除するために、削除リストに追加（後で削除）
                RentLivingEdit.RentLivingSectionToBeDeletedIDs.Add(RentLivingEditSectionSelectedItem.RentLivingSection_ID);
            }

            // 削除
            RentLivingEdit.RentLivingSections.Remove(RentLivingEditSectionSelectedItem);
        }


        // RL編集　新規部屋の画像追加
        public ICommand RentLivingEditSectionNewPictureAddCommand { get; }
        public bool RentLivingEditSectionNewPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionNewPictureAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionNew == null) return;

            var files = _openDialogService.GetOpenPictureFileDialog("部屋の写真追加");

            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 画像データの読み込み
                            byte[] ImageData;
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            //BinaryReader br = new BinaryReader(fs);
                            //ImageData = br.ReadBytes((int)fs.Length);
                            //br.Close();
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                            ImageData = ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = FixedSize(img, 200, 150);
                            byte[] ImageThumbData = ImageToByteArray(thumbImg);


                            RentLivingSectionPicture rlpic = new RentLivingSectionPicture(RentLivingEditSectionNew.Rent_ID, RentLivingEditSectionNew.RentLiving_ID, RentLivingEditSectionNew.RentLivingSection_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsNew = true;
                            rlpic.IsModified = false;

                            rlpic.Picture = BitmapImageFromImage(thumbImg, FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingEditSectionNew.RentLivingSectionPictures.Add(rlpic);


                            fs.Close();
                        }
                        else
                        {
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "File";
                            er.ErrCode = 0;
                            er.ErrText = "「" + "File Does Not Exist" + "」";
                            er.ErrDescription = fileName + " ファイルが存在しません。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingEditSectionNewPictureAddCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // RL編集　新規部屋の画像削除
        public ICommand RentLivingEditSectionNewPictureDeleteCommand { get; }
        public bool RentLivingEditSectionNewPictureDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionNewPictureDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionNew == null) return;

            // 選択アイテム保持用
            List<RentLivingSectionPicture> selectedList = new List<RentLivingSectionPicture>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingSectionPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingSectionPicture);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                RentLivingEditSectionNew.RentLivingSectionPictures.Remove(item);

                // 新規部屋なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
            }
        }

        // RL編集　編集部屋の画像追加
        public ICommand RentLivingEditSectionEditPictureAddCommand { get; }
        public bool RentLivingEditSectionEditPictureAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionEditPictureAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionEdit == null) return;

            var files = _openDialogService.GetOpenPictureFileDialog("部屋の写真追加");

            if (files != null)
            {
                foreach (String filePath in files)
                {
                    string fileName = filePath.Trim();

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        if (fi.Exists)
                        {
                            // 画像データの読み込み
                            byte[] ImageData;
                            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                            //BinaryReader br = new BinaryReader(fs);
                            //ImageData = br.ReadBytes((int)fs.Length);
                            //br.Close();
                            System.Drawing.Image img = System.Drawing.Image.FromStream(fs, false, false); // 検証なしが早い。https://www.atmarkit.co.jp/ait/articles/0706/07/news139.html

                            ImageData = ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = FixedSize(img, 200, 150);
                            byte[] ImageThumbData = ImageToByteArray(thumbImg);


                            RentLivingSectionPicture rlpic = new RentLivingSectionPicture(RentLivingEditSectionEdit.Rent_ID, RentLivingEditSectionEdit.RentLiving_ID, RentLivingEditSectionEdit.RentLivingSection_ID, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsModified = false;
                            rlpic.IsNew = true;

                            rlpic.Picture = BitmapImageFromImage(thumbImg, FileExtToImageFormat(rlpic.PictureFileExt));

                            RentLivingEditSectionEdit.RentLivingSectionPictures.Add(rlpic);


                            fs.Close();
                        }
                        else
                        {
                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "File";
                            er.ErrCode = 0;
                            er.ErrText = "「" + "File Does Not Exist" + "」";
                            er.ErrDescription = fileName + " ファイルが存在しません。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingEditSectionEditPictureAddCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // RL編集　編集部屋の画像削除
        public ICommand RentLivingEditSectionEditPictureDeleteCommand { get; }
        public bool RentLivingEditSectionEditPictureDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditSectionEditPictureDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;
            if (RentLivingEditSectionEdit == null) return;

            // 選択アイテム保持用
            List<RentLivingSectionPicture> selectedList = new List<RentLivingSectionPicture>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingSectionPicture>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingSectionPicture);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                if (item.IsNew)
                {
                    // 新規画像なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
                }
                else
                {
                    // DBからも削除するために、削除リストに追加（後で削除）
                    RentLivingEditSectionEdit.RentLivingSectionPicturesToBeDeletedIDs.Add(item.RentSectionPicture_ID);
                }

                // 一覧から削除
                RentLivingEditSectionEdit.RentLivingSectionPictures.Remove(item);
            }

            
        }


        // RL編集　物件の図面PDF追加
        public ICommand RentLivingEditZumenPdfAddCommand { get; }
        public bool RentLivingEditZumenPdfAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditZumenPdfAddCommand_Execute()
        {
            if (RentLivingEdit == null) return;

            string fileName = _openDialogService.GetOpenZumenPdfFileDialog("図面の追加");

            if (!string.IsNullOrEmpty(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Exists)
                {
                    // 図面ファイルのPDFデータの読み込み
                    byte[] PdfData;
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    long len = fs.Length;

                    BinaryReader br = new BinaryReader(fs);
                    PdfData = br.ReadBytes((int)fs.Length);
                    br.Close();

                    RentLivingZumenPDF rlZumen = new RentLivingZumenPDF(RentLivingEdit.Rent_ID, RentLivingEdit.RentLiving_ID, Guid.NewGuid().ToString());
                    rlZumen.PDFData = PdfData;
                    rlZumen.FileSize = len;

                    // TODO:
                    //rlZumen.DateTimeAdded = DateTime.Now;
                    rlZumen.DateTimePublished = DateTime.Now;
                    rlZumen.DateTimeVerified = DateTime.Now;

                    rlZumen.IsDirty = false;
                    rlZumen.IsNew = true;

                    RentLivingEdit.RentLivingZumenPDFs.Add(rlZumen);

                    fs.Close();
                }
                else
                {
                    // エラーイベント発火
                    MyError er = new MyError();
                    er.ErrType = "File";
                    er.ErrCode = 0;
                    er.ErrText = "「" + "File Does Not Exist" + "」";
                    er.ErrDescription = fileName + " ファイルが存在しません。";
                    er.ErrDatetime = DateTime.Now;
                    er.ErrPlace = "MainViewModel::RentLivingEditZumenPdfAddCommand_Execute()";
                    ErrorOccured?.Invoke(er);
                }
            }
        }

        // RL編集　物件の図面PDF削除
        public ICommand RentLivingEditZumenPdfDeleteCommand { get; }
        public bool RentLivingEditZumenPdfDeleteCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditZumenPdfDeleteCommand_Execute(object obj)
        {
            if (obj == null) return;

            if (RentLivingEdit == null) return;

            // 選択アイテム保持用
            List<RentLivingZumenPDF> selectedList = new List<RentLivingZumenPDF>();

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingZumenPDF>();

            foreach (var item in collection)
            {
                // 削除リストに追加
                selectedList.Add(item as RentLivingZumenPDF);
            }

            // 選択注文アイテムをループして、アイテムを削除する
            foreach (var item in selectedList)
            {
                if (item.IsNew)
                {
                    // 新規なので、DBにはまだ保存されていないはずなので、DBから削除する処理は不要。
                }
                else
                {
                    // DBからも削除するために、削除リストに追加（後で削除）
                    RentLivingEdit.RentLivingZumenPdfToBeDeletedIDs.Add(item.RentZumenPDF_ID);
                }

                // 一覧から削除
                RentLivingEdit.RentLivingZumenPDFs.Remove(item);
            }

        }

        // RL編集　物件の図面PDF表示
        public ICommand RentLivingEditZumenPdfShowCommand { get; }
        public bool RentLivingEditZumenPdfShowCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditZumenPdfShowCommand_Execute(object obj)
        {
            if (obj == null) return;
            if (RentLivingEdit == null) return;

            // System.Windows.Controls.SelectedItemCollection をキャストして、ループ
            System.Collections.IList items = (System.Collections.IList)obj;
            var collection = items.Cast<RentLivingZumenPDF>();

            foreach (var item in collection)
            {

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT PdfData FROM RentLivingZumenPdf WHERE RentLivingZumenPdf_ID = '{0}'", (item as RentLivingZumenPDF).RentZumenPDF_ID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                byte[] pdfBytes = (byte[])reader["PdfData"];

                                File.WriteAllBytes(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf", pdfBytes);

                                Process.Start(new ProcessStartInfo(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf") { UseShellExecute = true });

                                break;
                            }
                        }
                    }
                }

                break;

            }
        }

        // RL編集　物件の図面PDF表示（ダブルクリックやエンター押下で）
        public ICommand RentLivingEditZumenPdfEnterCommand { get; }
        public bool RentLivingEditZumenPdfEnterCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditZumenPdfEnterCommand_Execute(RentZumenPDF obj)
        {
            if (obj == null) return;
            if (RentLivingEdit == null) return;

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = String.Format("SELECT PdfData FROM RentLivingZumenPdf WHERE RentLivingZumenPdf_ID = '{0}'", (obj as RentZumenPDF).RentZumenPDF_ID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] pdfBytes = (byte[])reader["PdfData"];

                            File.WriteAllBytes(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf", pdfBytes);

                            Process.Start(new ProcessStartInfo(Path.GetTempPath() + Path.DirectorySeparatorChar + "temp.pdf") { UseShellExecute = true });

                            break;
                        }
                    }
                }
            }


        }



        #endregion

        #region == 元付け業者 ==

        // 業者　管理、検索
        public ICommand AgencySearchCommand { get; }
        public bool AgencySearchCommand_CanExecute()
        {
            if (String.IsNullOrEmpty(AgencySearchText))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void AgencySearchCommand_Execute()
        {
            // Firest, clear it.
            Agencies.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Agency WHERE Name Like '%" + AgencySearchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            Agency ag = new Agency(Convert.ToString(reader["Agency_ID"]));
                            ag.Name = Convert.ToString(reader["Name"]);
                            ag.Branch = Convert.ToString(reader["Branch"]);
                            ag.PostalCode = Convert.ToString(reader["PostalCode"]);
                            ag.Address = Convert.ToString(reader["Address"]);
                            ag.TelNumber = Convert.ToString(reader["TelNumber"]);
                            ag.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                            ag.Memo = Convert.ToString(reader["Memo"]);
                            
                            ag.IsNew = false;
                            ag.IsDirty = false;

                            Agencies.Add(ag);

                        }
                    }
                }
            }
        }

        // 業者　管理、一覧
        public ICommand AgencyListCommand { get; }
        public bool AgencyListCommand_CanExecute()
        {
            return true;
        }
        public void AgencyListCommand_Execute()
        {
            // Firest, clear it.
            Agencies.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM Agency";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                Agency ag = new Agency(Convert.ToString(reader["Agency_ID"]));
                                ag.Name = Convert.ToString(reader["Name"]);
                                ag.Branch = Convert.ToString(reader["Branch"]);
                                ag.PostalCode = Convert.ToString(reader["PostalCode"]);
                                ag.Address = Convert.ToString(reader["Address"]);
                                ag.TelNumber = Convert.ToString(reader["TelNumber"]);
                                ag.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                                ag.Memo = Convert.ToString(reader["Memo"]);

                                ag.IsNew = false;
                                ag.IsDirty = false;

                                Agencies.Add(ag);

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errmessage;
                if (e.InnerException != null)
                {
                    errmessage = e.InnerException.Message;
                    System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::AgencyListCommand_Execute()");
                }
                else
                {
                    errmessage = e.Message;
                    System.Diagnostics.Debug.WriteLine("Exception:'" + e.Message + "' @MainViewModel::AgencyListCommand_Execute()");
                }

                // エラーイベント発火
                MyError er = new MyError();
                er.ErrType = "DB";
                er.ErrCode = 0;
                er.ErrText = "「" + errmessage + "」";
                er.ErrDescription = "業者を一覧（SELECT）する処理でエラーが発生しました。";
                er.ErrDatetime = DateTime.Now;
                er.ErrPlace = "In " + e.Source + " from MainViewModel::AgencyListCommand_Execute()";
                ErrorOccured?.Invoke(er);
            }


        }

        // 業者　管理、一覧選択アイテム削除（DELETE）
        public ICommand AgencySelectedDeleteCommand { get; }
        public bool AgencySelectedDeleteCommand_CanExecute()
        {
            if (AgenciesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void AgencySelectedDeleteCommand_Execute()
        {
            if (AgenciesSelectedItem != null)
            {
                // 選択アイテムのデータを削除

                string sqlDelete = String.Format("DELETE FROM Agency WHERE Agency_ID = '{0}'", AgenciesSelectedItem.Agency_ID);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {

                            cmd.CommandText = sqlDelete;
                            var result = cmd.ExecuteNonQuery();

                            cmd.Transaction.Commit();

                            // 一覧から削除
                            if (Agencies.Remove(AgenciesSelectedItem))
                            {
                                AgenciesSelectedItem = null;
                            }

                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @AgencySelectedDeleteCommand_Execute()");

                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "データベースを更新する処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::AgencySelectedDeleteCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // 業者　追加（画面表示）
        public ICommand AgencyNewCommand { get; }
        public bool AgencyNewCommand_CanExecute()
        {
            return true;
        }
        public void AgencyNewCommand_Execute()
        {
            // Agencyオブジェクトを用意
            AgencyEdit = new Agency(Guid.NewGuid().ToString())
            {
                IsNew = true,
                IsDirty = false
            };

            ShowAgencyEdit = true;
        }

        // 業者　編集（画面表示）
        public ICommand AgencySelectedEditCommand { get; }
        public bool AgencySelectedEditCommand_CanExecute()
        {
            if (AgenciesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void AgencySelectedEditCommand_Execute()
        {
            if (AgenciesSelectedItem == null) return;

            // Agencyオブジェクトを設定
            AgencyEdit = AgenciesSelectedItem;

            ShowAgencyEdit = true;
        }

        // 業者　追加・更新のキャンセル
        public ICommand AgencyNewOrEditCancelCommand { get; }
        public bool AgencyNewOrEditCancelCommand_CanExecute()
        {
            return true;
        }
        public void AgencyNewOrEditCancelCommand_Execute()
        {
            // 編集を非表示に（閉じる）
            if (ShowAgencyEdit) ShowAgencyEdit = false;
        }

        // 業者　追加または更新処理(InsertOrUpdate)
        public ICommand AgencyInsertOrUpdateCommand { get; }
        public bool AgencyInsertOrUpdateCommand_CanExecute()
        {
            return true;
        }
        public void AgencyInsertOrUpdateCommand_Execute()
        {
            if (AgencyEdit == null) return;

            // TODO: 入力チェック

            if (AgencyEdit.IsNew)
            {
                // 新規追加
                try
                {
                    string sqlInsertIntoAgency = String.Format("INSERT INTO Agency (Agency_ID, Name, Branch, PostalCode, Address, TelNumber, FaxNumber, Memo) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                        AgencyEdit.Agency_ID, AgencyEdit.Name, AgencyEdit.Branch, AgencyEdit.PostalCode, AgencyEdit.Address, AgencyEdit.TelNumber, AgencyEdit.FaxNumber, AgencyEdit.Memo);

                    using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                    {
                        connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = connection.BeginTransaction();
                            try
                            {
                                // Agencyテーブルへ追加
                                cmd.CommandText = sqlInsertIntoAgency;
                                var Result = cmd.ExecuteNonQuery();
                                if (Result != 1)
                                {
                                    AgencyEdit.IsNew = false;
                                    AgencyEdit.IsDirty = false;
                                }

                                //　コミット
                                cmd.Transaction.Commit();

                                // 追加画面を非表示に（閉じる）
                                ShowAgencyEdit = false;
                            }
                            catch (Exception e)
                            {
                                // ロールバック
                                cmd.Transaction.Rollback();

                                // エラーイベント発火
                                MyError er = new MyError();
                                er.ErrType = "DB";
                                er.ErrCode = 0;
                                er.ErrText = "「" + e.Message + "」";
                                er.ErrDescription = "データベースに登録する処理でエラーが発生し、ロールバックしました。";
                                er.ErrDatetime = DateTime.Now;
                                er.ErrPlace = "MainViewModel::AgencyInsertOrUpdateCommand()";
                                ErrorOccured?.Invoke(er);
                            }
                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. TargetInvocationException@MainViewModel::AgencyInsertOrUpdateCommand()");
                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. InvalidOperationException@MainViewModel::AgencyInsertOrUpdateCommand()");
                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::AgencyInsertOrUpdateCommand()");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::AgencyInsertOrUpdateCommand()");
                    }
                }
            }
            else
            {
                // 更新

                if (AgenciesSelectedItem == null) return;

                // 編集オブジェクトに格納されている更新された情報をDBへ更新

                string sqlUpdateAgency = String.Format("UPDATE Agency SET Name = '{1}', Branch = '{2}', PostalCode = '{3}', Address = '{4}', TelNumber = '{5}', FaxNumber = '{6}', Memo = '{7}' WHERE Agency_ID = '{0}'",
                    AgencyEdit.Agency_ID, AgencyEdit.Name, AgencyEdit.Branch, AgencyEdit.PostalCode, AgencyEdit.Address, AgencyEdit.TelNumber, AgencyEdit.FaxNumber, AgencyEdit.Memo);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sqlUpdateAgency;
                            var result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                //AgencyEdit.IsNew = false;
                                AgencyEdit.IsDirty = false;
                            }

                            cmd.Transaction.Commit();


                            // 編集画面を非表示に
                            ShowAgencyEdit = false;
                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::AgencyInsertOrUpdateCommand_Execute()");

                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "元付け業者の編集更新 (UPDATE)でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::AgencyInsertOrUpdateCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }

        }

        #endregion

        #region == 管理会社 ==

        // 管理会社　検索
        public ICommand MaintenanceCompanySearchCommand { get; }
        public bool MaintenanceCompanySearchCommand_CanExecute()
        {
            if (String.IsNullOrEmpty(MaintenanceCompanySearchText))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void MaintenanceCompanySearchCommand_Execute()
        {
            // Firest, clear it.
            MaintenanceCompanies.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM MaintenanceCompany WHERE Name Like '%" + MaintenanceCompanySearchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            MaintenanceCompany mc = new MaintenanceCompany(Convert.ToString(reader["MaintenanceCompany_ID"]));
                            mc.Name = Convert.ToString(reader["Name"]);
                            mc.Branch = Convert.ToString(reader["Branch"]);
                            mc.PostalCode = Convert.ToString(reader["PostalCode"]);
                            mc.Address = Convert.ToString(reader["Address"]);
                            mc.TelNumber = Convert.ToString(reader["TelNumber"]);
                            mc.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                            mc.Memo = Convert.ToString(reader["Memo"]);

                            mc.IsNew = false;
                            mc.IsDirty = false;

                            MaintenanceCompanies.Add(mc);

                        }
                    }
                }
            }
        }

        // 管理会社　一覧
        public ICommand MaintenanceCompanyListCommand { get; }
        public bool MaintenanceCompanyListCommand_CanExecute()
        {
            return true;
        }
        public void MaintenanceCompanyListCommand_Execute()
        {
            // Firest, clear it.
            MaintenanceCompanies.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM MaintenanceCompany";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                MaintenanceCompany mc = new MaintenanceCompany(Convert.ToString(reader["MaintenanceCompany_ID"]));
                                mc.Name = Convert.ToString(reader["Name"]);
                                mc.Branch = Convert.ToString(reader["Branch"]);
                                mc.PostalCode = Convert.ToString(reader["PostalCode"]);
                                mc.Address = Convert.ToString(reader["Address"]);
                                mc.TelNumber = Convert.ToString(reader["TelNumber"]);
                                mc.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                                mc.Memo = Convert.ToString(reader["Memo"]);

                                mc.IsNew = false;
                                mc.IsDirty = false;

                                MaintenanceCompanies.Add(mc);

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errmessage;
                if (e.InnerException != null)
                {
                    errmessage = e.InnerException.Message;
                    System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::MaintenanceCompanyListCommand_Execute()");
                }
                else
                {
                    errmessage = e.Message;
                    System.Diagnostics.Debug.WriteLine("Exception:'" + e.Message + "' @MainViewModel::MaintenanceCompanyListCommand_Execute()");
                }

                // エラーイベント発火
                MyError er = new MyError();
                er.ErrType = "DB";
                er.ErrCode = 0;
                er.ErrText = "「" + errmessage + "」";
                er.ErrDescription = "管理会社を一覧（SELECT）する処理でエラーが発生しました。";
                er.ErrDatetime = DateTime.Now;
                er.ErrPlace = "In " + e.Source + " from MainViewModel::MaintenanceCompanyListCommand_Execute()";
                ErrorOccured?.Invoke(er);
            }


        }

        // 管理会社　一覧選択アイテム削除（DELETE）
        public ICommand MaintenanceCompanySelectedDeleteCommand { get; }
        public bool MaintenanceCompanySelectedDeleteCommand_CanExecute()
        {
            if (MaintenanceCompaniesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void MaintenanceCompanySelectedDeleteCommand_Execute()
        {
            if (MaintenanceCompaniesSelectedItem != null)
            {
                // 選択アイテムのデータを削除

                string sqlDelete = String.Format("DELETE FROM MaintenanceCompany WHERE MaintenanceCompany_ID = '{0}'", 
                    MaintenanceCompaniesSelectedItem.MaintenanceCompany_ID);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {

                            cmd.CommandText = sqlDelete;
                            var result = cmd.ExecuteNonQuery();

                            cmd.Transaction.Commit();

                            // 一覧から削除
                            if (MaintenanceCompanies.Remove(MaintenanceCompaniesSelectedItem))
                            {
                                MaintenanceCompaniesSelectedItem = null;
                            }

                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MaintenanceCompanySelectedDeleteCommand_Execute()");

                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "データベースを更新する処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::MaintenanceCompanySelectedDeleteCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // 管理会社　追加（画面表示）
        public ICommand MaintenanceCompanyNewCommand { get; }
        public bool MaintenanceCompanyNewCommand_CanExecute()
        {
            return true;
        }
        public void MaintenanceCompanyNewCommand_Execute()
        {
            // MaintenanceCompany
            MaintenanceCompanyEdit = new MaintenanceCompany(Guid.NewGuid().ToString())
            {
                IsNew = true,
                IsDirty = false
            };

            ShowMaintenanceCompanyEdit = true;
        }

        // 管理会社　編集（画面表示）
        public ICommand MaintenanceCompanySelectedEditCommand { get; }
        public bool MaintenanceCompanySelectedEditCommand_CanExecute()
        {
            if (MaintenanceCompaniesSelectedItem != null)
                return true;
            else
                return false;
        }
        public void MaintenanceCompanySelectedEditCommand_Execute()
        {
            if (MaintenanceCompaniesSelectedItem == null) return;

            // MaintenanceCompanyオブジェクトを設定
            MaintenanceCompanyEdit = MaintenanceCompaniesSelectedItem;

            ShowMaintenanceCompanyEdit = true;
        }

        // 管理会社　追加・更新のキャンセル
        public ICommand MaintenanceCompanyNewOrEditCancelCommand { get; }
        public bool MaintenanceCompanyNewOrEditCancelCommand_CanExecute()
        {
            return true;
        }
        public void MaintenanceCompanyNewOrEditCancelCommand_Execute()
        {
            // 編集を非表示に（閉じる）
            ShowMaintenanceCompanyEdit = false;
        }

        // 管理会社　追加または更新処理(InsertOrUpdate)
        public ICommand MaintenanceCompanyInsertOrUpdateCommand { get; }
        public bool MaintenanceCompanyInsertOrUpdateCommand_CanExecute()
        {
            return true;
        }
        public void MaintenanceCompanyInsertOrUpdateCommand_Execute()
        {
            if (MaintenanceCompanyEdit == null) return;

            // TODO: 入力チェック

            if (MaintenanceCompanyEdit.IsNew)
            {
                // 新規追加
                try
                {
                    string sqlInsertInto = String.Format("INSERT INTO MaintenanceCompany (MaintenanceCompany_ID, Name, Branch, PostalCode, Address, TelNumber, FaxNumber, Memo) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')",
                        MaintenanceCompanyEdit.MaintenanceCompany_ID, MaintenanceCompanyEdit.Name, MaintenanceCompanyEdit.Branch, MaintenanceCompanyEdit.PostalCode, MaintenanceCompanyEdit.Address, MaintenanceCompanyEdit.TelNumber, MaintenanceCompanyEdit.FaxNumber, MaintenanceCompanyEdit.Memo);

                    using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                    {
                        connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = connection.BeginTransaction();
                            try
                            {
                                // テーブルへ追加
                                cmd.CommandText = sqlInsertInto;
                                var Result = cmd.ExecuteNonQuery();
                                if (Result != 1)
                                {
                                    MaintenanceCompanyEdit.IsNew = false;
                                    MaintenanceCompanyEdit.IsDirty = false;
                                }

                                //　コミット
                                cmd.Transaction.Commit();

                                // 追加画面を非表示に（閉じる）
                                ShowMaintenanceCompanyEdit = false;
                            }
                            catch (Exception e)
                            {
                                // ロールバック
                                cmd.Transaction.Rollback();

                                // エラーイベント発火
                                MyError er = new MyError();
                                er.ErrType = "DB";
                                er.ErrCode = 0;
                                er.ErrText = "「" + e.Message + "」";
                                er.ErrDescription = "データベースに登録する処理でエラーが発生し、ロールバックしました。";
                                er.ErrDatetime = DateTime.Now;
                                er.ErrPlace = "MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()";
                                ErrorOccured?.Invoke(er);
                            }
                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. TargetInvocationException@MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");
                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. InvalidOperationException@MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");
                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");
                    }
                }
            }
            else
            {
                // 更新

                if (MaintenanceCompaniesSelectedItem == null) return;

                // 編集オブジェクトに格納されている更新された情報をDBへ更新

                string sqlUpdate = String.Format("UPDATE MaintenanceCompany SET Name = '{1}', Branch = '{2}', PostalCode = '{3}', Address = '{4}', TelNumber = '{5}', FaxNumber = '{6}', Memo = '{7}' WHERE Agency_ID = '{0}'",
                    MaintenanceCompanyEdit.MaintenanceCompany_ID, MaintenanceCompanyEdit.Name, MaintenanceCompanyEdit.Branch, MaintenanceCompanyEdit.PostalCode, MaintenanceCompanyEdit.Address, MaintenanceCompanyEdit.TelNumber, MaintenanceCompanyEdit.FaxNumber, MaintenanceCompanyEdit.Memo);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sqlUpdate;
                            var result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                //MaintenanceCompanyEdit.IsNew = false;
                                MaintenanceCompanyEdit.IsDirty = false;
                            }

                            cmd.Transaction.Commit();


                            // 編集画面を非表示に
                            ShowMaintenanceCompanyEdit = false;
                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()");

                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "管理会社の編集更新 (UPDATE)でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::MaintenanceCompanyInsertOrUpdateCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        #endregion

        #region == オーナー ==

        // オーナー　検索
        public ICommand OwnerSearchCommand { get; }
        public bool OwnerSearchCommand_CanExecute()
        {
            if (String.IsNullOrEmpty(OwnerSearchText))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void OwnerSearchCommand_Execute()
        {
            // Firest, clear it.
            Owners.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Owner WHERE Name Like '%" + OwnerSearchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            Owner o = new Owner(Convert.ToString(reader["Owner_ID"]));
                            o.Name = Convert.ToString(reader["Name"]);
                            o.PostalCode = Convert.ToString(reader["PostalCode"]);
                            o.Address = Convert.ToString(reader["Address"]);
                            o.TelNumber = Convert.ToString(reader["TelNumber"]);
                            o.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                            o.Memo = Convert.ToString(reader["Memo"]);

                            o.IsNew = false;
                            o.IsDirty = false;

                            Owners.Add(o);

                        }
                    }
                }
            }
        }

        // オーナー　一覧
        public ICommand OwnerListCommand { get; }
        public bool OwnerListCommand_CanExecute()
        {
            return true;
        }
        public void OwnerListCommand_Execute()
        {
            // Firest, clear it.
            Owners.Clear();

            try
            {
                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM Owner";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {

                                Owner o = new Owner(Convert.ToString(reader["Owner_ID"]));
                                o.Name = Convert.ToString(reader["Name"]);
                                o.PostalCode = Convert.ToString(reader["PostalCode"]);
                                o.Address = Convert.ToString(reader["Address"]);
                                o.TelNumber = Convert.ToString(reader["TelNumber"]);
                                o.FaxNumber = Convert.ToString(reader["FaxNumber"]);
                                o.Memo = Convert.ToString(reader["Memo"]);

                                o.IsNew = false;
                                o.IsDirty = false;

                                Owners.Add(o);

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errmessage;
                if (e.InnerException != null)
                {
                    errmessage = e.InnerException.Message;
                    System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::OwnerListCommand_Execute()");
                }
                else
                {
                    errmessage = e.Message;
                    System.Diagnostics.Debug.WriteLine("Exception:'" + e.Message + "' @MainViewModel::OwnerListCommand_Execute()");
                }

                // エラーイベント発火
                MyError er = new MyError();
                er.ErrType = "DB";
                er.ErrCode = 0;
                er.ErrText = "「" + errmessage + "」";
                er.ErrDescription = "オーナーを一覧（SELECT）する処理でエラーが発生しました。";
                er.ErrDatetime = DateTime.Now;
                er.ErrPlace = "In " + e.Source + " from MainViewModel::OwnerListCommand_Execute()";
                ErrorOccured?.Invoke(er);
            }


        }

        // オーナー　一覧選択アイテム削除（DELETE）
        public ICommand OwnerSelectedDeleteCommand { get; }
        public bool OwnerSelectedDeleteCommand_CanExecute()
        {
            if (OwnersSelectedItem != null)
                return true;
            else
                return false;
        }
        public void OwnerSelectedDeleteCommand_Execute()
        {
            if (OwnersSelectedItem != null)
            {
                // 選択アイテムのデータを削除

                string sqlDelete = String.Format("DELETE FROM Owner WHERE Owner_ID = '{0}'",
                    OwnersSelectedItem.Owner_ID);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {

                            cmd.CommandText = sqlDelete;
                            var result = cmd.ExecuteNonQuery();

                            cmd.Transaction.Commit();

                            // 一覧から削除
                            if (Owners.Remove(OwnersSelectedItem))
                            {
                                OwnersSelectedItem = null;
                            }

                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @OwnerSelectedDeleteCommand_Execute()");

                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "データベースを更新する処理でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::OwnerSelectedDeleteCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // オーナー　追加（画面表示）
        public ICommand OwnerNewCommand { get; }
        public bool OwnerNewCommand_CanExecute()
        {
            return true;
        }
        public void OwnerNewCommand_Execute()
        {
            // Owner
            OwnerEdit = new Owner(Guid.NewGuid().ToString())
            {
                IsNew = true,
                IsDirty = false
            };

            ShowOwnerEdit = true;
        }

        // オーナー　編集（画面表示）
        public ICommand OwnerSelectedEditCommand { get; }
        public bool OwnerSelectedEditCommand_CanExecute()
        {
            if (OwnersSelectedItem != null)
                return true;
            else
                return false;
        }
        public void OwnerSelectedEditCommand_Execute()
        {
            if (OwnersSelectedItem == null) return;

            // Owner
            OwnerEdit = OwnersSelectedItem;

            ShowOwnerEdit = true;
        }

        // オーナー　追加・更新のキャンセル
        public ICommand OwnerNewOrEditCancelCommand { get; }
        public bool OwnerNewOrEditCancelCommand_CanExecute()
        {
            return true;
        }
        public void OwnerNewOrEditCancelCommand_Execute()
        {
            // 編集を非表示に（閉じる）
            ShowOwnerEdit = false;
        }

        // オーナー　追加または更新処理(InsertOrUpdate)
        public ICommand OwnerInsertOrUpdateCommand { get; }
        public bool OwnerInsertOrUpdateCommand_CanExecute()
        {
            return true;
        }
        public void OwnerInsertOrUpdateCommand_Execute()
        {
            if (OwnerEdit == null) return;

            // TODO: 入力チェック

            if (OwnerEdit.IsNew)
            {
                // 新規追加
                try
                {
                    string sqlInsertInto = String.Format("INSERT INTO Owner (Owner_ID, Name, PostalCode, Address, TelNumber, FaxNumber, Memo) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                        OwnerEdit.Owner_ID, OwnerEdit.Name, OwnerEdit.PostalCode, OwnerEdit.Address, OwnerEdit.TelNumber, OwnerEdit.FaxNumber, OwnerEdit.Memo);

                    using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                    {
                        connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.Transaction = connection.BeginTransaction();
                            try
                            {
                                // テーブルへ追加
                                cmd.CommandText = sqlInsertInto;
                                var Result = cmd.ExecuteNonQuery();
                                if (Result != 1)
                                {
                                    OwnerEdit.IsNew = false;
                                    OwnerEdit.IsDirty = false;
                                }

                                //　コミット
                                cmd.Transaction.Commit();

                                // 追加画面を非表示に（閉じる）
                                ShowOwnerEdit = false;
                            }
                            catch (Exception e)
                            {
                                // ロールバック
                                cmd.Transaction.Rollback();

                                // エラーイベント発火
                                MyError er = new MyError();
                                er.ErrType = "DB";
                                er.ErrCode = 0;
                                er.ErrText = "「" + e.Message + "」";
                                er.ErrDescription = "データベースに登録する処理でエラーが発生し、ロールバックしました。";
                                er.ErrDatetime = DateTime.Now;
                                er.ErrPlace = "MainViewModel::OwnerInsertOrUpdateCommand_Execute()";
                                ErrorOccured?.Invoke(er);
                            }
                        }
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. TargetInvocationException@MainViewModel::OwnerInsertOrUpdateCommand_Execute()");
                    throw ex.InnerException;
                }
                catch (System.InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine("Opps. InvalidOperationException@MainViewModel::OwnerInsertOrUpdateCommand_Execute()");
                    throw ex.InnerException;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine(e.InnerException.Message + " @MainViewModel::OwnerInsertOrUpdateCommand_Execute()");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::OwnerInsertOrUpdateCommand_Execute()");
                    }
                }
            }
            else
            {
                // 更新

                if (OwnersSelectedItem == null) return;

                // 編集オブジェクトに格納されている更新された情報をDBへ更新

                string sqlUpdate = String.Format("UPDATE Owner SET Name = '{1}', PostalCode = '{2}', Address = '{3}', TelNumber = '{4}', FaxNumber = '{5}', Memo = '{6}' WHERE Owner_ID = '{0}'",
                    OwnerEdit.Owner_ID, OwnerEdit.Name, OwnerEdit.PostalCode, OwnerEdit.Address, OwnerEdit.TelNumber, OwnerEdit.FaxNumber, OwnerEdit.Memo);

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();
                        try
                        {
                            cmd.CommandText = sqlUpdate;
                            var result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                //OwnerEdit.IsNew = false;
                                OwnerEdit.IsDirty = false;
                            }

                            cmd.Transaction.Commit();


                            // 編集画面を非表示に
                            ShowOwnerEdit = false;
                        }
                        catch (Exception e)
                        {
                            cmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::OwnerInsertOrUpdateCommand_Execute()");

                            // エラーイベント発火
                            MyError er = new MyError();
                            er.ErrType = "DB";
                            er.ErrCode = 0;
                            er.ErrText = "「" + e.Message + "」";
                            er.ErrDescription = "オーナーの編集更新 (UPDATE)でエラーが発生し、ロールバックしました。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::OwnerInsertOrUpdateCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        #endregion

        #region == エラー通知画面周り ==

        // エラー通知画面のクローズ
        public ICommand CloseErrorCommand { get; }
        public bool CloseErrorCommand_CanExecute()
        {
            return true;
        }
        public void CloseErrorCommand_Execute()
        {
            ShowErrorDialog = false;
        }

        #endregion

        #endregion

    }
}
