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
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using ZumenSearch.Models;
using ZumenSearch.ViewModels;
using ZumenSearch.ViewModels.Classes;
using ZumenSearch.Models.Classes;
using ZumenSearch.Views;
using ZumenSearch.Common;

/// ////////////////////////////////////////////////////////////
/// //////////////まずは小さく造って大きく育てる////////////////
/// ////////////////////////////////////////////////////////////

/// ■ TODO:
///
/// 区分所有かどうかで、図面の追加画面を変える。（利便性のため、表示用の一覧はして、追加削除はしない読み取り専用にする）
///
/// 登記情報のPDFを登録できるようにする。
/// 
/// エラー処理、及びログ保存
/// Modelsに基底クラス定義やデータ操作をリストラクチャー。

/// ● 履歴：
/// v0.0.0.13 もろもろ
/// v0.0.0.11 サイズ・レイアウト
/// 

/// ◆ 後で・検討中： 
/// WinUI 3.0が出たらツラを作り直す。
/// 〒・住所・Geo関連のデータを入力補完
/// XMLでインポート・エクスポート
/// RESTful API, Server, P2P and beyond...
/// 不動産IDが出来たら、Webサーバーを作り、会社内共有
/// （サーバー機能を追加して、同一ネットワーク内のローカルIPアドレスを指定してお互いに物件をフェッチし合えるようにする・・重複対策は・・・）
/// 技術的に対応できるならば、P2Pで会社間共有


namespace ZumenSearch.ViewModels
{

    public class MainViewModel : ViewModelBase
    {

        #region == 基本 ==

        // Application version.
        private const string _appVer = "0.0.0.13";

        // Application name.
        private const string _appName = "ZumenSearch";

        // Application config file folder name aka "publisher".
        private const string _appDeveloper = "torum";

        // Application Window Title.
        public string AppTitle
        {
            get
            {
                return _appName;
            }
        }

        public string AppTitleVer
        {
            get
            {
                return _appName + " v" + _appVer;
            }
        }

        #endregion

        #region == SQLite データベース ==

        // SQLite DB へのデータアクセスの共通モジュール
        private readonly DataAccess dataAccessModule = new DataAccess();

        public SqliteConnectionStringBuilder connectionStringBuilder;

        #endregion

        #region == 物件関連オブジェクト ==

        public ObservableCollection<RentLiving> Rents { get; } = new ObservableCollection<RentLiving>();

        #region == 賃貸住居用 ==

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

        #endregion

        #region == 元付け業者 ==

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

        #endregion

        #region == 管理会社 ==

        public ObservableCollection<MaintenanceCompany> MaintenanceCompanies { get; } = new ObservableCollection<MaintenanceCompany>();

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

        // 管理会社
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

        #endregion

        #region == オーナー ==

        public ObservableCollection<Owner> Owners { get; } = new ObservableCollection<Owner>();

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

        #endregion

        #region == 表示切替のフラグ ==

        // 賃貸住居用　検索結果のTabControlインデックス切り替え
        private int _rentLivingSearchTabSelectedIndex = 0;
        public int RentLivingSearchTabSelectedIndex
        {
            get
            {
                return _rentLivingSearchTabSelectedIndex;
            }
            set
            {
                if (_rentLivingSearchTabSelectedIndex == value) return;

                _rentLivingSearchTabSelectedIndex = value;
                this.NotifyPropertyChanged("RentLivingSearchTabSelectedIndex");
            }
        }

        // 削除予定 RL検索一覧・追加タブインデックス
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

        // 削除予定 RL検索一覧画面の表示フラグ 
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

        // 削除予定 RL新規追加画面の表示フラグ
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

        // 削除予定 RL編集画面の表示フラグ
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


        // 削除予定 RL新規物件の新規部屋タブインデックス
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

        // 削除予定 RL新規物件の部屋一覧画面の表示フラグ
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

