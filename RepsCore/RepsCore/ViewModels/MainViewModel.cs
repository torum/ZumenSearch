using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
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
using System.IO.Enumeration;
using System.Windows.Media.Effects;
using System.Windows.Documents;

/// //////////////////////////////////////////////////////////
/// //////////////小さく造って大きく育てる！//////////////////
/// //////////////////////////////////////////////////////////

/// TODO:
/// 部屋（複数）を追加・編集・削除
/// 条件検索
/// 建物にPDFのファイルを追加
/// 入力補助（住所・〒・Geo）
/// 入力チェック
/// 元付け・オーナー管理
/// エラー処理、及びログ保存
/// 

/// 後で：
/// 画像データのビットマップ形式へ統一？


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
    /// 賃貸物件のRent・Section等、及び派生クラス
    /// </summary>
    #region == Rent・Section等、及びその派生クラス ==

    /// <summary>
    /// 物件の基底クラス
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
    /// 賃貸住居用物件のクラス（建物）
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
        
        // 物件写真一覧
        public ObservableCollection<RentLivingPicture> RentLivingPictures { get; set; } = new ObservableCollection<RentLivingPicture>();

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

        // 部屋一覧
        public ObservableCollection<RentLivingSection> RentLivingSections { get; set; } = new ObservableCollection<RentLivingSection>();

        public RentLiving (string rentid, string rentlivingid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;

            this._type = RentTypes.RentLiving;
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
        public bool IsVacant {
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
    }

    /// <summary>
    /// 賃貸住居用物件のクラス（部屋）
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

        public RentLivingSection(string rentid, string rentlivingid, string sectionid)
        {
            this._rent_ID = rentid;
            this._rentLiving_ID = rentlivingid;
            this._rentLivingSection_ID = sectionid;
        }
    }

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

        protected string _rentLiving_id;
        public string RentLiving_ID
        {
            get
            {
                return _rentLiving_id;
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

        public bool IsModified { get; set; }

        public bool IsNew { get; set; }

    }

    public class RentLivingPicture : RentPicture
    {
        public RentLivingPicture(string rentid, string rentlivingid, string rentlivingpictureid)
        {
            this._rent_id = rentid;
            this._rentLiving_id = rentlivingid;

            this._rentPicture_id = rentlivingpictureid;
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
    /// https://stackoverflow.com/questions/28707039/trying-to-understand-using-a-service-to-open-a-dialog?noredirect=1&lq=1
    /// </summary>
    #region == IO Dialog Serviceダイアログ表示用クラス ==

    /// TODO: サービスのインジェクションは・・・とりあえずしない。

    public interface IOpenDialogService
    {
        string[] GetOpenFileDialog(string title);
    }
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

        // 賃貸物件住居用　新規追加用のクラスオブジェクト
        private RentLiving _rentLivingNew;
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
        private RentLiving _rentLivingEdit;
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

        //private IOpenDialogService openDialogService;
        private OpenDialogService _openDialogService = new OpenDialogService();

        /// <summary>
        /// メインのビューモデルのコンストラクタ
        /// </summary>
        public MainViewModel()// (IOpenDialogService openDialogService)
        {
            // エラーイベントにサブスクライブ
            ErrorOccured += new MyErrorEvent(OnError);

            #region == コマンドのイニシャライズ ==

            // 賃貸住居用新規物件
            RentLivingNewCommand = new RelayCommand(RentLivingNewCommand_Execute, RentLivingNewCommand_CanExecute);
            RentLivingNewAddCommand = new RelayCommand(RentLivingNewAddCommand_Execute, RentLivingNewAddCommand_CanExecute);
            RentLivingNewCancelCommand = new RelayCommand(RentLivingNewCancelCommand_Execute, RentLivingNewCancelCommand_CanExecute);
            // 賃貸住居用新規画像追加
            RentLivingNewPictureAddCommand = new RelayCommand(RentLivingNewPictureAddCommand_Execute, RentLivingNewPictureAddCommand_CanExecute);
            RentLivingNewPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingNewPictureDeleteCommand_Execute(param),
                param => RentLivingNewPictureDeleteCommand_CanExecute());

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

            // 賃貸住居用管理編集画像追加
            RentLivingEditPictureAddCommand = new RelayCommand(RentLivingEditPictureAddCommand_Execute, RentLivingEditPictureAddCommand_CanExecute);
            RentLivingEditPictureDeleteCommand = new GenericRelayCommand<object>(
                param => RentLivingEditPictureDeleteCommand_Execute(param),
                param => RentLivingEditPictureDeleteCommand_CanExecute());
            RentLivingEditPictureChangeCommand = new GenericRelayCommand<object>(
                param => RentLivingEditPictureChangeCommand_Execute(param),
                param => RentLivingEditPictureChangeCommand_CanExecute());


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
                                "PostalCode TEXT," +
                                "Location TEXT," +
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

        // 新規賃貸住居用　物件追加（画面表示）
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

        // 新規賃貸住居用　物件追加キャンセル
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

        // 新規賃貸住居用　物件の画像追加
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

        // 新規賃貸住居用　物件の画像削除
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
                RentLivingNew.RentLivingPictures.Remove(item);
            }

        }

        // 新規賃貸住居用　物件追加処理(INSERT)
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

                            if (RentLivingNew.RentLivingPictures.Count > 0)
                            {
                                foreach (var pic in RentLivingNew.RentLivingPictures)
                                {
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

        // 新規賃貸住居用　物件の部屋追加（画面表示）
        public ICommand RentLivingNewSectionCommand { get; }
        public bool RentLivingNewSectionCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionCommand_Execute()
        {
            ShowRentLivingSectionNew = true;
        }

        // 新規賃貸住居用　物件の部屋追加処理 (INSERT)
        public ICommand RentLivingNewSectionAddCommand { get; }
        public bool RentLivingNewSectionAddCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionAddCommand_Execute()
        {

        }

        // 新規賃貸住居用　物件の部屋追加キャンセル
        public ICommand RentLivingNewSectionCancelCommand { get; }
        public bool RentLivingNewSectionCancelCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewSectionCancelCommand_Execute()
        {
            ShowRentLivingSectionNew = false;
        }

        // 賃貸住居用　物件管理、一覧
        public ICommand RentLivingEditListCommand { get; }
        public bool RentLivingEditListCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditListCommand_Execute()
        {
            // Firest, clear it.
            _editRents.Clear();

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

                                _editRents.Add(rl);
                                
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
                er.ErrPlace = "In " + e.Source +  " from MainViewModel::RentLivingEditListCommand_Execute()";
                ErrorOccured?.Invoke(er);
            }


        }

        // 賃貸住居用　物件管理、検索
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
            _editRents.Clear();

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

                            _editRents.Add(rl);

                        }
                    }
                }
            }
        }

        // 賃貸住居用　物件管理、選択アイテム編集（画面表示）
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

        // 賃貸住居用　物件管理、選択アイテム編集、更新キャンセル
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

        // 賃貸住居用　物件管理　物件画像追加
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
                            MyError er = new MyError();
                            er.ErrType = "File";
                            er.ErrCode = 0;
                            er.ErrText = "「" + "File Does Not Exist" + "」";
                            er.ErrDescription = fileName + " ファイルが存在しません。";
                            er.ErrDatetime = DateTime.Now;
                            er.ErrPlace = "MainViewModel::RentLivingEditAddCommand_Execute()";
                            ErrorOccured?.Invoke(er);
                        }
                    }
                }
            }
        }

        // 賃貸住居用　物件管理　物件画像削除
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
                RentLivingEdit.RentLivingPictures.Remove(item);
            }

        }

        // 賃貸住居用　物件管理　物件画像差し替え更新
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


        // 賃貸住居用　物件管理、選択編集、選択アイテム編集更新（UPDATE）
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

            // TODO: 入力チェック

            if (RentLivingEditSelectedItem != null)
            {

                // 編集オブジェクトに格納されている更新された情報をDBへ更新

                string sqlUpdateRent = String.Format("UPDATE Rent SET Name = '{1}', Type = '{2}', PostalCode = '{3}', Location = '{4}', TrainStation1 = '{5}', TrainStation2 = '{6}' WHERE Rent_ID = '{0}'",
                    RentLivingEdit.Rent_ID, RentLivingEdit.Name, RentLivingEdit.Type.ToString(), RentLivingEdit.PostalCode, RentLivingEdit.Location, RentLivingEdit.TrainStation1, RentLivingEdit.TrainStation2);

                string sqlUpdateRentLiving = String.Format("UPDATE RentLiving SET Kind = '{1}', Floors = '{2}', FloorsBasement = '{3}', BuiltYear = '{4}' WHERE RentLiving_ID = '{0}'",
                    RentLivingEdit.RentLiving_ID, RentLivingEdit.Kind.ToString(), RentLivingEdit.Floors, RentLivingEdit.FloorsBasement, RentLivingEdit.BuiltYear);


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

                            if (RentLivingEdit.RentLivingPictures.Count > 0)
                            {
                                foreach (var pic in RentLivingEdit.RentLivingPictures)
                                {
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
        }

        // 賃貸住居用　物件管理、選択編集、選択アイテム表示(PDFとか)
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

        // 賃貸住居用　物件管理、選択編集、選択アイテム削除（DELETE）
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

    }
}
