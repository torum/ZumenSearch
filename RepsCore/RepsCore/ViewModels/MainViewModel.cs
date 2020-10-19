using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Input;
using System.IO;
using System.ComponentModel;
using RepsCore.Common;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Media;
using System.Windows.Threading;
//using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using System.Windows.Media.Imaging;

/// TODO:
/// 部屋（複数）を追加・編集・削除する機能
/// 条件検索
/// 建物にPDFのファイルを追加
/// 入力補助（住所・〒・Geo）
/// 元付け・オーナー管理
/// エラー処理、及びログ保存


/// 履歴：
/// エラーイベントとエラー表示機能の実装。
/// 画像ファイルの追加。
/// 


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

    #endregion

    /// <summary>
    /// 賃貸物件のRent・Section、及び派生クラス
    /// </summary>
    #region == Rent・Section、及びその派生クラス ==

    /// <summary>
    /// 物件の（基底）クラス
    /// </summary>
    public class Rent : ViewModelBase
    {
        // GUID and Primary Key
        private string _rent_id;
        public string Rent_ID
        {
            get
            {
                return _rent_id;
            }
            set
            {
                if (_rent_id == value) return;

                _rent_id = value;
                this.NotifyPropertyChanged("Rent_ID");
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

        private RentTypes _Type;
        public RentTypes Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (_Type == value) return;

                _Type = value;
                this.NotifyPropertyChanged("Type");
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
    /// 賃貸住居用物件のクラス（建物）
    /// </summary>
    public class RentLiving : Rent
    {
        private string _rentLiving_ID;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_ID;
            }
            set
            {
                if (_rentLiving_ID == value) return;

                _rentLiving_ID = value;
                this.NotifyPropertyChanged("RentLiving_ID");
            }
        }

        private string _rentLivingPicture_ID; //TODO temp
        public string RentLivingPicture_ID
        {
            get
            {
                return _rentLivingPicture_ID;
            }
            set
            {
                if (_rentLivingPicture_ID == value) return;

                _rentLivingPicture_ID = value;
                this.NotifyPropertyChanged("RentLivingPicture_ID");
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

    }

    /// <summary>
    /// 賃貸住居用物件の編集用クラス
    /// </summary>
    public class RentLivingEditing: RentLiving
    {
        private string _pictureFilePath;
        public string PictureFilePath
        {
            get
            {
                return _pictureFilePath;
            }
            set
            {
                if (_pictureFilePath == value) return;

                _pictureFilePath = value;
                this.NotifyPropertyChanged("PictureFilePath");
            }
        }
    }

    /// <summary>
    /// 賃貸住居用物件のクラス（部屋）
    /// </summary>
    public class RentLivingSection : ViewModelBase
    {
        private string _rentLivingSection_ID;
        public string RentLivingSection_ID
        {
            get
            {
                return _rentLivingSection_ID;
            }
            set
            {
                if (_rentLivingSection_ID == value) return;

                _rentLivingSection_ID = value;
                this.NotifyPropertyChanged("RentLivingSection_ID");
            }
        }

        private string _rentLiving_ID;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_ID;
            }
            set
            {
                if (_rentLiving_ID == value) return;

                _rentLiving_ID = value;
                this.NotifyPropertyChanged("RentLiving_ID");
            }
        }

        private string _rent_ID;
        public string Rent_ID
        {
            get
            {
                return _rent_ID;
            }
            set
            {
                if (_rent_ID == value) return;

                _rent_ID = value;
                this.NotifyPropertyChanged("Rent_ID");
            }
        }

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

        #region == データベース ==

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

        // 賃貸物件住居用　新規追加用のクラスオブジェクト
        private RentLivingEditing _rentLivingNew = new RentLivingEditing();
        public RentLivingEditing RentLivingNew
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
        private RentLivingEditing _rentLivingEdit = new RentLivingEditing();
        public RentLivingEditing RentLivingEdit
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

        // 賃貸物件住居用　編集一覧用のコレクション
        private ObservableCollection<RentLiving> _editRents = new ObservableCollection<RentLiving>();
        public ObservableCollection<RentLiving> EditRents
        {
            get { return this._editRents; }
        }

        #endregion

        #region == 表示切替のフラグ ==

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

        // RL部屋新規追加画面の表示フラグ
        private bool _showRentLivingSectionNew = false;
        public bool ShowRentLivingSectionNew
        {
            get
            {
                return _showRentLivingSectionNew;
            }
            set
            {
                if (_showRentLivingSectionNew == value) return;

                _showRentLivingSectionNew = value;
                this.NotifyPropertyChanged("ShowRentLivingSectionNew");
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

                    ShowRentLivingSearchList = false;
                    ShowRentLivingNew = false;
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

        // 賃貸住居用編集画面の検索結果一覧リストビューの選択されたオブジェクトを保持
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

        #region == コマンドの宣言 ==

        // 新規賃貸住居用　物件追加（画面表示）
        public ICommand RentLivingNewCommand { get; }
        // 新規賃貸住居用　物件追加処理
        public ICommand RentLivingNewAddCommand { get; }
        // 新規賃貸住居用　物件追加キャンセル
        public ICommand RentLivingNewCancelCommand { get; }
        // 新規賃貸住居用　物件の部屋追加（画面表示）
        public ICommand RentLivingNewSectionCommand { get; }
        // 新規賃貸住居用　物件の部屋追加処理
        public ICommand RentLivingNewSectionAddCommand { get; }
        // 新規賃貸住居用　物件の部屋追加キャンセル
        public ICommand RentLivingNewSectionCancelCommand { get; }
        // 賃貸住居用　物件管理、一覧
        public ICommand RentLivingEditListCommand { get; }
        // 賃貸住居用　物件管理、検索
        public ICommand RentLivingEditSearchCommand { get; }
        // 賃貸住居用　物件管理、選択編集（画面表示）
        public ICommand RentLivingEditSelectedEditCommand { get; }
        // 賃貸住居用　物件管理、選択編集、更新処理
        public ICommand RentLivingEditSelectedEditUpdateCommand { get; }
        // 賃貸住居用　物件管理、選択編集、更新キャンセル
        public ICommand RentLivingEditSelectedEditCancelCommand { get; }
        // 賃貸住居用　物件管理、選択編集、PDF表示
        public ICommand RentLivingEditSelectedViewCommand { get; }
        // 賃貸住居用　物件管理、選択編集、削除
        public ICommand RentLivingEditSelectedDeleteCommand { get; }
        // エラー通知画面のクローズ
        public ICommand CloseErrorCommand { get; }

        #endregion

        /// <summary>
        /// メインのビューモデルのコンストラクタ
        /// </summary>
        public MainViewModel()
        {
            // エラーイベントにサブスクライブ
            ErrorOccured += new MyErrorEvent(OnError);

            #region == コマンドのイニシャライズ ==

            // 賃貸住居用新規物件
            RentLivingNewCommand = new RelayCommand(RentLivingNewCommand_Execute, RentLivingNewCommand_CanExecute);
            RentLivingNewAddCommand = new RelayCommand(RentLivingNewAddCommand_Execute, RentLivingNewAddCommand_CanExecute);
            RentLivingNewCancelCommand = new RelayCommand(RentLivingNewCancelCommand_Execute, RentLivingNewCancelCommand_CanExecute);
            // 賃貸住居用新規部屋
            RentLivingNewSectionCommand = new RelayCommand(RentLivingNewSectionCommand_Execute, RentLivingNewSectionCommand_CanExecute);
            RentLivingNewSectionAddCommand = new RelayCommand(RentLivingNewSectionAddCommand_Execute, RentLivingNewSectionAddCommand_CanExecute);
            RentLivingNewSectionCancelCommand = new RelayCommand(RentLivingNewSectionCancelCommand_Execute, RentLivingNewSectionCancelCommand_CanExecute);
            // 賃貸住居用管理一覧
            RentLivingEditListCommand = new RelayCommand(RentLivingEditListCommand_Execute, RentLivingEditListCommand_CanExecute);
            // 賃貸住居用管理検索
            RentLivingEditSearchCommand = new RelayCommand(RentLivingEditSearchCommand_Execute, RentLivingEditSearchCommand_CanExecute);
            // 賃貸住居用管理選択編集
            RentLivingEditSelectedEditCommand = new RelayCommand(RentLivingEditSelectedEditCommand_Execute, RentLivingEditSelectedEditCommand_CanExecute);
            RentLivingEditSelectedEditUpdateCommand = new RelayCommand(RentLivingEditSelectedEditUpdateCommand_Execute, RentLivingEditSelectedEditUpdateCommand_CanExecute);
            RentLivingEditSelectedEditCancelCommand = new RelayCommand(RentLivingEditSelectedEditCancelCommand_Execute, RentLivingEditSelectedEditCancelCommand_CanExecute);
            // 賃貸住居用管理選択表示
            RentLivingEditSelectedViewCommand = new RelayCommand(RentLivingEditSelectedViewCommand_Execute, RentLivingEditSelectedViewCommand_CanExecute);
            // 賃貸住居用管理選択削除
            RentLivingEditSelectedDeleteCommand = new RelayCommand(RentLivingEditSelectedDeleteCommand_Execute, RentLivingEditSelectedDeleteCommand_CanExecute);
            // エラー通知画面
            CloseErrorCommand = new RelayCommand(CloseErrorCommand_Execute, CloseErrorCommand_CanExecute);
            
            #endregion

            #region == SQLite DB のイニシャライズ ==

            // DB file path のセット
            _dataBaseFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + _appName + ".database";

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
                                "PostalCode TEXT NOT NULL," +
                                "Location TEXT NOT NULL," +
                                "LocationHiddenPart TEXT," +
                                "GeoLocationLatitude TEXT," +
                                "GeoLocationLongitude TEXT," +
                                "TrainStation1 TEXT," +
                                "TrainStation2 TEXT)";
                            tableCmd.ExecuteNonQuery();

                            // 賃貸住居用物件の「建物」テーブル
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLiving (" +
                                "RentLiving_ID TEXT NOT NULL PRIMARY KEY," +
                                "Rent_ID TEXT NOT NULL," +
                                "Kind TEXT NOT NULL," +
                                "Floors INTEGER NOT NULL," +
                                "FloorsBasement INTEGER," +
                                "BuiltYear INTEGER NOT NULL," +
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
                            tableCmd.CommandText = "CREATE TABLE IF NOT EXISTS RentLivingPdf (" +
                                "RentLivingPicture_ID TEXT NOT NULL PRIMARY KEY," +
                                "RentLiving_ID TEXT NOT NULL," +
                                "Rent_ID TEXT NOT NULL," +
                                "PdfData BLOB NOT NULL," +
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
                                "FOREIGN KEY (Rent_ID) REFERENCES Rent(Rent_ID)," +
                                "FOREIGN KEY (RentLiving_ID) REFERENCES RentLiving(RentLiving_ID)," +
                                "FOREIGN KEY (RentLivingSection_ID) REFERENCES RentLivingListing(RentLivingSection_ID)" +
                                " )";
                            tableCmd.ExecuteNonQuery();

                            // トランザクションコミット
                            tableCmd.Transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            // トランザクションのロールバック
                            tableCmd.Transaction.Rollback();

                            System.Diagnostics.Debug.WriteLine(e.Message);
                            //TODO 
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

        #endregion

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

        #endregion

        #region == コマンドの実装 ==

        // 賃貸住居用物件、追加画面表示
        public bool RentLivingNewCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewCommand_Execute()
        {
            // RentNewオブジェクトをクリアというかイニシャライズ
            RentLivingNew.Rent_ID = Guid.NewGuid().ToString();
            RentLivingNew.Type = RentTypes.RentLiving;
            RentLivingNew.Name = "";
            RentLivingNew.PostalCode = "";
            RentLivingNew.Location = "";
            RentLivingNew.TrainStation1 = "";
            RentLivingNew.TrainStation2 = "";

            RentLivingNew.RentLiving_ID = Guid.NewGuid().ToString();
            // TODO: 残りもクリアする。

            if (!ShowRentLivingNew) ShowRentLivingNew = true;

        }

        // 賃貸住居用物件、追加 (INSERT)
        public bool RentLivingNewAddCommand_CanExecute()
        {
            // 物件名必須
            if (string.IsNullOrEmpty(RentLivingNew.Name))
                return false;
            else
                return true;
        }
        public void RentLivingNewAddCommand_Execute()
        {
            // TODO: 入力チェック


            // 写真画像の追加があるかどうかチェック
            bool isPicturePresent = false;
            if (!string.IsNullOrEmpty(RentLivingNew.PictureFilePath))
                isPicturePresent = true;

            if (isPicturePresent)
            {
                string fileName = RentLivingNew.PictureFilePath.Trim();

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
                    System.Drawing.Image thumbImg = img.GetThumbnailImage(200, 150, () => false, IntPtr.Zero);
                    byte[] ImageThumbData = ImageToByteArray(thumbImg);

                    fs.Close();

                    RentLivingNew.PictureData = ImageData;
                    RentLivingNew.PictureThumbW200xData = ImageThumbData;
                    RentLivingNew.PictureFileExt = fi.Extension;
                }
                else
                {
                    isPicturePresent = false;

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

            try
            {
                // 念のため？
                RentLivingNew.Rent_ID = Guid.NewGuid().ToString();
                RentLivingNew.Type = RentTypes.RentLiving;

                RentLivingNew.RentLiving_ID = Guid.NewGuid().ToString();
                RentLivingNew.RentLivingPicture_ID = Guid.NewGuid().ToString();

                string sqlInsertIntoRent = String.Format("INSERT INTO Rent (Rent_ID, Name, Type, PostalCode, Location, TrainStation1, TrainStation2) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')",
                    RentLivingNew.Rent_ID, RentLivingNew.Name, RentLivingNew.Type.ToString(), RentLivingNew.PostalCode, RentLivingNew.Location, RentLivingNew.TrainStation1, RentLivingNew.TrainStation2);

                string sqlInsertIntoRentLiving = String.Format("INSERT INTO RentLiving (RentLiving_ID, Rent_ID, Kind, Floors, FloorsBasement, BuiltYear) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                    RentLivingNew.RentLiving_ID, RentLivingNew.Rent_ID, RentLivingNew.Kind.ToString(), RentLivingNew.Floors, RentLivingNew.FloorsBasement, RentLivingNew.BuiltYear);

                string sqlInsertIntoRentLivingPicture = String.Format("INSERT INTO RentLivingPicture (RentLivingPicture_ID, RentLiving_ID, Rent_ID, PictureData, PictureThumbW200xData, PictureFileExt) VALUES ('{0}', '{1}', '{2}', @0, @1, '{5}')",
                    RentLivingNew.RentLivingPicture_ID, RentLivingNew.RentLiving_ID, RentLivingNew.Rent_ID, RentLivingNew.PictureData, RentLivingNew.PictureThumbW200xData, RentLivingNew.PictureFileExt);

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

                            if (isPicturePresent)
                            {
                                // 物件画像の追加
                                cmd.CommandText = sqlInsertIntoRentLivingPicture;

                                SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                parameter1.Value = RentLivingNew.PictureData;
                                cmd.Parameters.Add(parameter1);

                                SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                parameter2.Value = RentLivingNew.PictureThumbW200xData;
                                cmd.Parameters.Add(parameter2);

                                var InsertIntoRentLivingPictureResult = cmd.ExecuteNonQuery();
                                if (InsertIntoRentLivingResult != 1)
                                {
                                    // これいる?
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

                            System.Diagnostics.Debug.WriteLine(e.Message + " @MainViewModel::RentLivingNewAddCommand_Execute()");

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

        // 賃貸住居用物件、追加キャンセル
        public bool RentLivingNewCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewCancelCommand_Execute()
        {
            // 編集を非表示に（閉じる）
            if (ShowRentLivingNew) ShowRentLivingNew = false;

            // TODO RentNew をクリアする。
        }

        // 賃貸住居用物件、部屋の追加画面表示
        public bool RentLivingNewSectionCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionCommand_Execute()
        {
            ShowRentLivingSectionNew = true;
        }

        // 賃貸住居用物件、部屋の追加 (INSERT)
        public bool RentLivingNewSectionAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionAddCommand_Execute()
        {

        }

        // 賃貸住居用物件、部屋の追加キャンセル
        public bool RentLivingNewSectionCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionCancelCommand_Execute()
        {
            ShowRentLivingSectionNew = false;
        }

        // 賃貸住居用物件、一覧
        public bool RentLivingEditListCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditListCommand_Execute()
        {
            // Firest, clear it.
            _editRents.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    // TODO JOIN
                    cmd.CommandText = "SELECT * FROM Rent";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            RentLiving rl = new RentLiving();
                            rl.Rent_ID = Convert.ToString(reader["Rent_ID"]);
                            rl.Name = Convert.ToString(reader["Name"]);
                            rl.Type = rl.StringToRentType[Convert.ToString(reader["Type"])];
                            rl.PostalCode = Convert.ToString(reader["PostalCode"]);
                            rl.Location = Convert.ToString(reader["Location"]);
                            rl.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                            rl.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                            _editRents.Add(rl);

                        }
                    }
                }
            }
        }

        // 賃貸住居用物件、検索
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
            _editRents.Clear();

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    // TODO JOIN
                    cmd.CommandText = "SELECT * FROM Rent WHERE Name Like '%" + RentLivingEditSearchText + "%'";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            RentLiving rl = new RentLiving();
                            rl.Rent_ID = Convert.ToString(reader["Rent_ID"]);
                            rl.Name = Convert.ToString(reader["Name"]);
                            rl.Type = rl.StringToRentType[Convert.ToString(reader["Type"])];
                            rl.PostalCode = Convert.ToString(reader["PostalCode"]);
                            rl.Location = Convert.ToString(reader["Location"]);
                            rl.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                            rl.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                            _editRents.Add(rl);

                        }
                    }
                }
            }
        }

        // 賃貸住居用物件、選択アイテム編集表示
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
            // TODO RentLivingEditを一旦すべてクリアというか初期化しておきたい。
            RentLivingEdit.Picture = null;
            RentLivingEdit.PictureFilePath = "";

            if (RentLivingEditSelectedItem != null)
            {
                var rentid = RentLivingEditSelectedItem.Rent_ID;
                // 選択アイテムのデータを一旦編集オブジェクトに格納
                //RentLivingEdit.Rent_ID = RentLivingEditSelectedItem.Rent_ID;
                //RentLivingEdit.Name = RentLivingEditSelectedItem.Name;
                //RentLivingEdit.PostalCode = RentLivingEditSelectedItem.PostalCode;
                //RentLivingEdit.Location = RentLivingEditSelectedItem.Location;
                //RentLivingEdit.TrainStation1 = RentLivingEditSelectedItem.TrainStation1;
                //RentLivingEdit.TrainStation2 = RentLivingEditSelectedItem.TrainStation2;

                using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.Transaction = connection.BeginTransaction();

                        cmd.CommandText = String.Format("SELECT * FROM Rent INNER JOIN RentLiving ON Rent.Rent_ID = RentLiving.Rent_ID WHERE Rent.Rent_ID = '{0}'", rentid);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RentLivingEdit.Rent_ID = rentid;
                                RentLivingEdit.Name = Convert.ToString(reader["Name"]);
                                RentLivingEdit.Type = RentLivingEdit.StringToRentType[Convert.ToString(reader["Type"])];
                                RentLivingEdit.PostalCode = Convert.ToString(reader["PostalCode"]);
                                RentLivingEdit.Location = Convert.ToString(reader["Location"]);
                                RentLivingEdit.TrainStation1 = Convert.ToString(reader["TrainStation1"]);
                                RentLivingEdit.TrainStation2 = Convert.ToString(reader["TrainStation2"]);

                                RentLivingEdit.RentLiving_ID = Convert.ToString(reader["RentLiving_ID"]);
                                RentLivingEdit.Kind = RentLivingEdit.StringToRentLivingKind[Convert.ToString(reader["Kind"])];
                                RentLivingEdit.Floors = Convert.ToInt32(reader["Floors"]);
                                RentLivingEdit.FloorsBasement = Convert.ToInt32(reader["FloorsBasement"]);
                                RentLivingEdit.BuiltYear = Convert.ToInt32(reader["BuiltYear"]);

                            }
                        }

                        cmd.CommandText = String.Format("SELECT * FROM RentLivingPicture WHERE Rent_ID = '{0}'", rentid);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                RentLivingEdit.RentLivingPicture_ID = Convert.ToString(reader["RentLivingPicture_ID"]);

                                byte[] imageBytes = (byte[])reader["PictureData"];
                                RentLivingEdit.PictureData = imageBytes;

                                byte[] imageThumbBytes = (byte[])reader["PictureThumbW200xData"];
                                RentLivingEdit.PictureThumbW200xData = imageThumbBytes;
                                /*
                                byte[] imageBytes = new byte[0];
                                if (reader["PictureData"] != null && !Convert.IsDBNull(reader["PictureData"]))
                                {
                                    imageBytes = (byte[])(reader["PictureData"]);
                                }
                                */

                                RentLivingEdit.PictureFileExt = Convert.ToString(reader["PictureFileExt"]);

                                RentLivingEdit.Picture = BitmapImageFromBytes(imageBytes);
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

        // 賃貸住居用物件、選択アイテム編集更新（UPDATE）
        public bool RentLivingEditSelectedEditUpdateCommand_CanExecute()
        {
            if (RentLivingEditSelectedItem == null)
            {
                return false;
            }
            else
            {
                // 物件名必須
                if (string.IsNullOrEmpty(RentLivingEdit.Name))
                    return false;
                else
                    return true;
            }
        }
        public void RentLivingEditSelectedEditUpdateCommand_Execute()
        {
            // TODO: 入力チェック

            if (RentLivingEditSelectedItem != null)
            {

                // 写真画像の追加があるかどうかチェック
                bool isPicturePresent = false;
                if (!string.IsNullOrEmpty(RentLivingEdit.PictureFilePath))
                    isPicturePresent = true;

                if (isPicturePresent)
                {
                    string fileName = RentLivingEdit.PictureFilePath.Trim();

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
                        System.Drawing.Image thumbImg = img.GetThumbnailImage(200, 150, () => false, IntPtr.Zero);
                        byte[] ImageThumbData = ImageToByteArray(thumbImg);

                        fs.Close();

                        RentLivingEdit.PictureData = ImageData;
                        RentLivingEdit.PictureThumbW200xData = ImageThumbData;
                        RentLivingEdit.PictureFileExt = fi.Extension;

                        RentLivingEdit.PictureFilePath = "";
                    }
                    else
                    {
                        isPicturePresent = false;

                        // エラーイベント発火
                        MyError er = new MyError();
                        er.ErrType = "File";
                        er.ErrCode = 0;
                        er.ErrText = "「" + "File Does Not Exist" + "」";
                        er.ErrDescription = fileName + " ファイルが存在しません。";
                        er.ErrDatetime = DateTime.Now;
                        er.ErrPlace = "MainViewModel::RentLivingEditSelectedEditUpdateCommand_Execute()";
                        ErrorOccured?.Invoke(er);
                    }
                }


                // 編集オブジェクトに格納されている更新された情報をDBへ更新

                string sqlUpdateRent = String.Format("UPDATE Rent SET Name = '{1}', Type = '{2}', PostalCode = '{3}', Location = '{4}', TrainStation1 = '{5}', TrainStation2 = '{6}' WHERE Rent_ID = '{0}'",
                    RentLivingEdit.Rent_ID, RentLivingEdit.Name, RentLivingEdit.Type.ToString(), RentLivingEdit.PostalCode, RentLivingEdit.Location, RentLivingEdit.TrainStation1, RentLivingEdit.TrainStation2);

                string sqlUpdateRentLiving = String.Format("UPDATE RentLiving SET Kind = '{1}', Floors = '{2}', FloorsBasement = '{3}', BuiltYear = '{4}' WHERE RentLiving_ID = '{0}'",
                    RentLivingEdit.RentLiving_ID, RentLivingEdit.Kind.ToString(), RentLivingEdit.Floors, RentLivingEdit.FloorsBasement, RentLivingEdit.BuiltYear);

                string sqlUpdateRentLivingPicture = String.Format("UPDATE RentLivingPicture SET PictureData = @0, PictureThumbW200xData = @1, PictureFileExt = '{5}' WHERE RentLivingPicture_ID = '{0}'",
                    RentLivingEdit.RentLivingPicture_ID, RentLivingEdit.RentLiving_ID, RentLivingEdit.Rent_ID, RentLivingEdit.PictureData, RentLivingEdit.PictureThumbW200xData, RentLivingEdit.PictureFileExt);

                // TODO 

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
                                // TODO 
                            }

                            cmd.CommandText = sqlUpdateRentLiving;
                            result = cmd.ExecuteNonQuery();
                            if (result != 1)
                            {
                                // TODO
                            }

                            if (isPicturePresent)
                            {
                                // 物件画像の追加
                                cmd.CommandText = sqlUpdateRentLivingPicture;

                                SqliteParameter parameter1 = new SqliteParameter("@0", System.Data.DbType.Binary);
                                parameter1.Value = RentLivingEdit.PictureData;
                                cmd.Parameters.Add(parameter1);

                                SqliteParameter parameter2 = new SqliteParameter("@1", System.Data.DbType.Binary);
                                parameter2.Value = RentLivingEdit.PictureThumbW200xData;
                                cmd.Parameters.Add(parameter2);

                                result = cmd.ExecuteNonQuery();
                                if (result != 1)
                                {
                                    // これいる?
                                }
                            }


                            cmd.Transaction.Commit();

                            // 編集オブジェクトに格納された情報を、選択アイテムに更新（Listviewの情報が更新されるー＞DBからSelectして一覧を読み込みし直さなくて良くなる）
                            RentLivingEditSelectedItem.Name = RentLivingEdit.Name;
                            RentLivingEditSelectedItem.PostalCode = RentLivingEdit.PostalCode;
                            RentLivingEditSelectedItem.Location = RentLivingEdit.Location;
                            RentLivingEditSelectedItem.TrainStation1 = RentLivingEdit.TrainStation1;
                            RentLivingEditSelectedItem.TrainStation2 = RentLivingEdit.TrainStation2;

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
        }

        // 賃貸住居用物件、選択アイテム編集更新キャンセル
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

        // 賃貸住居用物件、選択アイテム表示(PDFとか)
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

        // 賃貸住居用物件、選択アイテム削除（DELETE）
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
                            if (_editRents.Remove(RentLivingEditSelectedItem))
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


        // エラー通知画面を閉じる
        public bool CloseErrorCommand_CanExecute()
        {
            return true;
        }
        public void CloseErrorCommand_Execute()
        {
            ShowErrorDialog = false;
        }

        

        #endregion

    }
}