        // 削除予定 RL新規物件の部屋新規追加画面の表示フラグ
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

        // 削除予定 RL新規物件の部屋編集画面の表示フラグ
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


        // 削除予定 RL編集物件の新規部屋タブインデックス
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

        // 削除予定 RL編集物件の部屋一覧画面の表示フラグ
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

        // 削除予定 RL編集物件の部屋新規追加画面の表示フラグ
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

        // 削除予定 RL編集物件の部屋編集画面の表示フラグ
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


        // 削除予定 元付け業者Agency 検索一覧・追加・編集タブインデックス
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

        // 削除予定 元付け業者Agency検索一覧画面の表示フラグ
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

        // 削除予定 元付け業者Agency新規追加画面の表示フラグ
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


        // 削除予定 管理会社 検索一覧・追加・編集タブインデックス
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

        // 削除予定 管理会社 検索一覧画面の表示フラグ
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

        // 削除予定 管理会社 新規追加画面の表示フラグ
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


        // 削除予定 オーナー 検索一覧・追加・編集タブインデックス
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

        // 削除予定 オーナー 検索一覧画面の表示フラグ
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

        // 削除予定 オーナー 新規追加画面の表示フラグ
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

        #region == ホーム画面 ==

        private RentSearchTags _selectedRentSearchTags = RentSearchTags.RentName;
        public RentSearchTags SelectedRentSearchTags
        {
            get
            {
                return _selectedRentSearchTags;
            }
            set
            {
                if (_selectedRentSearchTags == value)
                    return;

                _selectedRentSearchTags = value;
                NotifyPropertyChanged("SelectedRentSearchTags");
            }
        }

        private string _homeSearchText;
        public string HomeSearchText
        {
            get
            {
                return _homeSearchText;
            }
            set
            {
                if (_homeSearchText == value) return;

                _homeSearchText = value;
                this.NotifyPropertyChanged("HomeSearchText");
            }
        }

        #endregion

        //
        public event EventHandler<OpenRentLivingWindowEventArgs> OpenRentLivingWindow;

        public MainViewModel()
        {
            #region == コマンドのイニシャライズ ==

            HomeSearchExecCommand = new RelayCommand(HomeSearchExecCommand_Execute, HomeSearchExecCommand_CanExecute);

            // RL 管理検索
            RentLivingEditSearchCommand = new RelayCommand(RentLivingEditSearchCommand_Execute, RentLivingEditSearchCommand_CanExecute);
            // RL 管理一覧
            RentLivingEditListCommand = new RelayCommand(RentLivingEditListCommand_Execute, RentLivingEditListCommand_CanExecute);
            // RL 検索条件画面に戻る
            RentLivingSearchBackCommand = new RelayCommand(RentLivingSearchBackCommand_Execute, RentLivingSearchBackCommand_CanExecute);

            // RL 管理一覧選択表示
            RentLivingEditSelectedViewCommand = new RelayCommand(RentLivingEditSelectedViewCommand_Execute, RentLivingEditSelectedViewCommand_CanExecute);
            // RL 管理一覧選択削除
            RentLivingEditSelectedDeleteCommand = new RelayCommand(RentLivingEditSelectedDeleteCommand_Execute, RentLivingEditSelectedDeleteCommand_CanExecute);


            // RL 管理新規物件
            RentLivingNewCommand = new RelayCommand(RentLivingNewCommand_Execute, RentLivingNewCommand_CanExecute);
            RentLivingNewAddCommand = new RelayCommand(RentLivingNewAddCommand_Execute, RentLivingNewAddCommand_CanExecute);

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
            
            //
            //RentLivingEditSelectedEditUpdateCommand = new RelayCommand(RentLivingEditSelectedEditUpdateCommand_Execute, RentLivingEditSelectedEditUpdateCommand_CanExecute);




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

            var dataBaseFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + _appName + ".db";
            
            dataAccessModule.InitializeDatabase(dataBaseFilePath);

            #endregion

            #region == イベントへのサブスクライブ ==

            ErrorOccured += new MyErrorEvent(OnError);

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

            #region == 編集ウィンドウを閉じる ==

            // TODO: When MainWindow try to close itself, confirm to close all the child windows. 

            App app = App.Current as App;

            // Now, use "app.Windows" this time.
            foreach (var w in app.Windows)
            {
                if (w is RentLivingSectionWindow)
                {
                    if ((w as RentLivingSectionWindow).DataContext == null)
                        continue;

                    if (!((w as RentLivingSectionWindow).DataContext is RentLivingSectionViewModel))
                        continue;

                    (w as RentLivingSectionWindow).Close();
                }
            }

            foreach (var w in app.Windows)
            {
                if (w is RentLivingWindow)
                {
                    if ((w as RentLivingWindow).DataContext == null)
                        continue;

                    if (!((w as RentLivingWindow).DataContext is RentLivingViewModel))
                        continue;

                    (w as RentLivingWindow).Close();
                }

            }

            #endregion

            #region == ウィンドウ関連 ==
            /*
            if (sender is Window)
            {
                if ((sender as Window).WindowState == WindowState.Normal)
                {
                    Properties.Settings.Default.MainWindow_Left = (sender as Window).Left;
                    Properties.Settings.Default.MainWindow_Top = (sender as Window).Top;
                    Properties.Settings.Default.MainWindow_Height = (sender as Window).Height;
                    Properties.Settings.Default.MainWindow_Width = (sender as Window).Width;
                }

                Properties.Settings.Default.Save();
            }
            */

            #endregion

        }

        #region == エラーイベント ==

        public delegate void MyErrorEvent(MyError err);
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

        #region == コマンド ==

        #region == ホーム画面 ==

        public ICommand HomeSearchExecCommand { get; }
        public bool HomeSearchExecCommand_CanExecute()
        {
            return true;
        }
        public void HomeSearchExecCommand_Execute()
        {
            //
        }

        #endregion

        #region == 賃貸住居用 物件管理 ==

        // 検索
        public ICommand RentLivingEditSearchCommand { get; }
        public bool RentLivingEditSearchCommand_CanExecute()
        {
            // TODO
            if (String.IsNullOrEmpty(RentLivingEditSearchText))
            {
                //return false;
            }
            else
            {
                //return true;
            }
            return true;
        }
        public void RentLivingEditSearchCommand_Execute()
        {
            // 一覧をクリア
            Rents.Clear();

            // 検索結果一覧タブに移動
            RentLivingSearchTabSelectedIndex = 1;

            // 検索結果一覧を取得
            dataAccessModule.GetSearchResultOfRentLiving(Rents, RentLivingEditSearchText);

        }

        // 一覧
        public ICommand RentLivingEditListCommand { get; }
        public bool RentLivingEditListCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingEditListCommand_Execute()
        {
            // 一覧をクリア
            Rents.Clear();

            // 検索結果一覧タブに移動
            RentLivingSearchTabSelectedIndex = 1;

            // 一覧を取得
            dataAccessModule.GetListOfRentLiving(Rents);

            //TODO: エラー取得して、エラーを通知。
        }

        // 検索へ戻る
        public ICommand RentLivingSearchBackCommand { get; }
        public bool RentLivingSearchBackCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingSearchBackCommand_Execute()
        {
            // 検索結果一覧アイテムの選択保持変数をクリア・解除
            RentLivingEditSelectedItem = null;

            // 検索タブに移動
            RentLivingSearchTabSelectedIndex = 0;
        }

        // 新規追加（編集画面表示）
        public ICommand RentLivingNewCommand { get; }
        public bool RentLivingNewCommand_CanExecute()
        {
            return true;
        }
        public void RentLivingNewCommand_Execute()
        {
            // 編集用の賃貸住居用物件オブジェクトを用意
            RentLiving rl = new RentLiving(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
            {
                IsNew = true
            };

            // 編集画面に渡すEventArgsを用意
            OpenRentLivingWindowEventArgs ag = new OpenRentLivingWindowEventArgs
            {
                Id = rl.RentLivingId,
                EditObject = rl,
                DataAccessModule = this.dataAccessModule
            };

            // 画面表示イベント発火
            OpenRentLivingWindow?.Invoke(this, ag);
        }

        // 編集（編集画面表示）
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
            if (RentLivingEditSelectedItem == null)
                return;

            // 編集用の賃貸住居用物件オブジェクトを取得
            RentLiving rl = dataAccessModule.GetRentLivingById(RentLivingEditSelectedItem.RentId, RentLivingEditSelectedItem.RentLivingId);

            //TODO:エラーの通知

            // 編集画面に渡すEventArgsを用意
            OpenRentLivingWindowEventArgs ag = new OpenRentLivingWindowEventArgs
            {
                Id = rl.RentLivingId,
                EditObject = rl,
                DataAccessModule = dataAccessModule
            };

            // 画面表示イベント発火
            OpenRentLivingWindow?.Invoke(this, ag);
        }

        // 表示(PDFとか)
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

        // 削除（DELETE）
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
                // DBから削除
                dataAccessModule.DeleteRentLiving(RentLivingEditSelectedItem.RentId);

                //TODO: エラーのトラップと通知。

                // 一覧からも削除
                if (Rents.Remove(RentLivingEditSelectedItem))
                {
                    RentLivingEditSelectedItem = null;
                }
            }
        }

        #endregion

        #region == RL 新規物件 削除予定 ==

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
                    RentLivingNew.RentId, RentLivingNew.Name, RentLivingNew.Type.ToString(), RentLivingNew.PostalCode, RentLivingNew.Location, RentLivingNew.TrainStation1, RentLivingNew.TrainStation2);

                string sqlInsertIntoRentLiving = String.Format("INSERT INTO RentLiving (RentLiving_ID, Rent_ID, Kind, Floors, FloorsBasement, BuiltYear) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')",
                    RentLivingNew.RentLivingId, RentLivingNew.RentId, RentLivingNew.Kind.ToString(), RentLivingNew.Floors, RentLivingNew.FloorsBasement, RentLivingNew.BuiltYear);

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
                                            pic.RentPictureId, RentLivingNew.RentLivingId, RentLivingNew.RentId, pic.PictureData, pic.PictureThumbW200xData, pic.PictureFileExt);

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
                                        pdf.RentZumenPdfId, RentLivingNew.RentLivingId, RentLivingNew.RentId, pdf.PDFData, pdf.DateTimeAdded.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

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
                                            room.RentLivingSectionId, RentLivingNew.RentLivingId, RentLivingNew.RentId, room.RentLivingSectionRoomNumber, room.RentLivingSectionPrice, room.RentLivingSectionMadori);

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
                                                roompic.RentSectionPictureId, roompic.RentLivingSectionId, RentLivingNew.RentLivingId, RentLivingNew.RentId, roompic.PictureData, roompic.PictureThumbW200xData, roompic.PictureFileExt);

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

                            ImageData = Methods.ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = Methods.FixedSize(img, 200, 150);
                            byte[] ImageThumbData = Methods.ImageToByteArray(thumbImg);


                            RentLivingPicture rlpic = new RentLivingPicture(RentLivingNew.RentId, RentLivingNew.RentLivingId, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;
                            rlpic.IsModified = false;
                            rlpic.IsNew = true;

                            rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

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
            RentLivingNewSectionNew = new RentLivingSection(RentLivingNew.RentId, RentLivingNew.RentLivingId, Guid.NewGuid().ToString());
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
            RentLivingNewSectionNew = new RentLivingSection(RentLivingNew.RentId, RentLivingNew.RentLivingId, Guid.NewGuid().ToString());
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

                            ImageData = Methods.ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = Methods.FixedSize(img, 200, 150);
                            byte[] ImageThumbData = Methods.ImageToByteArray(thumbImg);


                            RentLivingSectionPicture rlpic = new RentLivingSectionPicture(RentLivingNewSectionNew.RentId, RentLivingNewSectionNew.RentLivingId, RentLivingNewSectionNew.RentLivingSectionId, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsNew = true;
                            rlpic.IsModified = false;

                            rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

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

                            ImageData = Methods.ImageToByteArray(img);

                            // サムネイル画像の作成
                            System.Drawing.Image thumbImg = Methods.FixedSize(img, 200, 150);
                            byte[] ImageThumbData = Methods.ImageToByteArray(thumbImg);


                            RentLivingSectionPicture rlpic = new RentLivingSectionPicture(RentLivingNewSectionEdit.RentId, RentLivingNewSectionEdit.RentLivingId, RentLivingNewSectionEdit.RentLivingSectionId, Guid.NewGuid().ToString());
                            rlpic.PictureData = ImageData;
                            rlpic.PictureThumbW200xData = ImageThumbData;
                            rlpic.PictureFileExt = fi.Extension;

                            rlpic.IsNew = true;
                            rlpic.IsModified = false;

                            rlpic.Picture = Methods.BitmapImageFromImage(thumbImg, Methods.FileExtToImageFormat(rlpic.PictureFileExt));

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

                    RentLivingZumenPDF rlZumen = new RentLivingZumenPDF(RentLivingNew.RentId, RentLivingNew.RentLivingId, Guid.NewGuid().ToString());
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
                RentLivingEdit.RentId, RentLivingEdit.Name, RentLivingEdit.Type.ToString(), RentLivingEdit.PostalCode, RentLivingEdit.Location, RentLivingEdit.TrainStation1, RentLivingEdit.TrainStation2);

            string sqlUpdateRentLiving = String.Format("UPDATE RentLiving SET Kind = '{1}', Floors = '{2}', FloorsBasement = '{3}', BuiltYear = '{4}' WHERE RentLiving_ID = '{0}'",
                RentLivingEdit.RentLivingId, RentLivingEdit.Kind.ToString(), RentLivingEdit.Floors, RentLivingEdit.FloorsBasement, RentLivingEdit.BuiltYear);

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
                                        pic.RentPictureId, RentLivingEdit.RentLivingId, RentLivingEdit.RentId, pic.PictureData, pic.PictureThumbW200xData, pic.PictureFileExt);

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
                                        pic.RentPictureId, RentLivingEdit.RentLivingId, RentLivingEdit.RentId, pic.PictureData, pic.PictureThumbW200xData, pic.PictureFileExt);

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
                                        pdf.RentZumenPdfId, RentLivingEdit.RentLivingId, RentLivingEdit.RentId, pdf.PDFData, pdf.DateTimeAdded.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

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
                                        pdf.RentZumenPdfId, pdf.DateTimePublished.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.DateTimeVerified.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"), pdf.FileSize);

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
                                            room.RentLivingSectionId, RentLivingEdit.RentLivingId, RentLivingEdit.RentId, room.RentLivingSectionRoomNumber, room.RentLivingSectionPrice, room.RentLivingSectionMadori);

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
                                            room.RentLivingSectionId, room.RentLivingSectionRoomNumber, room.RentLivingSectionPrice, room.RentLivingSectionMadori);

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
                                                roompic.RentSectionPictureId, roompic.RentLivingSectionId, roompic.RentLivingId, roompic.RentId, roompic.PictureData, roompic.PictureThumbW200xData, roompic.PictureFileExt);

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
                                                roompic.RentSectionPictureId, roompic.RentLivingSectionId, roompic.RentLivingId, roompic.RentId, roompic.PictureData, roompic.PictureThumbW200xData, roompic.PictureFileExt);

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


    }


    #endregion

}
